Imports LibGit2Sharp
Imports System.IO
Imports System.Text.RegularExpressions


Public MustInherit Class LinkHandlerBase
    Implements ILinkHandler


    Private Const SshPrefix As String = "ssh://"


    Private Shared ReadOnly UsernamePattern As New Regex("(?<scheme>https?://)[^@]+@(?<address>.+)")


    Protected Sub New(options As IOptions)
        Me.Options = options
    End Sub


    Protected ReadOnly Property Options As IOptions


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
        Dim branchOrHash As String
        Dim baseUrl As String


        ' Sometimes Visual Studio gives us the file path in lowercase.
        ' No idea why it does that, but we need to correct it because
        ' the case-mismatched names don't play well with the Git websites.
        filePath = FixFilePathCasing(filePath)

        fixedRemoteUrl = FixRemoteUrl(gitInfo.RemoteUrl)
        server = GetMatchingServerUrl(fixedRemoteUrl)

        ' Get the repository's path out of the remote URL.
        repositoryPath = GetRepositoryPath(fixedRemoteUrl, server)

        relativePathToFile = filePath.Substring(gitInfo.RootDirectory.Length).Replace("\", "/").Trim("/"c)

        ' Get the current branch name or commit SHA
        ' depending on what type of link we need to create.
        Using repository As New Repository(gitInfo.RootDirectory)
            If Options.LinkType = LinkType.Branch Then
                branchOrHash = GetBranchName(repository.Head)
            Else
                branchOrHash = repository.Head.Tip.Sha
            End If
        End Using

        baseUrl = server.BaseUrl

        If baseUrl.EndsWith("/"c) Then
            baseUrl = baseUrl.Substring(0, baseUrl.Length - 1)
        End If

        url = CreateUrl(
            baseUrl,
            repositoryPath,
            branchOrHash,
            relativePathToFile
        )

        If selection IsNot Nothing Then
            url &= GetSelectionHash(filePath, selection)
        End If

        Return url
    End Function


    Private Function FixFilePathCasing(filePath As String) As String
        Dim dir As DirectoryInfo


        ' Adpated from: https://stackoverflow.com/a/326153/4397397

        If (Not File.Exists(filePath)) AndAlso (Not Directory.Exists(filePath)) Then
            Return filePath
        End If

        dir = New DirectoryInfo(filePath)

        ' If there's no parent, then this is the drive. We don't care
        ' about the drive because the drive is always above the repository
        ' root. But for the sake of consistency, we will make it uppercase.
        If dir.Parent Is Nothing Then
            Return dir.Name.ToUpper()
        End If

        ' Fix the parent path and get the correct name of the file.
        Return Path.Combine(
            FixFilePathCasing(dir.Parent.FullName),
            dir.Parent.GetFileSystemInfos(dir.Name)(0).Name
        )
    End Function


    Protected Overridable Function GetMatchingServerUrl(
            remoteUrl As String
        ) As ServerUrl

        Return (
            From server In GetServerUrls()
            Where remoteUrl.StartsWith(server.BaseUrl, StringComparison.Ordinal) _
            OrElse remoteUrl.StartsWith(server.SshUrl, StringComparison.Ordinal)
        ).FirstOrDefault()
    End Function


    Protected Overridable Function GetServerUrls() As IEnumerable(Of ServerUrl)
        ' Derived classes should override this if they don't overrides `GetMatchingServerUrl`.
        Throw New NotSupportedException()
    End Function


    Private Function FixRemoteUrl(remoteUrl As String) As String
        If remoteUrl.StartsWith(SshPrefix, StringComparison.Ordinal) Then
            ' Remove the SSH prefix.
            remoteUrl = remoteUrl.Substring(SshPrefix.Length)

        Else
            Dim match As Match


            ' This will be an HTTP address. Check if there's 
            ' a username in the URL And if there is, remove it.
            match = UsernamePattern.Match(remoteUrl)

            If match.Success Then
                remoteUrl = match.Groups("scheme").Value & match.Groups("address").Value
            End If
        End If

        Return remoteUrl
    End Function


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
        ' with a slash (for HTTPS paths) or a colon (for Git paths), 
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
            branchOrHash As String,
            relativePathToFile As String
        ) As String


    Protected MustOverride Function GetSelectionHash(
            filePath As String,
            selection As LineSelection
        ) As String

End Class
