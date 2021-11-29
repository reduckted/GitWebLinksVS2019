Imports EnvDTE
Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel.Design


Public Class CopyLinkToCurrentFileCommand
    Inherits CommandBase


    Private Const Name As String = "Copy Link to Current File"


    Protected Overrides Iterator Function GetCommandIDs() As IEnumerable(Of CommandID)
        Yield Commands.CopyLinkToCurrentFile
        Yield Commands.CopyLinkToSelection
    End Function


    Protected Overrides Sub BeforeQueryStatus(command As OleMenuCommand)
        If (LinkInfo IsNot Nothing) AndAlso (Dte.ActiveDocument IsNot Nothing) Then
            Logger.Log($"Command '{Name}' is enabled with handler '{LinkInfo.Handler.Name}'.")
            command.Visible = True
            command.Text = $"Copy Link to {LinkInfo.Handler.Name}"

        Else
            Logger.Log($"Command '{Name}' is disabled.")
            command.Visible = False
        End If
    End Sub


    Protected Overrides Sub Invoke(id As CommandID)
        Try
            If (LinkInfo IsNot Nothing) AndAlso (Dte.ActiveDocument IsNot Nothing) Then
                Dim filePath As String


                filePath = Dte.ActiveDocument.FullName

                If Not String.IsNullOrEmpty(filePath) Then
                    Dim textSelection As TextSelection
                    Dim lineSelection As LineSelection
                    Dim url As String


                    If id.Equals(Commands.CopyLinkToCurrentFile) Then
                        textSelection = Nothing
                    Else
                        textSelection = TryCast(Dte.ActiveDocument.Selection, TextSelection)
                    End If

                    If textSelection IsNot Nothing Then
                        lineSelection = New LineSelection(
                        textSelection.TopLine,
                        textSelection.BottomLine,
                        textSelection.TopPoint.DisplayColumn,
                        textSelection.BottomPoint.DisplayColumn
                    )

                    Else
                        lineSelection = Nothing
                    End If

                    url = LinkInfo.Handler.MakeUrl(LinkInfo.GitInfo, filePath, lineSelection)

                    SetClipboardText(url)
                End If
            End If

        Catch ex As Exception
            Logger.Log($"Exception occurred when invoking command '{Name}': {ex}")
        End Try
    End Sub

End Class
