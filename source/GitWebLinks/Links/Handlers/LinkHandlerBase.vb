Imports LibGit2Sharp
Imports System.Text.RegularExpressions


Public MustInherit Class LinkHandlerBase
    Implements ILinkHandler


    Private Const SshPrefix As String = "ssh://"


    Private Shared ReadOnly UsernamePattern As New Regex("(?<scheme>https?://)[^@]+@(?<address>.+)")


    Public MustOverride ReadOnly Property Name As String _
        Implements ILinkHandler.Name


    Public Function IsMatch(remoteUrl As String) As Boolean _
        Implements ILinkHandler.IsMatch

        Return GetMatchingServerUrl(FixRemoteUrl(remoteUrl)) IsNot Nothing
    End Function


    Public Function MakeUrl(
            gitInfo As GitInfo,
            filePath As String,
            selection As LineSelection
        ) As String _
        Implements ILinkHandler.MakeUrl

        Dim url As String
        Dim fixedRemoteUrl As String
        Dim server As ServerUrl
        Dim repositoryPath As String
        Dim relativePathToFile As String
        Dim branch As String
        Dim baseUrl As String


        fixedRemoteUrl = FixRemoteUrl(gitInfo.RemoteUrl)
        server = GetMatchingServerUrl(fixedRemoteUrl)

        ' Get the repository's path out of the remote URL.
        repositoryPath = GetRepositoryPath(fixedRemoteUrl, server)

        relativePathToFile = filePath.Substring(gitInfo.RootDirectory.Length).Replace("\", "/").Trim("/"c)

        ' Get the current branch name. The remote branch might not be the same,
        ' but it's better than using a commit hash which won't match anything on
        ' the remote if there are commits to this branch on the local repository.
        Using repository As New Repository(gitInfo.RootDirectory)
            branch = GetBranchName(repository.Head)
        End Using

        baseUrl = server.BaseUrl

        If baseUrl.EndsWith("/"c) Then
            baseUrl = baseUrl.Substring(0, baseUrl.Length - 1)
        End If

        url = CreateUrl(
            baseUrl,
            repositoryPath,
            Uri.EscapeUriString(branch),
            Uri.EscapeUriString(relativePathToFile)
        )

        If selection IsNot Nothing Then
            url &= GetSelectionHash(filePath, selection)
        End If

        Return url
    End Function


    Private Function GetMatchingServerUrl(
            remoteUrl As String
        ) As ServerUrl

        Return (
            From server In GetServerUrls()
            Where remoteUrl.StartsWith(server.BaseUrl, StringComparison.Ordinal) _
            OrElse remoteUrl.StartsWith(server.SshUrl, StringComparison.Ordinal)
        ).FirstOrDefault()
    End Function


    Private Function FixRemoteUrl(remoteUrl As String) As String
        If remoteUrl.StartsWith(SshPrefix, StringComparison.Ordinal) Then
            ' Remove the SSH prefix.
            remoteUrl = remoteUrl.Substring(SshPrefix.Length)

        Else
            Dim match As Match


            ' This will be an HTTP address. Check if there's 
            ' a username in the URL And if there Is, remove it.
            match = UsernamePattern.Match(remoteUrl)

            If match.Success Then
                remoteUrl = match.Groups("scheme").Value & match.Groups("address").Value
            End If
        End If

        Return remoteUrl
    End Function


    Protected MustOverride Function GetServerUrls() As IEnumerable(Of ServerUrl)


    Private Function GetRepositoryPath(
            remoteUrl As String,
            matchingServer As ServerUrl
        ) As String

        Dim path As String


        If remoteUrl.StartsWith(matchingServer.BaseUrl, StringComparison.Ordinal) Then
            path = remoteUrl.Substring(matchingServer.BaseUrl.Length)
        Else
            path = remoteUrl.Substring(matchingServer.SshUrl.Length)
        End If

        ' The server URL we matched against may not have ended 
        ' with a slash (For HTTPS paths) or a colon (For Git paths), 
        ' which means the path might start with that. Trim that off now.
        If path.Length > 0 Then
            If (path(0) = "/"c) OrElse (path(0) = ":"c) Then
                path = path.Substring(1)
            End If
        End If

        If path.EndsWith(".git", StringComparison.OrdinalIgnoreCase) Then
            path = path.Substring(0, path.Length - 4)
        End If

        Return path
    End Function


    Protected MustOverride Function GetBranchName(branch As Branch) As String


    Protected MustOverride Function CreateUrl(
            baseUrl As String,
            repositoryPath As String,
            branch As String,
            relativePathToFile As String
        ) As String


    Protected MustOverride Function GetSelectionHash(
            filePath As String,
            selection As LineSelection
        ) As String

End Class
