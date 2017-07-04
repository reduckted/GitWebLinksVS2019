Imports System.ComponentModel.Composition


<Export(GetType(ILinkInfoFinder))>
Public Class LinkInfoFinder
    Implements ILinkInfoFinder


    Private ReadOnly cgGitInfoFinder As IGitInfoFinder
    Private ReadOnly cgLinkHandlers As List(Of ILinkHandler)


    <ImportingConstructor()>
    Public Sub New(
            gitInfoFinder As IGitInfoFinder,
            <ImportMany()> linkHandlers As IEnumerable(Of ILinkHandler)
        )

        cgGitInfoFinder = gitInfoFinder
        cgLinkHandlers = linkHandlers.ToList()
    End Sub


    Public Function Find(solutionDirectory As String) As LinkInfo _
        Implements ILinkInfoFinder.Find

        Dim gitInfo As GitInfo


        gitInfo = cgGitInfoFinder.Find(solutionDirectory)

        If gitInfo IsNot Nothing Then
            For Each handler In cgLinkHandlers
                If handler.IsMatch(gitInfo.RemoteUrl) Then
                    Return New LinkInfo(gitInfo, handler)
                End If
            Next
        End If

        Return Nothing
    End Function

End Class
