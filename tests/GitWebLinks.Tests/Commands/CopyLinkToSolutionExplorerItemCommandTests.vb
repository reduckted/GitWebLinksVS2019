Imports EnvDTE
Imports EnvDTE80
Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel.Design


Public Class CopyLinkToSolutionExplorerItemCommandTests

    Private Enum SolutionExplorerItemKind
        Solution
        Project
        Folder
        File
    End Enum


    Public Class InitializeAsyncMethod

        <Fact()>
        Public Async Function InitializesTheCommand() As Threading.Tasks.Task
            Dim command As CopyLinkToSolutionExplorerItemCommand
            Dim serviceProvider As TestAsyncServiceProvider
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim commandService As Mock(Of IMenuCommandService)
            Dim addedCommands As List(Of MenuCommand)


            dte = MockDte()

            addedCommands = New List(Of MenuCommand)
            infoProvider = New Mock(Of ILinkInfoProvider)

            commandService = New Mock(Of IMenuCommandService)
            commandService.Setup(Sub(x) x.AddCommand(It.IsAny(Of MenuCommand))).Callback(Sub(x As MenuCommand) addedCommands.Add(x))

            serviceProvider = New TestAsyncServiceProvider
            serviceProvider.AddService(commandService.Object)
            serviceProvider.AddService(dte)
            serviceProvider.AddService(infoProvider.Object)
            serviceProvider.AddService(Mock.Of(Of IClipboard))

            command = New CopyLinkToSolutionExplorerItemCommand
            Await command.InitializeAsync(serviceProvider)

            Assert.Equal(
                {Commands.CopyLinkToSolutionExplorerItem},
                addedCommands.Select(Function(x) x.CommandID)
            )
        End Function

    End Class


    Public Class BeforeQueryStatusMethod

        <Fact()>
        Public Async Function HidesTheCommandWhenThereIsNoGitInfo() As Threading.Tasks.Task
            Dim command As MenuCommand
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim status As Integer


            dte = MockDte()

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(DirectCast(Nothing, LinkInfo))

            command = Await AddCommandAsync(dte, infoProvider.Object)
            status = command.OleStatus

            Assert.False(command.Visible)
        End Function


        <Fact()>
        Public Async Function ShowsTheCommandWhenGitInfoExists() As Threading.Tasks.Task
            Dim command As MenuCommand
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dte = MockDte()

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = Await AddCommandAsync(dte, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
        End Function


        <Fact()>
        Public Async Function ShowsTheCommandForProjectItems() As Threading.Tasks.Task
            Dim command As OleMenuCommand
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dte = MockDte(
                selectedItemKind:=SolutionExplorerItemKind.File
            )

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = Await AddCommandAsync(dte, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy Link to Foo", command.Text)
        End Function


        <Fact()>
        Public Async Function ShowsTheCommandForProjects() As Threading.Tasks.Task
            Dim command As OleMenuCommand
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dte = MockDte(
                selectedItemKind:=SolutionExplorerItemKind.Project
            )

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = Await AddCommandAsync(dte, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy Link to Foo", command.Text)
        End Function


        <Fact()>
        Public Async Function ShowsTheCommandForSolutions() As Threading.Tasks.Task
            Dim command As OleMenuCommand
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dte = MockDte(
                selectedItemKind:=SolutionExplorerItemKind.Solution
            )

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = Await AddCommandAsync(dte, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy Link to Foo", command.Text)
        End Function

    End Class


    Public Class InvokeMethod

        <Fact()>
        Public Async Function PutsLinkOnClipboard() As Threading.Tasks.Task
            Dim command As OleMenuCommand
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim clipboard As Mock(Of IClipboard)
            Dim gitInfo As GitInfo
            Dim linkInfo As LinkInfo


            dte = MockDte(
                selectedFileName:="Z:\foo\bar.txt"
            )

            gitInfo = New GitInfo("a", "b")

            handler = New Mock(Of ILinkHandler)
            handler.Setup(Function(x) x.MakeUrl(gitInfo, "Z:\foo\bar.txt", Nothing)).Returns("http://foo.bar")

            linkInfo = New LinkInfo(gitInfo, handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(linkInfo)

            clipboard = New Mock(Of IClipboard)

            command = Await AddCommandAsync(dte, infoProvider.Object, clipboard.Object)
            command.Invoke()

            clipboard.Verify(Sub(x) x.SetText("http://foo.bar"), Times.Once)
        End Function

    End Class


    Private Shared Async Function AddCommandAsync(
            dte As DTE,
            infoProvider As ILinkInfoProvider,
            Optional clipboard As IClipboard = Nothing
        ) As Threading.Tasks.Task(Of OleMenuCommand)

        Dim serviceProvider As TestAsyncServiceProvider
        Dim command As CopyLinkToSolutionExplorerItemCommand
        Dim commandService As Mock(Of IMenuCommandService)
        Dim commands As List(Of MenuCommand)


        commands = New List(Of MenuCommand)

        commandService = New Mock(Of IMenuCommandService)
        commandService.Setup(Sub(x) x.AddCommand(It.IsAny(Of MenuCommand))).Callback(Sub(x As MenuCommand) commands.Add(x))

        If clipboard Is Nothing Then
            clipboard = Mock.Of(Of IClipboard)
        End If

        serviceProvider = New TestAsyncServiceProvider
        serviceProvider.AddService(dte)
        serviceProvider.AddService(infoProvider)
        serviceProvider.AddService(clipboard)
        serviceProvider.AddService(commandService.Object)

        command = New CopyLinkToSolutionExplorerItemCommand
        Await command.InitializeAsync(serviceProvider)

        Return commands.Cast(Of OleMenuCommand).First()
    End Function


    Private Shared Function MockDte(
            Optional selectedFileName As String = "Z:\foo.txt",
            Optional selectedItemKind As SolutionExplorerItemKind = SolutionExplorerItemKind.File
        ) As DTE

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

        Return dte.Object
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
