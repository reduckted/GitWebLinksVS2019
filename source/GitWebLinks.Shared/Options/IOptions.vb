Public Interface IOptions

    Property GitHubEnterpriseUrls As IEnumerable(Of ServerUrl)


    Property BitbucketServerUrls As IEnumerable(Of ServerUrl)


    Property LinkType As LinkType


    Property EnableDebugLogging As Boolean


    Sub Save()

End Interface
