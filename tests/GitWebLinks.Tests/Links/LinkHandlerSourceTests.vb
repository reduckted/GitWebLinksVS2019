Public Class LinkHandlerSourceTests

    <Fact()>
    Public Async Function FindsAllHandlers() As Threading.Tasks.Task
        Dim source As LinkHandlerSource
        Dim provider As TestAsyncServiceProvider
        Dim handlers As IEnumerable(Of ILinkHandler)


        provider = New TestAsyncServiceProvider
        provider.AddService(Mock.Of(Of IOptions))

        source = New LinkHandlerSource
        Await source.InitializeAsync(provider)

        handlers = source.GetHandlers()

        Assert.Equal(
            {
                GetType(AzureDevOpsHandler),
                GetType(BitbucketCloudHandler),
                GetType(BitbucketServerHandler),
                GetType(GitHubHandler),
                GetType(VisualStudioTeamServicesHandler)
            },
            handlers.Select(Function(x) x.GetType()).OrderBy(Function(x) x.Name)
        )
    End Function

End Class
