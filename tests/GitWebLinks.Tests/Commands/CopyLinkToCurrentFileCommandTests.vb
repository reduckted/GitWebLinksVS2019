Imports EnvDTE
Imports EnvDTE80
Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel.Design


Public Class CopyLinkToCurrentFileCommandTests

    Public Class InitializeMethod

        <Fact()>
        Public Sub InitializesTheCommand()
            Dim command As CopyLinkToCurrentFileCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim commandService As Mock(Of IMenuCommandService)
            Dim addedCommands As List(Of MenuCommand)


            addedCommands = New List(Of MenuCommand)
            infoProvider = New Mock(Of ILinkInfoProvider)

            dteProvider = MockDteProvider()

            commandService = New Mock(Of IMenuCommandService)
            commandService.Setup(Sub(x) x.AddCommand(It.IsAny(Of MenuCommand))).Callback(Sub(x As MenuCommand) addedCommands.Add(x))

            command = New CopyLinkToCurrentFileCommand(dteProvider, infoProvider.Object, Mock.Of(Of IClipboard))
            command.Initialize(commandService.Object)

            Assert.Equal(
                {
                    Commands.CopyLinkToCurrentFile,
                    Commands.CopyLinkToSelection
                },
                addedCommands.Select(Function(x) x.CommandID)
            )
        End Sub

    End Class


    Public Class BeforeQueryStatusMethod

        <Fact()>
        Public Sub HidesTheCommandWhenThereIsNoLinkInfo()
            Dim command As MenuCommand
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim status As Integer
            Dim dteProvider As IDteProvider


            dteProvider = MockDteProvider()

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(DirectCast(Nothing, LinkInfo))

            command = AddCommand(Commands.CopyLinkToCurrentFile, dteProvider, infoProvider.Object)
            status = command.OleStatus

            Assert.False(command.Visible)
        End Sub


        <Fact()>
        Public Sub ShowsCurrentFileCommandWhenGitInfoExists()
            Dim command As OleMenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dteProvider = MockDteProvider()

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = AddCommand(Commands.CopyLinkToCurrentFile, dteProvider, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy Link to Foo", command.Text)
        End Sub


        <Fact()>
        Public Sub ShowsSelectionCommandWhenGitInfoExistsAndNoMultiLineSelection()
            Dim command As OleMenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dteProvider = MockDteProvider(
                selection:=New LineSelection(3, 3)
            )

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = AddCommand(Commands.CopyLinkToSelection, dteProvider, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy Link to Foo", command.Text)
        End Sub


        <Fact()>
        Public Sub ShowsSelectionCommandWhenGitInfoExistsAndMultipleLinesAreSelected()
            Dim command As OleMenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim status As Integer
            Dim info As LinkInfo


            dteProvider = MockDteProvider(
                selection:=New LineSelection(3, 4)
            )

            handler = New Mock(Of ILinkHandler)
            handler.SetupGet(Function(x) x.Name).Returns("Foo")

            info = New LinkInfo(New GitInfo("a", "b"), handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(info)

            command = AddCommand(Commands.CopyLinkToSelection, dteProvider, infoProvider.Object)
            status = command.OleStatus

            Assert.True(command.Visible)
            Assert.Equal("Copy Link to Foo", command.Text)
        End Sub

    End Class


    Public Class InvokeMethod

        <Fact()>
        Public Sub PutsLinkOnClipboardForCurrentFile()
            Dim command As OleMenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim clipboard As Mock(Of IClipboard)
            Dim gitInfo As GitInfo
            Dim linkInfo As LinkInfo


            dteProvider = MockDteProvider(
                activeFileName:="Z:\foo\bar.txt"
            )

            gitInfo = New GitInfo("a", "b")

            handler = New Mock(Of ILinkHandler)
            handler.Setup(Function(x) x.MakeUrl(gitInfo, "Z:\foo\bar.txt", Nothing)).Returns("http://foo.bar")

            linkInfo = New LinkInfo(gitInfo, handler.Object)

            infoProvider = New Mock(Of ILinkInfoProvider)
            infoProvider.SetupGet(Function(x) x.LinkInfo).Returns(linkInfo)

            clipboard = New Mock(Of IClipboard)

            command = AddCommand(Commands.CopyLinkToCurrentFile, dteProvider, infoProvider.Object, clipboard.Object)
            command.Invoke()

            clipboard.Verify(Sub(x) x.SetText("http://foo.bar"), Times.Once)
        End Sub


        <Fact()>
        Public Sub PutsLinkOnClipboardForSingleLineSelection()
            Dim command As OleMenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim clipboard As Mock(Of IClipboard)
            Dim gitInfo As GitInfo
            Dim linkInfo As LinkInfo


            dteProvider = MockDteProvider(
                activeFileName:="Z:\foo\bar.txt",
                selection:=New LineSelection(34, 34)
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

            command = AddCommand(Commands.CopyLinkToSelection, dteProvider, infoProvider.Object, clipboard.Object)
            command.Invoke()

            clipboard.Verify(Sub(x) x.SetText("http://foo.bar"), Times.Once)
        End Sub


        <Fact()>
        Public Sub PutsLinkOnClipboardForMultipleLineSelection()
            Dim command As OleMenuCommand
            Dim dteProvider As IDteProvider
            Dim infoProvider As Mock(Of ILinkInfoProvider)
            Dim handler As Mock(Of ILinkHandler)
            Dim clipboard As Mock(Of IClipboard)
            Dim gitInfo As GitInfo
            Dim linkInfo As LinkInfo


            dteProvider = MockDteProvider(
                activeFileName:="Z:\foo\bar.txt",
                selection:=New LineSelection(21, 38)
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

            command = AddCommand(Commands.CopyLinkToSelection, dteProvider, infoProvider.Object, clipboard.Object)
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

        Dim command As CopyLinkToCurrentFileCommand
        Dim commandService As Mock(Of IMenuCommandService)
        Dim commands As List(Of MenuCommand)


        commands = New List(Of MenuCommand)

        commandService = New Mock(Of IMenuCommandService)
        commandService.Setup(Sub(x) x.AddCommand(It.IsAny(Of MenuCommand))).Callback(Sub(x As MenuCommand) commands.Add(x))

        If clipboard Is Nothing Then
            clipboard = Mock.Of(Of IClipboard)
        End If

        command = New CopyLinkToCurrentFileCommand(dteProvider, infoProvider, clipboard)
        command.Initialize(commandService.Object)

        Return commands.Cast(Of OleMenuCommand).First(Function(x) x.CommandID.Equals(id))
    End Function


    Private Shared Function MockDteProvider(
            Optional activeFileName As String = "Z:\foo.txt",
            Optional selection As LineSelection = Nothing
        ) As IDteProvider

        Dim provider As Mock(Of IDteProvider)
        Dim dte As Mock(Of DTE)
        Dim dte2 As Mock(Of DTE2)
        Dim activeDocument As Mock(Of Document)
        Dim textSelection As Mock(Of TextSelection)


        If selection Is Nothing Then
            selection = New LineSelection(1, 1)
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

        provider = New Mock(Of IDteProvider)
        provider.SetupGet(Function(x) x.Dte).Returns(dte.Object)

        Return provider.Object
    End Function

End Class
