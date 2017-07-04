Public Class LinkInfoTests

    <Fact()>
    Public Sub StoresParameters()
        Dim linkInfo As LinkInfo
        Dim gitInfo As GitInfo
        Dim handler As ILinkHandler


        gitInfo = New GitInfo("a", "b")
        handler = Mock.Of(Of ILinkHandler)

        linkInfo = New LinkInfo(gitInfo, handler)

        Assert.Same(gitInfo, linkInfo.GitInfo)
        Assert.Same(handler, linkInfo.Handler)
    End Sub

End Class
