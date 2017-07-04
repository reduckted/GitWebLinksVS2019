Imports EnvDTE
Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel.Composition
Imports System.ComponentModel.Design


<Export(GetType(CommandBase))>
Public Class CopyLinkToSolutionExplorerItemCommand
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
        Yield Commands.CopyLinkToFileItem
        Yield Commands.CopyLinkToFolderItem
    End Function


    Protected Overrides Sub BeforeQueryStatus(command As OleMenuCommand)
        If (cgLinkInfoProvider.LinkInfo IsNot Nothing) AndAlso (Not String.IsNullOrEmpty(GetSelectedFilePath())) Then
            Dim type As String


            If command.CommandID.Equals(Commands.CopyLinkToFolderItem) Then
                type = "folder"
            Else
                type = "file"
            End If

            command.Visible = True
            command.Text = $"Copy link to {type} on {cgLinkInfoProvider.LinkInfo.Handler.Name}"

        Else
            command.Visible = False
        End If
    End Sub


    Protected Overrides Sub Invoke(id As CommandID)
        If cgLinkInfoProvider.LinkInfo IsNot Nothing Then
            Dim filePath As String


            filePath = GetSelectedFilePath()

            If Not String.IsNullOrEmpty(filePath) Then
                Dim url As String


                url = cgLinkInfoProvider.LinkInfo.Handler.MakeUrl(
                    cgLinkInfoProvider.LinkInfo.GitInfo,
                    filePath,
                    Nothing
                )

                cgClipboard.SetText(url)
            End If
        End If
    End Sub


    Protected Function GetSelectedFilePath() As String
        Dim items As Array


        items = DirectCast(Dte.ToolWindows.SolutionExplorer.SelectedItems, Array)

        For Each item In items.OfType(Of UIHierarchyItem)
            If TypeOf item.Object Is Solution Then
                Return DirectCast(item.Object, Solution).FullName
            End If

            If TypeOf item.Object Is Project Then
                Return DirectCast(item.Object, Project).FullName
            End If

            If TypeOf item.Object Is ProjectItem Then
                ' An index of zero works for normal project items, but fails
                ' for solution items. An index of one works for both. Weird.
                Return DirectCast(item.Object, ProjectItem).FileNames(1)
            End If
        Next item

        Return Nothing
    End Function

End Class
