Public Interface IOptions

    Property GitHubEnterpriseUrls As IEnumerable(Of ServerUrl)


    Property BitbucketServerUrls As IEnumerable(Of ServerUrl)


    Sub Save()

End Interface
