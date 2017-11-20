Imports Microsoft.VisualStudio.ComponentModelHost
Imports Microsoft.VisualStudio.Shell
Imports Microsoft.VisualStudio.Shell.Interop
Imports System.ComponentModel.Design
Imports System.Runtime.InteropServices


<PackageRegistration(UseManagedResourcesOnly:=True)>
<InstalledProductRegistration("#110", "#112", "1.0", IconResourceID:=400)>
<ProvideMenuResource("Menus.ctmenu", 1)>
<Guid(CopyLinkCommandPackage.PackageGuidString)>
<ProvideAutoLoad(UIContextGuids80.SolutionExists)>
<ProvideOptionPage(GetType(OptionsPage), "Git Web Links", "General", 0, 0, True)>
Public NotInheritable Class CopyLinkCommandPackage
    Inherits Package


    Public Const PackageGuidString As String = "2616be63-dcab-4321-8473-d332f6e89767"


    Protected Overrides Sub Initialize()
        Dim componentModel As IComponentModel
        Dim commandService As OleMenuCommandService


        MyBase.Initialize()

        componentModel = DirectCast(GetService(GetType(SComponentModel)), IComponentModel)
        commandService = DirectCast(GetService(GetType(IMenuCommandService)), OleMenuCommandService)

        For Each command In componentModel.GetExtensions(Of CommandBase)()
            command.Initialize(commandService)
        Next command
    End Sub

End Class
