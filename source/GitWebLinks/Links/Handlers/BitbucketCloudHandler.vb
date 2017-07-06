Imports System.ComponentModel.Composition


<Export(GetType(ILinkHandler))>
Public Class BitbucketCloudHandler
    Inherits LinkHandlerBase


    Private ReadOnly cgOptions As IOptions


    <ImportingConstructor()>
    Public Sub New(options As IOptions)
        cgOptions = options
    End Sub


    Public Overrides ReadOnly Property Name As String
        Get
            Return "Bitbucket"
        End Get
    End Property


    Protected Overrides Function GetServerUrls() As IEnumerable(Of String)
        Return cgOptions.BitbucketCloudUrls
    End Function


    Protected Overrides Function CreateUrl(
            repositoryPath As String,
            branch As String,
            relativePathToFile As String
        ) As String

        Return String.Join("/", {"https://bitbucket.org", repositoryPath, "src", branch, relativePathToFile})
    End Function


    Protected Overrides Function GetSelectionHash(
            filePath As String,
            selection As LineSelection
        ) As String

        Dim hash As String


        hash = $"#{Uri.EscapeUriString(IO.Path.GetFileName(filePath))}-{selection.StartLineNumber}"

        If selection.StartLineNumber <> selection.EndLineNumber Then
            hash &= $":{selection.EndLineNumber}"
        End If

        Return hash
    End Function

End Class
