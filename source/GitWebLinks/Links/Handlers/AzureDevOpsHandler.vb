Imports LibGit2Sharp
Imports System.Text.RegularExpressions


Public Class AzureDevOpsHandler
    Inherits LinkHandlerBase


    Private Shared ReadOnly HttpsPattern As New Regex("^https:\/\/(.+@)?dev.azure.com\/(?<username>[^\/]+)\/(?<project>[^\/]+)\/_git\/.+$")
    Private Shared ReadOnly SshPattern As New Regex("^git@ssh\.dev\.azure\.com:v3\/(?<username>[^\/]+)\/(?<project>[^\/]+)\/.+$")


    Public Overrides ReadOnly Property Name As String
        Get
            Return "Azure DevOps"
        End Get
    End Property


    Protected Overrides Function GetMatchingServerUrl(remoteUrl As String) As ServerUrl
        Dim match As Match


        match = SshPattern.Match(remoteUrl)

        If Not match.Success Then
            match = HttpsPattern.Match(remoteUrl)
        End If

        If match.Success Then
            Dim username As String
            Dim project As String


            username = match.Groups("username").Value
            project = match.Groups("project").Value

            Return New ServerUrl(
                $"https://dev.azure.com/{username}/{project}/_git",
                $"git@ssh.dev.azure.com:v3/{username}/{project}"
            )
        End If

        Return Nothing
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

        Dim root As String
        Dim path As String
        Dim version As String
        Dim branchOrHashPrefix As String


        If Options.LinkType = LinkType.Branch Then
            branchOrHashPrefix = "GB"
        Else
            branchOrHashPrefix = "GC"
        End If

        root = String.Join("/", {baseUrl, repositoryPath})
        path = Uri.EscapeDataString(relativePathToFile)
        version = $"{branchOrHashPrefix}{Uri.EscapeDataString(branchOrHash)}"

        Return $"{root}?path=%2F{path}&version={version}"
    End Function


    Protected Overrides Function GetSelectionHash(
            filePath As String,
            selection As LineSelection
        ) As String

        Dim args As String


        args = $"&line={selection.StartLineNumber}"

        If selection.StartLineNumber <> selection.EndLineNumber Then
            ' The selection spans multiple lines. Add the end line number.
            args &= $"&lineEnd={selection.EndLineNumber}"

            ' Multi-line selections always need to specify the start
            ' and end columns, otherwise nothing ends up being selected.
            args &= $"&lineStartColumn={selection.StartColumnNumber}&lineEndColumn={selection.EndColumnNumber}"

        Else
            ' If the single-line selection is an actual selection as opposed to the caret
            ' being somewhere on the line but not actually selecting any text, then we will
            ' include that same selection range in the link. If there is no selected text, then
            ' we'll leave the start and end columns out. If we include them when they are the same
            ' value, Azure DevOps will still scroll to the line, but the line won't be highlighted.
            If selection.StartColumnNumber <> selection.EndColumnNumber Then
                args &= $"&lineStartColumn={selection.StartColumnNumber}&lineEndColumn={selection.EndColumnNumber}"

            Else
                ' The modern repository landing page in Azure DevOps won't highlight
                ' any text if we only provide a start line number. We also need to include
                ' the start column and end column. Since there is no actual text selected,
                ' we will select the whole line by setting the end line number to the next
                ' line and the start and end columns to the start of each line.
                args &= $"&lineEnd={selection.StartLineNumber + 1}&lineStartColumn=1&lineEndColumn=1"
            End If
        End If

        Return args
    End Function

End Class
