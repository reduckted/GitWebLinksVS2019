Imports System.Collections.ObjectModel


Public Class OptionsPageControlViewModel

    Public Sub New()
        GitHubEnterpriseUrls = New ObservableCollection(Of ServerUrlModel)
        BitbucketServerUrls = New ObservableCollection(Of ServerUrlModel)
    End Sub


    Public ReadOnly Property GitHubEnterpriseUrls As ObservableCollection(Of ServerUrlModel)


    Public ReadOnly Property BitbucketServerUrls As ObservableCollection(Of ServerUrlModel)

End Class
