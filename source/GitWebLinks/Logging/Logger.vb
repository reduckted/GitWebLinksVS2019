Imports Microsoft.VisualStudio
Imports Microsoft.VisualStudio.Shell.Interop


Public Class Logger
    Implements IAsyncInitializable
    Implements ILogger


    Private Shared ReadOnly OutputPaneIdentifier As New Guid("{51ABEFB8-7E4D-4B37-8F5A-3C6A032381A3}")


    Private cgOptions As IOptions
    Private cgOutputWindow As IVsOutputWindow
    Private cgOutputPane As Lazy(Of IVsOutputWindowPane)


    Public Sub New()
        cgOutputPane = New Lazy(Of IVsOutputWindowPane)(
            AddressOf CreateOutputPane,
            True
        )
    End Sub


    Private Function CreateOutputPane() As IVsOutputWindowPane
        Dim pane As IVsOutputWindowPane = Nothing


        cgOutputWindow.CreatePane(OutputPaneIdentifier, "Git Web Links", 1, 0)
        cgOutputWindow.GetPane(OutputPaneIdentifier, pane)

        Return pane
    End Function


    Public Async Function InitializeAsync(provider As IAsyncServiceProvider) As System.Threading.Tasks.Task _
        Implements IAsyncInitializable.InitializeAsync

        cgOptions = Await provider.GetServiceAsync(Of IOptions)
        cgOutputWindow = Await provider.GetServiceAsync(Of IVsOutputWindow)
    End Function


    Public Sub Log(message As String) _
        Implements ILogger.Log

        If cgOptions.EnableDebugLogging Then
            cgOutputPane.Value.OutputStringThreadSafe(message & Environment.NewLine)
        End If
    End Sub

End Class
