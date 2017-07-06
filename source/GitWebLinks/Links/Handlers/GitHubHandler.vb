Imports System.ComponentModel.Composition


<Export(GetType(ILinkHandler))>
Public Class GitHubHandler
    Inherits LinkHandlerBase


    Private ReadOnly cgOptions As IOptions


    <ImportingConstructor()>
    Public Sub New(options As IOptions)
        cgOptions = options
    End Sub


    Public Overrides ReadOnly Property Name As String
        Get
            Return "GitHub"
        End Get
    End Property


    Protected Overrides Function GetServerUrls() As IEnumerable(Of String)
        Return cgOptions.GitHubUrls
    End Function


    Protected Overrides Function CreateUrl(
            repositoryPath As String,
            branch As String,
            relativePathToFile As String
        ) As String

        Return String.Join("/", {"https://github.com", repositoryPath, "blob", branch, relativePathToFile})
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
