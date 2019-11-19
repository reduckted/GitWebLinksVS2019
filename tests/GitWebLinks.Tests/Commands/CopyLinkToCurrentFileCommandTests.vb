Imports EnvDTE
Imports EnvDTE80
Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel.Design


Public Class CopyLinkToCurrentFileCommandTests

    Public Class InitializeAsyncMethod

        <Fact()>
        Public Async Function InitializesTheCommand() As Threading.Tasks.Task
            Dim command As CopyLinkToCurrentFileCommand
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim commandService As Mock(Of IMenuCommandService)
            Dim addedCommands As List(Of MenuCommand)
            Dim serviceProvider As TestAsyncServiceProvider


            addedCommands = New List(Of MenuCommand)
            infoProvider = New Mock(Of ILinkInfoProvider)

            commandService = New Mock(Of IMenuCommandService)
            commandService.Setup(Sub(x) x.AddCommand(It.IsAny(Of MenuCommand))).Callback(Sub(x As MenuCommand) addedCommands.Add(x))

            serviceProvider = New TestAsyncServiceProvider
            serviceProvider.AddService(infoProvider.Object)
            serviceProvider.AddService(Mock.Of(Of IClipboard))
            serviceProvider.AddService(commandService.Object)
            serviceProvider.AddService(MockDte())

            command = New CopyLinkToCurrentFileCommand()
            Await command.InitializeAsync(serviceProvider)

            Assert.Equal(
                {
                    Commands.CopyLinkToCurrentFile,
                    Commands.CopyLinkToSelection
                },
                addedCommands.Select(Function(x) x.CommandID)
            )
        End Function

    End Class


    Public Class BeforeQueryStatusMethod

        <Fact()>
        Public Async Function HidesTheCommandWhenThereIsNoLinkInfo() As Threading.Tasks.Task
            Dim command As MenuCommand
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim status As Integer
            Dim dte As DTE


            dte = MockDte()

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(DirectCast(Nothing, LinkInfo))

            command = Await AddCommandAsync(Commands.CopyLinkToCurrentFile, dte, infoProvider.Object)
            status = command.OleStatus

            Assert.False(command.Visible)
        End Function


        <Fact()>
        Public Async Function ShowsCurrentFileCommandWhenGitInfoExists() As Threading.Tasks.Task
            Dim command As OleMenuCommand
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dte = MockDte()

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = Await AddCommandAsync(Commands.CopyLinkToCurrentFile, dte, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy Link to Foo", command.Text)
        End Function


        <Fact()>
        Public Async Function ShowsSelectionCommandWhenGitInfoExistsAndNoMultiLineSelection() As Threading.Tasks.Task
            Dim command As OleMenuCommand
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dte = MockDte(
                selection:=New LineSelection(3, 3, 3, 3)
            )

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = Await AddCommandAsync(Commands.CopyLinkToSelection, dte, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy Link to Foo", command.Text)
        End Function


        <Fact()>
        Public Async Function ShowsSelectionCommandWhenGitInfoExistsAndMultipleLinesAreSelected() As Threading.Tasks.Task
            Dim command As OleMenuCommand
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dte = MockDte(
                selection:=New LineSelection(3, 4, 3, 3)
            )

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = Await AddCommandAsync(Commands.CopyLinkToSelection, dte, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy Link to Foo", command.Text)
        End Function

    End Class


    Public Class InvokeMethod

        <Fact()>
        Public Async Function PutsLinkOnClipboardForCurrentFile() As Threading.Tasks.Task
            Dim command As OleMenuCommand
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim clipboard As Mock(Of IClipboard)
            Dim gitInfo As GitInfo
            Dim linkInfo As LinkInfo


            dte = MockDte(
                activeFileName:="Z:\foo\bar.txt"
            )

            gitInfo = New GitInfo("a", "b")

            handler = New Mock(Of ILinkHandler)
            handler.Setup(Function(x) x.MakeUrl(gitInfo, "Z:\foo\bar.txt", Nothing)).Returns("http://foo.bar")

            linkInfo = New LinkInfo(gitInfo, handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(linkInfo)

            clipboard = New Mock(Of IClipboard)

            command = Await AddCommandAsync(Commands.CopyLinkToCurrentFile, dte, infoProvider.Object, clipboard.Object)
            command.Invoke()

            clipboard.Verify(Sub(x) x.SetText("http://foo.bar"), Times.Once)
        End Function


        <Fact()>
        Public Async Function PutsLinkOnClipboardForSingleLineSelection() As Threading.Tasks.Task
            Dim command As OleMenuCommand
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim clipboard As Mock(Of IClipboard)
            Dim gitInfo As GitInfo
            Dim linkInfo As LinkInfo


            dte = MockDte(
                activeFileName:="Z:\foo\bar.txt",
                selection:=New LineSelection(34, 34, 3, 3)
            )

            gitInfo = New GitInfo("a", "b")

            handler = New Mock(Of ILinkHandler)

            handler.Setup(
                Function(x) x.MakeUrl(gitInfo, "Z:\foo\bar.txt", It.Is(Function(s As LineSelection) s.StartLineNumber = 34 AndAlso s.EndLineNumber = 34))
            ).Returns("http://foo.bar")

            linkInfo = New LinkInfo(gitInfo, handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(linkInfo)

            clipboard = New Mock(Of IClipboard)

            command = Await AddCommandAsync(Commands.CopyLinkToSelection, dte, infoProvider.Object, clipboard.Object)
            command.Invoke()

            clipboard.Verify(Sub(x) x.SetText("http://foo.bar"), Times.Once)
        End Function


        <Fact()>
        Public Async Function PutsLinkOnClipboardForMultipleLineSelection() As Threading.Tasks.Task
            Dim command As OleMenuCommand
            Dim dte As DTE
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim clipboard As Mock(Of IClipboard)
            Dim gitInfo As GitInfo
            Dim linkInfo As LinkInfo


            dte = MockDte(
                activeFileName:="Z:\foo\bar.txt",
                selection:=New LineSelection(21, 38, 3, 3)
            )

            gitInfo = New GitInfo("a", "b")

            handler = New Mock(Of ILinkHandler)

            handler.Setup(
                Function(x) x.MakeUrl(gitInfo, "Z:\foo\bar.txt", It.Is(Function(s As LineSelection) s.StartLineNumber = 21 AndAlso s.EndLineNumber = 38))
            ).Returns("http://foo.bar")

            linkInfo = New LinkInfo(gitInfo, handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(linkInfo)

            clipboard = New Mock(Of IClipboard)

            command = Await AddCommandAsync(Commands.CopyLinkToSelection, dte, infoProvider.Object, clipboard.Object)
            command.Invoke()

            clipboard.Verify(Sub(x) x.SetText("http://foo.bar"), Times.Once)
        End Function

    End Class


    Private Shared Async Function AddCommandAsync(
            id As CommandID,
            dte As DTE,
            infoProvider As ILinkInfoProvider,
            Optional clipboard As IClipboard = Nothing
        ) As Threading.Tasks.Task(Of OleMenuCommand)

        Dim command As CopyLinkToCurrentFileCommand
        Dim commandService As Mock(Of IMenuCommandService)
        Dim commands As List(Of MenuCommand)
        Dim serviceProvider As TestAsyncServiceProvider


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

        command = New CopyLinkToCurrentFileCommand()
        Await command.InitializeAsync(serviceProvider)

        Return commands.Cast(Of OleMenuCommand).First(Function(x) x.CommandID.Equals(id))
    End Function


    Private Shared Function MockDte(
            Optional activeFileName As String = "Z:\foo.txt",
            Optional selection As LineSelection = Nothing
        ) As DTE

        Dim dte As Mock(Of DTE)
        Dim dte2 As Mock(Of DTE2)
        Dim activeDocument As Mock(Of Document)
        Dim textSelection As Mock(Of TextSelection)


        If selection Is Nothing Then
            selection = New LineSelection(1, 1, 1, 1)
        End If

        textSelection = New Mock(Of TextSelection)
        textSelection.SetupGet(Function(x) x.TopLine).Returns(selection.StartLineNumber)
        textSelection.SetupGet(Function(x) x.BottomLine).Returns(selection.EndLineNumber)

        activeDocument = New Mock(Of Document)
        activeDocument.SetupGet(Function(x) x.FullName).Returns(activeFileName)
        activeDocument.SetupGet(Function(x) x.Selection).Returns(textSelection.Object)

        dte = New Mock(Of DTE)
        dte2 = dte.As(Of DTE2)()
        dte2.SetupGet(Function(x) x.ActiveDocument).Returns(activeDocument.Object)

        Return dte.Object
    End Function

End Class
