Imports System.ComponentModel.Composition
Imports EnvDTE


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
        cgCurrentLinkInfo = cgLinkInfoFinder.Find(IO.Path.GetDirectoryName(cgDte.Solution.FullName))
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
