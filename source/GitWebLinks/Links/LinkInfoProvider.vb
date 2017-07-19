Imports EnvDTE
Imports System.ComponentModel.Composition


<Export(GetType(ILinkInfoProvider))>
Public Class LinkInfoProvider
    Implements ILinkInfoProvider


    Private ReadOnly cgDte As DTE
    Private ReadOnly cgLinkInfoFinder As ILinkInfoFinder
    Private ReadOnly cgSolutionEvents As SolutionEvents
    Private cgCurrentLinkInfo As LinkInfo


    <ImportingConstructor()>
    Public Sub New(
            dteProvider As IDteProvider,
            linkInfoFinder As ILinkInfoFinder
        )

        cgDte = dteProvider.Dte
        cgLinkInfoFinder = linkInfoFinder
        cgSolutionEvents = cgDte.Events.SolutionEvents

        AddHandler cgSolutionEvents.AfterClosing, AddressOf OnSolutionClosed
        AddHandler cgSolutionEvents.Opened, AddressOf OnSolutionOpened

        If cgDte.Solution.IsOpen Then
            OnSolutionOpened()
        End If
    End Sub


    Private Sub OnSolutionOpened()
        ' When a new solution is created this event is raised, but the solution doesn't have a file 
        ' name at that stage, so we can't get the link info for it. But, once the project is fully 
        ' created, this event is raised again, and we can get the link info at that point.
        If Not String.IsNullOrEmpty(cgDte.Solution?.FullName) Then
            cgCurrentLinkInfo = cgLinkInfoFinder.Find(IO.Path.GetDirectoryName(cgDte.Solution.FullName))
        End If
    End Sub


    Private Sub OnSolutionClosed()
        cgCurrentLinkInfo = Nothing
    End Sub


    Public ReadOnly Property LinkInfo As LinkInfo _
        Implements ILinkInfoProvider.LinkInfo

        Get
            Return cgCurrentLinkInfo
        End Get
    End Property

End Class
