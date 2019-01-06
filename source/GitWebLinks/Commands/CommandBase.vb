Imports EnvDTE
Imports EnvDTE80
Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel.Design


Public MustInherit Class CommandBase
    Implements IAsyncInitializable


    Private cgClipboard As IClipboard
    Private cgLinkInfoProvider As ILinkInfoProvider
    Private cgDte As DTE2


    Public Async Function InitializeAsync(provider As IAsyncServiceProvider) As Threading.Tasks.Task _
        Implements IAsyncInitializable.InitializeAsync

        Dim commandService As IMenuCommandService


        cgClipboard = Await provider.GetServiceAsync(Of IClipboard)
        cgLinkInfoProvider = Await provider.GetServiceAsync(Of ILinkInfoProvider)
        cgDte = DirectCast(Await provider.GetServiceAsync(Of DTE), DTE2)

        commandService = Await provider.GetServiceAsync(Of IMenuCommandService)

        For Each id In GetCommandIDs()
            AddCommand(commandService, id)
        Next id
    End Function


    Private Sub AddCommand(
            commandService As IMenuCommandService,
            id As CommandID
        )

        Dim command As OleMenuCommand


        command = New OleMenuCommand(Sub(s, e) Invoke(id), id)
        AddHandler command.BeforeQueryStatus, Sub(s, e) BeforeQueryStatus(DirectCast(s, OleMenuCommand))

        commandService.AddCommand(command)
    End Sub


    Protected MustOverride Function GetCommandIDs() As IEnumerable(Of CommandID)


    Protected MustOverride Sub BeforeQueryStatus(command As OleMenuCommand)


    Protected MustOverride Sub Invoke(commandID As CommandID)


    Protected ReadOnly Property Dte As DTE2
        Get
            Return cgDte
        End Get
    End Property


    Protected ReadOnly Property LinkInfo As LinkInfo
        Get
            Return cgLinkInfoProvider.LinkInfo
        End Get
    End Property


    Protected Sub SetClipboardText(text As String)
        cgClipboard.SetText(text)
    End Sub

End Class
