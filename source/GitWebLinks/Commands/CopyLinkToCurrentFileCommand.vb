Imports EnvDTE
Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel.Composition
Imports System.ComponentModel.Design


<Export(GetType(CommandBase))>
Public Class CopyLinkToCurrentFileCommand
    Inherits CommandBase


    Private ReadOnly cgLinkInfoProvider As ILinkInfoProvider
    Private ReadOnly cgClipboard As IClipboard


    <ImportingConstructor()>
    Public Sub New(
            dteProvider As IDteProvider,
            linkInfoProvider As ILinkInfoProvider,
            clipboard As IClipboard
        )

        MyBase.New(dteProvider)
        cgLinkInfoProvider = linkInfoProvider
        cgClipboard = clipboard
    End Sub


    Protected Overrides Iterator Function GetCommandIDs() As IEnumerable(Of CommandID)
        Yield Commands.CopyLinkToCurrentFile
        Yield Commands.CopyLinkToSelection
    End Function


    Protected Overrides Sub BeforeQueryStatus(command As OleMenuCommand)
        If (cgLinkInfoProvider.LinkInfo IsNot Nothing) AndAlso (Dte.ActiveDocument IsNot Nothing) Then
            command.Visible = True
            command.Text = $"Copy Link to {cgLinkInfoProvider.LinkInfo.Handler.Name}"

        Else
            command.Visible = False
        End If
    End Sub


    Protected Overrides Sub Invoke(id As CommandID)
        If (cgLinkInfoProvider.LinkInfo IsNot Nothing) AndAlso (Dte.ActiveDocument IsNot Nothing) Then
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
                    lineSelection = New LineSelection(textSelection.TopLine, textSelection.BottomLine)
                Else
                    lineSelection = Nothing
                End If

                url = cgLinkInfoProvider.LinkInfo.Handler.MakeUrl(
                    cgLinkInfoProvider.LinkInfo.GitInfo,
                    filePath,
                    lineSelection
                )

                cgClipboard.SetText(url)
            End If
        End If
    End Sub

End Class
