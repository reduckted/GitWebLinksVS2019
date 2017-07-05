Public Class OptionsTests

    <Fact()>
    Public Sub UsesDefaultGitHubUrls()
        Dim options As Options


        options = New Options(MockSettingsManager())

        Assert.Equal(
            options.GitHubUrls,
            {"https://github.com", "git@github.com"}
        )
    End Sub


    <Fact()>
    Public Sub UsesDefaultBitbucketCloudHubUrls()
        Dim options As Options


        options = New Options(MockSettingsManager())

        Assert.Equal(
            options.BitbucketCloudUrls,
            {"https://bitbucket.org", "git@bitbucket.org"}
        )
    End Sub


    Private Shared Function MockSettingsManager() As ISettingsManager
        Dim settingsManager As Mock(Of ISettingsManager)


        settingsManager = New Mock(Of ISettingsManager)
        settingsManager.Setup(Function(x) x.Contains(It.IsAny(Of String))).Returns(False)

        Return settingsManager.Object
    End Function

End Class
