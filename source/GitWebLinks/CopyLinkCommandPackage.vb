Imports Microsoft.VisualStudio.ComponentModelHost
Imports Microsoft.VisualStudio.Shell
Imports Microsoft.VisualStudio.Shell.Interop
Imports System.ComponentModel.Design
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


    Protected Overrides Async Function InitializeAsync(
            cancellationToken As CancellationToken,
            progress As IProgress(Of ServiceProgressData)
        ) As Tasks.Task

        Dim componentModel As IComponentModel
        Dim commandService As OleMenuCommandService


        componentModel = DirectCast(Await GetServiceAsync(GetType(SComponentModel)), IComponentModel)
        commandService = DirectCast(Await GetServiceAsync(GetType(IMenuCommandService)), OleMenuCommandService)

        Await JoinableTaskFactory.SwitchToMainThreadAsync()

        For Each command In componentModel.GetExtensions(Of CommandBase)()
            command.Initialize(commandService)
        Next command
    End Function

End Class
