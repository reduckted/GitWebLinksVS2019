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


        args = $"&line={selection.StartLineNumber}&lineStartColumn={selection.StartColumnNumber}&lineEndColumn={selection.EndColumnNumber}"

        If selection.StartLineNumber <> selection.EndLineNumber Then
            args &= $"&lineEnd={selection.EndLineNumber}"
        End If

        Return args
    End Function

End Class
