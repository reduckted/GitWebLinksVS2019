Imports LibGit2Sharp


Public MustInherit Class LinkHandlerBase
    Implements ILinkHandler


    Public MustOverride ReadOnly Property Name As String _
        Implements ILinkHandler.Name


    Public Function IsMatch(remoteUrl As String) As Boolean _
        Implements ILinkHandler.IsMatch

        Return GetMatchingServerUrl(remoteUrl) IsNot Nothing
    End Function


    Public Function MakeUrl(
            gitInfo As GitInfo,
            filePath As String,
            selection As LineSelection
        ) As String _
        Implements ILinkHandler.MakeUrl

        Dim url As String
        Dim repositoryPath As String
        Dim branch As String
        Dim relativePathToFile As String


        ' Get the repository's path out of the remote URL.
        repositoryPath = GetRepositoryPath(
            gitInfo.RemoteUrl,
            GetMatchingServerUrl(gitInfo.RemoteUrl)
        )

        relativePathToFile = filePath.Substring(gitInfo.RootDirectory.Length).Replace("\", "/").Trim("/"c)

        ' Get the current branch name. The remote branch might not be the same,
        ' but it's better than using a commit hash which won't match anything on
        ' the remote if there are commits to this branch on the local repository.
        Using repository As New Repository(gitInfo.RootDirectory)
            branch = repository.Head.FriendlyName
        End Using

        url = CreateUrl(
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
        ) As String

        Return GetServerUrls().FirstOrDefault(Function(x) remoteUrl.StartsWith(x))
    End Function


    Protected MustOverride Function GetServerUrls() As IEnumerable(Of String)


    Private Function GetRepositoryPath(
            remoteUrl As String,
            matchingServer As String
        ) As String

        Dim path As String


        path = remoteUrl.Substring(matchingServer.Length)

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


    Protected MustOverride Function CreateUrl(
            repositoryPath As String,
            branch As String,
            relativePathToFile As String
        ) As String


    Protected MustOverride Function GetSelectionHash(
            filePath As String,
            selection As LineSelection
        ) As String

End Class
