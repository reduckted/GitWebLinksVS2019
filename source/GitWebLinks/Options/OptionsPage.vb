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


    Protected Overrides ReadOnly Property Child As UIElement
        Get
            If cgControl Is Nothing Then
                cgControl = New OptionsPageControl
            End If

            Return cgControl
        End Get
    End Property


    Protected Overrides Sub OnActivate(e As CancelEventArgs)
        Dim options As IOptions


        MyBase.OnActivate(e)

        options = GetOptions()

        cgControl.GitHubUrls = String.Join(Environment.NewLine, options.GitHubUrls)
        cgControl.BitbucketCloudUrls = String.Join(Environment.NewLine, options.BitbucketCloudUrls)
        cgControl.BitbucketServerUrls = String.Join(Environment.NewLine, options.BitbucketServerUrls)
    End Sub


    Protected Overrides Sub OnApply(e As PageApplyEventArgs)
        If e.ApplyBehavior = ApplyKind.Apply Then
            Dim options As IOptions


            options = GetOptions()

            options.GitHubUrls = MakeCollection(cgControl.GitHubUrls)
            options.BitbucketCloudUrls = MakeCollection(cgControl.BitbucketCloudUrls)
            options.BitbucketServerUrls = MakeCollection(cgControl.BitbucketServerUrls)
        End If

        MyBase.OnApply(e)
    End Sub


    Private Function MakeCollection(s As String) As IEnumerable(Of String)
        Return (
            From value In s.Split({Environment.NewLine}, StringSplitOptions.None)
            Let trimmed = value.Trim()
            Where Not String.IsNullOrEmpty(trimmed)
            Select trimmed
        )
    End Function


    Private Function GetOptions() As IOptions
        Dim componentModel As IComponentModel


        componentModel = DirectCast(Site.GetService(GetType(SComponentModel)), IComponentModel)

        Return componentModel.DefaultExportProvider.GetExportedValue(Of IOptions)
    End Function

End Class
