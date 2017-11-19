Imports LibGit2Sharp
Imports System.IO


Public Class BitbucketServerHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Sub ReturnsBitbucket()
            Assert.Equal("Bitbucket", CreateHandler().Name)
        End Sub

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(BitbucketServerHandlerTests))>
        Public Sub MatchesServerUrlsFromSettings(remote As String)
            Dim handler As BitbucketServerHandler


            handler = CreateHandler({New ServerUrl("https://local-bitbucket:7990/context", "git@local-bitbucket:7999")})

            Assert.True(handler.IsMatch(remote))
        End Sub


        <Fact()>
        Public Sub DoesNotMatchServerUrlsNotInSettings()
            Dim handler As BitbucketServerHandler


            handler = CreateHandler()

            Assert.False(handler.IsMatch("https://codeplex.com/foo/bar.git"))
        End Sub

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(BitbucketServerHandlerTests))>
        Public Sub CreatesCorrectLinkFromRemoteUrl(remote As String)
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "lib\server\main.cs")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at=refs%2Fheads%2Fmaster",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(BitbucketServerHandlerTests))>
        Public Sub CreatesCorrectLinkFromHttpRemote(remote As String)
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String


                ' Swap the `https` URL for an `http` URL.
                If remote.StartsWith("https://", StringComparison.Ordinal) Then
                    remote = "http://" & remote.Substring("https://".Length)
                End If

                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "lib\server\main.cs")
                handler = CreateHandler({New ServerUrl("http://local-bitbucket:7990/context", "git@local-bitbucket:7999")})

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "http://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at=refs%2Fheads%2Fmaster",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWhenServerUrlEndsWithSlash()
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, GetHttpsRemoteUrl())
                fileName = Path.Combine(dir.FullPath, "lib\server\main.cs")
                handler = CreateHandler({New ServerUrl("https://local-bitbucket:7990/context/", "git@local-bitbucket:7999")})

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at=refs%2Fheads%2Fmaster",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWithSingleLineSelection()
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, GetGitRemoteUrl())
                fileName = Path.Combine(dir.FullPath, "lib\server\main.cs")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at=refs%2Fheads%2Fmaster#2",
                    handler.MakeUrl(info, fileName, New LineSelection(2, 2))
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWithMultiLineSelection()
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, GetGitRemoteUrl())
                fileName = Path.Combine(dir.FullPath, "lib\server\main.cs")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at=refs%2Fheads%2Fmaster#10-23",
                    handler.MakeUrl(info, fileName, New LineSelection(10, 23))
                )
            End Using
        End Sub


        <Fact()>
        Public Sub UsesCurrentBranch()
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, GetGitRemoteUrl())
                fileName = Path.Combine(dir.FullPath, "lib\server\main.cs")
                handler = CreateHandler()

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    LibGit2Sharp.Commands.Checkout(repo, repo.CreateBranch("feature/thing"))
                End Using

                Assert.Equal(
                    "https://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at=refs%2Fheads%2Ffeature%2Fthing",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub

    End Class


    Public Shared Iterator Function GetRemotes() As IEnumerable(Of Object())
        Yield {GetHttpsRemoteUrl()}
        Yield {GetHttpsRemoteUrl().Replace("https://", "https://username@")}
        Yield {GetGitRemoteUrl()}
        Yield {$"ssh://{GetGitRemoteUrl()}"}
    End Function


    Private Shared Function GetHttpsRemoteUrl() As String
        Return "https://local-bitbucket:7990/context/scm/bb/my-code.git"
    End Function


    Private Shared Function GetGitRemoteUrl() As String
        Return "git@local-bitbucket:7999/bb/my-code.git"
    End Function


    Private Shared Function CreateHandler(Optional servers() As ServerUrl = Nothing) As BitbucketServerHandler
        Dim options As Mock(Of IOptions)


        If servers Is Nothing Then
            servers = {New ServerUrl("https://local-bitbucket:7990/context", "git@local-bitbucket:7999")}
        End If

        options = New Mock(Of IOptions)
        options.SetupGet(Function(x) x.BitbucketServerUrls).Returns(servers)

        Return New BitbucketServerHandler(options.Object)
    End Function

End Class
