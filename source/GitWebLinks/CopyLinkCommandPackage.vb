Imports Microsoft.VisualStudio.Shell
Imports Microsoft.VisualStudio.Shell.Interop
Imports System.Runtime.InteropServices
Imports System.Threading


<PackageRegistration(UseManagedResourcesOnly:=True, AllowsBackgroundLoading:=True)>
<InstalledProductRegistration("#110", "#112", "1.0", IconResourceID:=400)>
<ProvideMenuResource("Menus.ctmenu", 1)>
<Guid(CopyLinkCommandPackage.PackageGuidString)>
<ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)>
<ProvideOptionPage(GetType(OptionsPage), "Git Web Links", "General", 0, 0, True)>
Public NotInheritable Class CopyLinkCommandPackage
    Inherits AsyncPackage


    Public Const PackageGuidString As String = "2616be63-dcab-4321-8473-d332f6e89767"


    Private ReadOnly cgCommands As New List(Of CommandBase)


    Protected Overrides Async Function InitializeAsync(
            cancellationToken As CancellationToken,
            progress As IProgress(Of ServiceProgressData)
        ) As Tasks.Task

        AddService(Of IClipboard, Clipboard)()
        AddService(Of ISettingsManager, SettingsManager)()
        AddService(Of IOptions, Options)()
        AddService(Of ILinkInfoProvider, LinkInfoProvider)()
        AddService(Of ILinkInfoFinder, LinkInfoFinder)()
        AddService(Of IGitInfoFinder, GitInfoFinder)()
        AddService(Of ILinkHandlerSource, LinkHandlerSource)()
        AddService(Of ILogger, Logger)()

        Await JoinableTaskFactory.SwitchToMainThreadAsync()

        Await AddCommandAsync(New CopyLinkToCurrentFileCommand)
        Await AddCommandAsync(New CopyLinkToSolutionExplorerItemCommand)
    End Function


    Private Overloads Sub AddService(Of TService, TImplementation As {New, TService})()
        AddService(
            GetType(TService),
            Async Function(container, cancellationToken, serviceType)
                Dim service As Object
                Dim initializable As IAsyncInitializable


                service = New TImplementation
                initializable = TryCast(service, IAsyncInitializable)

                If initializable IsNot Nothing Then
                    Await initializable.InitializeAsync(New PackageAsyncServiceProvider(Me))
                End If

                Return service
            End Function
        )
    End Sub


    Private Async Function AddCommandAsync(command As CommandBase) As Tasks.Task
        Await command.InitializeAsync(New PackageAsyncServiceProvider(Me))
        cgCommands.Add(command)
    End Function

End Class
