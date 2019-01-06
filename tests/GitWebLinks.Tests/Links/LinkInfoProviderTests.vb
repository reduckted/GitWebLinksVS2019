Imports EnvDTE


Public Class LinkInfoProviderTests

    <Fact()>
    Public Async Function FindsLinkInfoWhenSolutionIsOpened() As Threading.Tasks.Task
        Dim infoProvider As LinkInfoProvider
        Dim serviceProvider As TestAsyncServiceProvider
        Dim dte As DTE
        Dim finder As Mock(Of ILinkInfoFinder)
        Dim solutionEvents As MockSolutionEvents
        Dim info As LinkInfo


        info = New LinkInfo(New GitInfo("a", "b"), Mock.Of(Of ILinkHandler))
        finder = MockFinder(info)

        solutionEvents = Nothing

        dte = MockDte(False, "Z:\foo\bar\meep.sln", solutionEvents)

        serviceProvider = New TestAsyncServiceProvider
        serviceProvider.AddService(dte)
        serviceProvider.AddService(finder.Object)

        infoProvider = New LinkInfoProvider()
        Await infoProvider.InitializeAsync(serviceProvider)

        finder.Verify(Function(x) x.Find(It.IsAny(Of String)), Times.Never)
        Assert.Null(infoProvider.LinkInfo)

        solutionEvents.RaiseOpened()

        finder.Verify(Function(x) x.Find("Z:\foo\bar"), Times.Once)
        Assert.Same(info, infoProvider.LinkInfo)
    End Function


    <Fact()>
    Public Async Function FindsLinkInfoImmediatelyWhenSolutionIsAlreadyOpened() As Threading.Tasks.Task
        Dim infoProvider As LinkInfoProvider
        Dim serviceProvider As TestAsyncServiceProvider
        Dim dte As DTE
        Dim finder As Mock(Of ILinkInfoFinder)
        Dim solutionEvents As MockSolutionEvents
        Dim info As LinkInfo


        info = New LinkInfo(New GitInfo("a", "b"), Mock.Of(Of ILinkHandler))
        finder = MockFinder(info)

        solutionEvents = Nothing

        dte = MockDte(True, "Z:\foo\bar\meep.sln", solutionEvents)

        serviceProvider = New TestAsyncServiceProvider
        serviceProvider.AddService(dte)
        serviceProvider.AddService(finder.Object)

        infoProvider = New LinkInfoProvider
        Await infoProvider.InitializeAsync(serviceProvider)

        finder.Verify(Function(x) x.Find("Z:\foo\bar"), Times.Once)
        Assert.Same(info, infoProvider.LinkInfo)
    End Function


    <Fact()>
    Public Async Function ClearsLinkInfoWhenSolutionIsClosed() As Threading.Tasks.Task
        Dim infoProvider As LinkInfoProvider
        Dim serviceProvider As TestAsyncServiceProvider
        Dim dte As DTE
        Dim finder As Mock(Of ILinkInfoFinder)
        Dim solutionEvents As MockSolutionEvents
        Dim info As LinkInfo


        info = New LinkInfo(New GitInfo("a", "b"), Mock.Of(Of ILinkHandler))
        finder = MockFinder(info)

        solutionEvents = Nothing

        dte = MockDte(False, "Z:\foo\bar\meep.sln", solutionEvents)

        serviceProvider = New TestAsyncServiceProvider
        serviceProvider.AddService(dte)
        serviceProvider.AddService(finder.Object)

        infoProvider = New LinkInfoProvider
        Await infoProvider.InitializeAsync(serviceProvider)

        solutionEvents.RaiseOpened()

        Assert.Same(info, infoProvider.LinkInfo)

        solutionEvents.RaiseAfterClosing()

        Assert.Null(infoProvider.LinkInfo)
    End Function


    Private Shared Function MockDte(
            isSolutionOpen As Boolean,
            solutionFullName As String,
            ByRef solutionEvents As MockSolutionEvents
        ) As DTE

        Dim dte As Mock(Of DTE)
        Dim events As Mock(Of Events)
        Dim solution As Mock(Of Solution)


        solution = New Mock(Of Solution)
        solution.SetupGet(Function(x) x.IsOpen).Returns(isSolutionOpen)
        solution.SetupGet(Function(x) x.FullName).Returns(solutionFullName)

        solutionEvents = New MockSolutionEvents

        events = New Mock(Of Events)
        events.SetupGet(Function(x) x.SolutionEvents).Returns(solutionEvents)

        dte = New Mock(Of DTE)
        dte.SetupGet(Function(x) x.Events).Returns(events.Object)
        dte.SetupGet(Function(x) x.Solution).Returns(solution.Object)

        Return dte.Object
    End Function


    Private Shared Function MockFinder(info As LinkInfo) As Mock(Of ILinkInfoFinder)
        Dim finder As Mock(Of ILinkInfoFinder)


        finder = New Mock(Of ILinkInfoFinder)
        finder.Setup(Function(x) x.Find(It.IsAny(Of String))).Returns(info)

        Return finder
    End Function


    Private Class MockSolutionEvents
        Implements SolutionEvents


        Public Sub RaiseOpened()
            RaiseEvent Opened()
        End Sub


        Public Sub RaiseAfterClosing()
            RaiseEvent AfterClosing()
        End Sub


        Public Event Opened As _dispSolutionEvents_OpenedEventHandler Implements _dispSolutionEvents_Event.Opened


        Public Event BeforeClosing As _dispSolutionEvents_BeforeClosingEventHandler Implements _dispSolutionEvents_Event.BeforeClosing


        Public Event AfterClosing As _dispSolutionEvents_AfterClosingEventHandler Implements _dispSolutionEvents_Event.AfterClosing


        Public Event QueryCloseSolution As _dispSolutionEvents_QueryCloseSolutionEventHandler Implements _dispSolutionEvents_Event.QueryCloseSolution


        Public Event Renamed As _dispSolutionEvents_RenamedEventHandler Implements _dispSolutionEvents_Event.Renamed


        Public Event ProjectAdded As _dispSolutionEvents_ProjectAddedEventHandler Implements _dispSolutionEvents_Event.ProjectAdded


        Public Event ProjectRemoved As _dispSolutionEvents_ProjectRemovedEventHandler Implements _dispSolutionEvents_Event.ProjectRemoved


        Public Event ProjectRenamed As _dispSolutionEvents_ProjectRenamedEventHandler Implements _dispSolutionEvents_Event.ProjectRenamed

    End Class

End Class
