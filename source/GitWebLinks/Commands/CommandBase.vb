Imports EnvDTE80
Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel.Design


Public MustInherit Class CommandBase

    Protected Sub New(dteProvider As IDteProvider)
        Dte = DirectCast(dteProvider.Dte, DTE2)
    End Sub


    Protected ReadOnly Property Dte As DTE2


    Public Sub Initialize(commandService As IMenuCommandService)
        For Each id In GetCommandIDs()
            AddCommand(commandService, id)
        Next id
    End Sub


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

End Class
