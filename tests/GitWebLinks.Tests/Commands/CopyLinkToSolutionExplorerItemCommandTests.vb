Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel.Design
Imports System.Windows
Imports EnvDTE
Imports EnvDTE80

Public Class CopyLinkToSolutionExplorerItemCommandTests

    Private Enum SolutionExplorerItemKind
        Solution
        Project
        Folder
        File
    End Enum


    Public Class InitializeMethod

        <Fact()>
        Public Sub InitializesTheCommand()
            Dim command As CopyLinkToSolutionExplorerItemCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim commandService As Mock(Of IMenuCommandService)
            Dim addedCommands As List(Of MenuCommand)


            dteProvider = MockDteProvider()

            addedCommands = New List(Of MenuCommand)
            infoProvider = New Mock(Of ILinkInfoProvider)

            commandService = New Mock(Of IMenuCommandService)
            commandService.Setup(Sub(x) x.AddCommand(It.IsAny(Of MenuCommand))).Callback(Sub(x As MenuCommand) addedCommands.Add(x))

            command = New CopyLinkToSolutionExplorerItemCommand(dteProvider, infoProvider.Object, Mock.Of(Of IClipboard))
            command.Initialize(commandService.Object)

            Assert.Equal(
                {
                    Commands.CopyLinkToFileItem,
                    Commands.CopyLinkToFolderItem
                },
                addedCommands.Select(Function(x) x.CommandID)
            )
        End Sub

    End Class


    Public Class BeforeQueryStatusMethod

        <Fact()>
        Public Sub HidesTheCommandWhenThereIsNoGitInfo()
            Dim command As MenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim status As Integer


            dteProvider = MockDteProvider()

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(DirectCast(Nothing, LinkInfo))

            command = AddCommand(Commands.CopyLinkToFileItem, dteProvider, infoProvider.Object)
            status = command.OleStatus

            Assert.False(command.Visible)
        End Sub


        <Fact()>
        Public Sub ShowsTheCommandWhenGitInfoExists()
            Dim command As MenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dteProvider = MockDteProvider()

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = AddCommand(Commands.CopyLinkToFileItem, dteProvider, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
        End Sub


        <Fact()>
        Public Sub ShowsTheCommandForProjectItems()
            Dim command As OleMenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dteProvider = MockDteProvider(
                selectedItemKind:=SolutionExplorerItemKind.File
            )

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = AddCommand(Commands.CopyLinkToFileItem, dteProvider, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy link to Foo", command.Text)
        End Sub


        <Fact()>
        Public Sub ShowsTheCommandForProjects()
            Dim command As OleMenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dteProvider = MockDteProvider(
                selectedItemKind:=SolutionExplorerItemKind.Project
            )

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = AddCommand(Commands.CopyLinkToFileItem, dteProvider, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy link to Foo", command.Text)
        End Sub


        <Fact()>
        Public Sub ShowsTheCommandForSolutions()
            Dim command As OleMenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dteProvider = MockDteProvider(
                selectedItemKind:=SolutionExplorerItemKind.Solution
            )

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = AddCommand(Commands.CopyLinkToFileItem, dteProvider, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy link to Foo", command.Text)
        End Sub


        <Fact()>
        Public Sub ShowsTheCommandForFolders()
            Dim command As OleMenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dteProvider = MockDteProvider(
                selectedItemKind:=SolutionExplorerItemKind.Folder
            )

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = AddCommand(Commands.CopyLinkToFolderItem, dteProvider, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy link to Foo", command.Text)
        End Sub

    End Class


    Public Class InvokeMethod

        <Fact()>
        Public Sub PutsLinkOnClipboard()
            Dim command As OleMenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim clipboard As Mock(Of IClipboard)
            Dim gitInfo As GitInfo
            Dim linkInfo As LinkInfo


            dteProvider = MockDteProvider(
                selectedFileName:="Z:\foo\bar.txt"
            )

            gitInfo = New GitInfo("a", "b")

            handler = New Mock(Of ILinkHandler)
            handler.Setup(Function(x) x.MakeUrl(gitInfo, "Z:\foo\bar.txt", Nothing)).Returns("http://foo.bar")

            linkInfo = New LinkInfo(gitInfo, handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(linkInfo)

            clipboard = New Mock(Of IClipboard)

            command = AddCommand(Commands.CopyLinkToFileItem, dteProvider, infoProvider.Object, clipboard.Object)
            command.Invoke()

            clipboard.Verify(Sub(x) x.SetText("http://foo.bar"), Times.Once)
        End Sub

    End Class


    Private Shared Function AddCommand(
            id As CommandID,
            dteProvider As IDteProvider,
            infoProvider As ILinkInfoProvider,
            Optional clipboard As IClipboard = Nothing
        ) As OleMenuCommand

        Dim command As CopyLinkToSolutionExplorerItemCommand
        Dim commandService As Mock(Of IMenuCommandService)
        Dim commands As List(Of MenuCommand)


        commands = New List(Of MenuCommand)

        commandService = New Mock(Of IMenuCommandService)
        commandService.Setup(Sub(x) x.AddCommand(It.IsAny(Of MenuCommand))).Callback(Sub(x As MenuCommand) commands.Add(x))

        If clipboard Is Nothing Then
            clipboard = Mock.Of(Of IClipboard)
        End If

        command = New CopyLinkToSolutionExplorerItemCommand(dteProvider, infoProvider, clipboard)
        command.Initialize(commandService.Object)

        Return commands.Cast(Of OleMenuCommand).First(Function(x) x.CommandID.Equals(id))
    End Function


    Private Shared Function MockDteProvider(
            Optional selectedFileName As String = "Z:\foo.txt",
            Optional selectedItemKind As SolutionExplorerItemKind = SolutionExplorerItemKind.File
        ) As IDteProvider

        Dim provider As Mock(Of IDteProvider)
        Dim dte As Mock(Of DTE)
        Dim dte2 As Mock(Of DTE2)
        Dim toolWindows As Mock(Of ToolWindows)
        Dim solutionExplorer As Mock(Of UIHierarchy)
        Dim selectedItem As Mock(Of UIHierarchyItem)
        Dim selectedObject As Object


        Select Case selectedItemKind
            Case SolutionExplorerItemKind.Solution
                selectedObject = MockSolution(selectedFileName)

            Case SolutionExplorerItemKind.Project
                selectedObject = MockProject(selectedFileName)

            Case SolutionExplorerItemKind.Folder
                selectedObject = MockFolder(selectedFileName)

            Case Else
                selectedObject = MockProjectItem(selectedFileName)

        End Select

        selectedItem = New Mock(Of UIHierarchyItem)
        selectedItem.SetupGet(Function(x) x.Object).Returns(selectedObject)

        solutionExplorer = New Mock(Of UIHierarchy)
        solutionExplorer.SetupGet(Function(x) x.SelectedItems).Returns({selectedItem.Object})

        toolWindows = New Mock(Of ToolWindows)
        toolWindows.SetupGet(Function(x) x.SolutionExplorer).Returns(solutionExplorer.Object)

        dte = New Mock(Of DTE)
        dte2 = dte.As(Of DTE2)()
        dte2.SetupGet(Function(x) x.ToolWindows).Returns(toolWindows.Object)

        provider = New Mock(Of IDteProvider)
        provider.SetupGet(Function(x) x.Dte).Returns(dte.Object)

        Return provider.Object
    End Function


    Private Shared Function MockSolution(fullName As String) As Object
        Dim solution As Mock(Of Solution)


        solution = New Mock(Of Solution)
        solution.SetupGet(Function(x) x.FullName).Returns(fullName)

        Return solution.Object
    End Function


    Private Shared Function MockProject(fullName As String) As Object
        Dim project As Mock(Of Project)


        project = New Mock(Of Project)
        project.SetupGet(Function(x) x.FullName).Returns(fullName)

        Return project.Object
    End Function


    Private Shared Function MockFolder(fullName As String) As Object
        Dim project As Mock(Of ProjectItem)


        project = New Mock(Of ProjectItem)
        project.SetupGet(Function(x) x.FileNames(1)).Returns(fullName)

        Return project.Object
    End Function


    Private Shared Function MockProjectItem(fullName As String) As Object
        Dim project As Mock(Of ProjectItem)


        project = New Mock(Of ProjectItem)
        project.SetupGet(Function(x) x.FileNames(1)).Returns(fullName)

        Return project.Object
    End Function

End Class
