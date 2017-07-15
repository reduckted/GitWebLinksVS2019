Imports Microsoft.VisualStudio.ComponentModelHost
Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Windows


<ClassInterface(ClassInterfaceType.AutoDual)>
<CLSCompliant(False)>
<ComVisible(True)>
<Guid("1D9ECCF3-5D2F-4112-9B25-264596873DC9")>
Public Class OptionsPage
    Inherits UIElementDialogPage


    Private cgControl As OptionsPageControl
    Private cgViewModel As OptionsPageControlViewModel


    Protected Overrides ReadOnly Property Child As UIElement
        Get
            If cgControl Is Nothing Then
                cgViewModel = New OptionsPageControlViewModel
                cgControl = New OptionsPageControl With {.DataContext = cgViewModel}
            End If

            Return cgControl
        End Get
    End Property


    Protected Overrides Sub OnActivate(e As CancelEventArgs)
        Dim options As IOptions


        MyBase.OnActivate(e)

        options = GetOptions()

        cgViewModel.GitHubEnterpriseUrls.Clear()
        cgViewModel.BitbucketServerUrls.Clear()

        For Each server In ToModel(options.GitHubEnterpriseUrls)
            cgViewModel.GitHubEnterpriseUrls.Add(server)
        Next server

        For Each server In ToModel(options.BitbucketServerUrls)
            cgViewModel.BitbucketServerUrls.Add(server)
        Next server
    End Sub


    Protected Overrides Sub OnApply(e As PageApplyEventArgs)
        If e.ApplyBehavior = ApplyKind.Apply Then
            Dim options As IOptions


            options = GetOptions()

            options.GitHubEnterpriseUrls = FromModel(cgViewModel.GitHubEnterpriseUrls)
            options.BitbucketServerUrls = FromModel(cgViewModel.BitbucketServerUrls)

            options.Save()
        End If

        MyBase.OnApply(e)
    End Sub


    Private Function ToModel(servers As IEnumerable(Of ServerUrl)) As IEnumerable(Of ServerUrlModel)
        Return (
            From server In servers
            Select New ServerUrlModel With {
                .BaseUrl = server.BaseUrl,
                .SshUrl = server.SshUrl
            }
        )
    End Function


    Private Function FromModel(servers As IEnumerable(Of ServerUrlModel)) As IEnumerable(Of ServerUrl)
        Return servers.Select(Function(x) New ServerUrl(x.BaseUrl, x.SshUrl))
    End Function


    Private Function GetOptions() As IOptions
        Dim componentModel As IComponentModel


        componentModel = DirectCast(Site.GetService(GetType(SComponentModel)), IComponentModel)

        Return componentModel.DefaultExportProvider.GetExportedValue(Of IOptions)
    End Function

End Class
