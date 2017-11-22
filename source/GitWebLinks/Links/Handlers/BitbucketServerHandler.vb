Imports LibGit2Sharp
Imports System.ComponentModel.Composition
Imports System.Text.RegularExpressions

<Export(GetType(ILinkHandler))>
Public Class BitbucketServerHandler
    Inherits LinkHandlerBase


    Private Shared ReadOnly ProjectPattern As New Regex("(?<project>[^\/]+)\/(?<repo>[^\/]+)$")


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
        Return Options.BitbucketServerUrls
    End Function


    Protected Overrides Function GetBranchName(branch As Branch) As String
        Return branch.CanonicalName
    End Function


    Protected Overrides Function CreateUrl(
            baseUrl As String,
            repositoryPath As String,
            branchOrHash As String,
            relativePathToFile As String
        ) As String

        Dim url As String
        Dim projectMatch As Match
        Dim project As String
        Dim repo As String


        projectMatch = ProjectPattern.Match(repositoryPath)

        If Not projectMatch.Success Then
            Throw New ApplicationException("Could not find the project and repository names in the remote URL.")
        End If

        project = projectMatch.Groups("project").Value
        repo = projectMatch.Groups("repo").Value

        url = String.Join("/", {baseUrl, "projects", project, "repos", repo, "browse", relativePathToFile})

        ' The branch name is specified via a query parameter.
        url &= $"?at={Uri.EscapeDataString(branchOrHash)}"

        Return url
    End Function


    Protected Overrides Function GetSelectionHash(
            filePath As String,
            selection As LineSelection
        ) As String

        Dim hash As String


        hash = $"#{selection.StartLineNumber}"

        If selection.StartLineNumber <> selection.EndLineNumber Then
            hash &= $"-{selection.EndLineNumber}"
        End If

        Return hash
    End Function

End Class
