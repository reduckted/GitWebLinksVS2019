Imports LibGit2Sharp
Imports System.ComponentModel.Composition
Imports System.IO
Imports System.Text.RegularExpressions


<Export(GetType(ILinkHandler))>
Public Class GitHubHandler
    Implements ILinkHandler


    Private Shared ReadOnly RemotePattern As New Regex("^(?:git@github\.com:|https:\/\/github\.com\/)(.+)\.git$")


    Public ReadOnly Property Name As String _
        Implements ILinkHandler.Name

        Get
            Return "GitHub"
        End Get
    End Property


    Public Function IsMatch(remoteUrl As String) As Boolean _
        Implements ILinkHandler.IsMatch

        Return RemotePattern.IsMatch(remoteUrl)
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
        repositoryPath = RemotePattern.Match(gitInfo.RemoteUrl).Groups(1).Value

        relativePathToFile = filePath.Substring(gitInfo.RootDirectory.Length).Replace("\", "/").Trim("/"c)

        ' Get the current branch name. The remote branch might not be the same,
        ' but it's better than using a commit hash which won't match anything on
        ' the remote if there are commits to this branch on the local repository.
        Using repository As New Repository(gitInfo.RootDirectory)
            branch = repository.Head.FriendlyName
        End Using

        url = String.Join("/", {
            "https://github.com",
            repositoryPath,
            "blob",
            Uri.EscapeUriString(branch),
            Uri.EscapeUriString(relativePathToFile)
        })

        If selection IsNot Nothing Then
            url &= $"#L{selection.StartLineNumber}"

            If selection.StartLineNumber <> selection.EndLineNumber Then
                url &= $"-L{selection.EndLineNumber}"
            End If
        End If

        Return url
    End Function

End Class
