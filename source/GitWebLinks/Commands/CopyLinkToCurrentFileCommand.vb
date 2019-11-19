Imports EnvDTE
Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel.Design


Public Class CopyLinkToCurrentFileCommand
    Inherits CommandBase


    Protected Overrides Iterator Function GetCommandIDs() As IEnumerable(Of CommandID)
        Yield Commands.CopyLinkToCurrentFile
        Yield Commands.CopyLinkToSelection
    End Function


    Protected Overrides Sub BeforeQueryStatus(command As OleMenuCommand)
        If (LinkInfo IsNot Nothing) AndAlso (Dte.ActiveDocument IsNot Nothing) Then
            command.Visible = True
            command.Text = $"Copy Link to {LinkInfo.Handler.Name}"

        Else
            command.Visible = False
        End If
    End Sub


    Protected Overrides Sub Invoke(id As CommandID)
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
                    lineSelection = New LineSelection(textSelection.TopLine, textSelection.BottomLine, If(textSelection.TopPoint Is Nothing, 0, textSelection.TopPoint.DisplayColumn), If(textSelection.BottomPoint Is Nothing, 0, textSelection.BottomPoint.DisplayColumn))
                Else
                    lineSelection = Nothing
                End If



                url = LinkInfo.Handler.MakeUrl(LinkInfo.GitInfo, filePath, lineSelection)

                SetClipboardText(url)
            End If
        End If
    End Sub

End Class
