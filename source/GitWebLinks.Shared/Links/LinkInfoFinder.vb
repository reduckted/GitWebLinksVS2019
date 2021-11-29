Imports System.Threading.Tasks


Public Class LinkInfoFinder
    Implements IAsyncInitializable
    Implements ILinkInfoFinder


    Private cgGitInfoFinder As IGitInfoFinder
    Private cgLinkHandlers As IReadOnlyCollection(Of ILinkHandler)


    Public Async Function InitializeAsync(provider As IAsyncServiceProvider) As Task _
        Implements IAsyncInitializable.InitializeAsync

        cgGitInfoFinder = Await provider.GetServiceAsync(Of IGitInfoFinder)
        cgLinkHandlers = (Await provider.GetServiceAsync(Of ILinkHandlerSource)).GetHandlers()
    End Function


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
