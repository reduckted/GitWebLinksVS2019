Imports EnvDTE
Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel.Design


Public Class CopyLinkToSolutionExplorerItemCommand
    Inherits CommandBase


    Private Const Name As String = "Copy Link to Solution Explorer Item"


    Protected Overrides Iterator Function GetCommandIDs() As IEnumerable(Of CommandID)
        Yield Commands.CopyLinkToSolutionExplorerItem
    End Function


    Protected Overrides Sub BeforeQueryStatus(command As OleMenuCommand)
        If (LinkInfo IsNot Nothing) AndAlso (Not String.IsNullOrEmpty(GetSelectedFilePath())) Then
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
            If LinkInfo IsNot Nothing Then
                Dim filePath As String


                filePath = GetSelectedFilePath()

                If Not String.IsNullOrEmpty(filePath) Then
                    Dim url As String


                    url = LinkInfo.Handler.MakeUrl(LinkInfo.GitInfo, filePath, Nothing)

                    SetClipboardText(url)
                End If
            End If

        Catch ex As Exception
            Logger.Log($"Exception occurred when invoking command '{Name}': {ex}")
        End Try
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
