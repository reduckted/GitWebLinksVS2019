Public Class GitInfoTests

    <Fact()>
    Public Sub StoresParameters()
        Dim info As GitInfo
        Dim root As String
        Dim remote As String


        root = "Z:\foo"
        remote = "abcdef"

        info = New GitInfo(root, remote)

        Assert.Same(root, info.RootDirectory)
        Assert.Equal(remote, info.RemoteUrl)
    End Sub

End Class
