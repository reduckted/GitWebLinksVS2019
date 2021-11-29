Imports LibGit2Sharp


Public Class GitHubHandler
    Inherits LinkHandlerBase


    Private Shared ReadOnly GitHubUrl As New ServerUrl("https://github.com", "git@github.com")


    Public Overrides ReadOnly Property Name As String
        Get
            Return "GitHub"
        End Get
    End Property


    Protected Overrides Iterator Function GetServerUrls() As IEnumerable(Of ServerUrl)
        Yield GitHubUrl

        For Each server In Options.GitHubEnterpriseUrls
            Yield server
        Next server
    End Function


    Protected Overrides Function GetBranchName(branch As Branch) As String
        Return branch.FriendlyName
    End Function


    Protected Overrides Function CreateUrl(
            baseUrl As String,
            repositoryPath As String,
            branchOrHash As String,
            relativePathToFile As String
        ) As String

        Return String.Join("/", {
            baseUrl,
            repositoryPath,
            "blob",
            Uri.EscapeUriString(branchOrHash),
            Uri.EscapeUriString(relativePathToFile)
        })
    End Function


    Protected Overrides Function GetSelectionHash(
            filePath As String,
            selection As LineSelection
        ) As String

        Dim hash As String


        hash = $"#L{selection.StartLineNumber}"

        If selection.StartLineNumber <> selection.EndLineNumber Then
            hash &= $"-L{selection.EndLineNumber}"
        End If

        Return hash
    End Function

End Class
