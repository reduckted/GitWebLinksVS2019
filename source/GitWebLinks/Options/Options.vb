Imports System.ComponentModel.Composition


<Export(GetType(IOptions))>
Public Class Options
    Implements IOptions


    Private Const GitHubUrlsProperty As String = "GitHubUrls"
    Private Const BitbucketCloudUrlsProperty As String = "BitbucketCloudURls"
    Private Const BitbucketServerUrlsProperty As String = "BitbucketServerUrls"


    Private Shared ReadOnly DefaultGitHubUrls() As String = {
        "https://github.com",
        "git@github.com"
    }


    Private Shared ReadOnly DefaultBitbucketCloudUrls() As String = {
        "https://bitbucket.org",
        "git@bitbucket.org"
    }


    Private Shared ReadOnly DefaultBitbucketServerUrls() As String = {
        "http://my-bitbucket-server:7990",
        "git@my-bitbucket-server:7999"
    }


    Private ReadOnly cgSettingsManager As ISettingsManager
    Private ReadOnly cgGitHubUrls As New List(Of String)(DefaultGitHubUrls)
    Private ReadOnly cgBitbucketCloudUrls As New List(Of String)(DefaultBitbucketCloudUrls)
    Private ReadOnly cgBitbucketServerUrls As New List(Of String)(DefaultBitbucketServerUrls)


    <ImportingConstructor()>
    Public Sub New(settingsManager As ISettingsManager)
        cgSettingsManager = settingsManager
        LoadSettings()
    End Sub


    Public Property GitHubUrls As IEnumerable(Of String) _
        Implements IOptions.GitHubUrls

        Get
            Return cgGitHubUrls
        End Get

        Set
            cgGitHubUrls.Clear()

            If Value IsNot Nothing Then
                cgGitHubUrls.AddRange(Value)
            End If

            ' Add the default URLs back in if they were all deleted.
            If cgGitHubUrls.Count = 0 Then
                cgGitHubUrls.AddRange(DefaultGitHubUrls)
            End If

            SaveSettings()
        End Set
    End Property


    Public Property BitbucketCloudUrls As IEnumerable(Of String) _
        Implements IOptions.BitbucketCloudUrls

        Get
            Return cgBitbucketCloudUrls
        End Get

        Set
            cgBitbucketCloudUrls.Clear()

            If Value IsNot Nothing Then
                cgBitbucketCloudUrls.AddRange(Value)
            End If

            ' Add the default URLs back in if they were all deleted.
            If cgBitbucketCloudUrls.Count = 0 Then
                cgBitbucketCloudUrls.AddRange(DefaultBitbucketCloudUrls)
            End If

            SaveSettings()
        End Set
    End Property


    Public Property BitbucketServerUrls As IEnumerable(Of String) _
        Implements IOptions.BitbucketServerUrls

        Get
            Return cgBitbucketServerUrls
        End Get

        Set
            cgBitbucketServerUrls.Clear()

            If Value IsNot Nothing Then
                cgBitbucketServerUrls.AddRange(Value)
            End If

            ' Note: We don't add the default URLs back in because the
            ' defaults are only used as an example of what to specify.

            SaveSettings()
        End Set
    End Property


    Private Sub LoadSettings()
        Try
            If cgSettingsManager.Contains(GitHubUrlsProperty) Then
                GitHubUrls = cgSettingsManager.GetString(GitHubUrlsProperty).Split("|"c)
            End If

            If cgSettingsManager.Contains(BitbucketCloudUrlsProperty) Then
                BitbucketCloudUrls = cgSettingsManager.GetString(BitbucketCloudUrlsProperty).Split("|"c)
            End If

            If cgSettingsManager.Contains(BitbucketServerUrlsProperty) Then
                BitbucketServerUrls = cgSettingsManager.GetString(BitbucketServerUrlsProperty).Split("|"c)
            End If

        Catch ex As Exception
            Diagnostics.Debug.WriteLine(ex.Message)
        End Try
    End Sub


    Private Sub SaveSettings()
        Try
            cgSettingsManager.SetString(GitHubUrlsProperty, String.Join("|", cgGitHubUrls))
            cgSettingsManager.SetString(BitbucketCloudUrlsProperty, String.Join("|", cgBitbucketCloudUrls))
            cgSettingsManager.SetString(BitbucketServerUrlsProperty, String.Join("|", cgBitbucketServerUrls))

        Catch ex As Exception
            Diagnostics.Debug.WriteLine(ex.Message)
        End Try
    End Sub

End Class
