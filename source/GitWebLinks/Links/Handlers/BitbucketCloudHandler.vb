Imports LibGit2Sharp
Imports System.ComponentModel.Composition


<Export(GetType(ILinkHandler))>
Public Class BitbucketCloudHandler
    Inherits LinkHandlerBase


    Private Shared ReadOnly ServerUrls As IEnumerable(Of ServerUrl) = {
        New ServerUrl("https://bitbucket.org", "git@bitbucket.org")
    }


    <ImportingConstructor()>
    Public Sub New(options As IOptions)
        MyBase.New(options)
    End Sub


    Public Overrides ReadOnly Property Name As String
        Get
            Return "Bitbucket"
        End Get
    End Property


    Protected Overrides Function GetServerUrls() As IEnumerable(Of ServerUrl)
        Return ServerUrls
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
            "src",
            Uri.EscapeUriString(branchOrHash),
            Uri.EscapeUriString(relativePathToFile)
        })
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
