Public Class OptionsTests
    <Fact()>
    Public Async Sub SaveServersWithEmptyBaseAndSsh()
        Dim options As New Options()
        Dim asyncServiceProvider As New TestAsyncServiceProvider()
        Dim mocksettingsManager As New Mock(Of ISettingsManager)
        asyncServiceProvider.AddService(mocksettingsManager.Object)
        Await options.InitializeAsync(asyncServiceProvider)
        options.BitbucketServerUrls = New List(Of ServerUrl) From {
            New ServerUrl(Nothing, Nothing)
        }

        options.Save()

        mocksettingsManager.Verify(Sub(x) x.SetString(It.Is(Function(n As String) n = "BitbucketServerServers"), It.Is(Function(v As String) v = "<servers />")), Times.Once())
    End Sub
End Class
