Public Interface IOptions

    Property GitHubEnterpriseUrls As IEnumerable(Of ServerUrl)


    Property BitbucketServerUrls As IEnumerable(Of ServerUrl)


    Property LinkType As LinkType


    Sub Save()

End Interface
