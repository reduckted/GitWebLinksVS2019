Imports LibGit2Sharp
Imports System.IO


Public Class BitbucketServerHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Async Function ReturnsBitbucket() As Threading.Tasks.Task
            Assert.Equal("Bitbucket Server", (Await CreateHandlerAsync()).Name)
        End Function

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(BitbucketServerHandlerTests))>
        Public Async Function MatchesServerUrlsFromSettings(remote As String) As Threading.Tasks.Task
            Dim handler As BitbucketServerHandler


            handler = Await CreateHandlerAsync({New ServerUrl("https://local-bitbucket:7990/context", "git@local-bitbucket:7999")})

            Assert.True(handler.IsMatch(remote))
        End Function

        <Fact()>
        Public Async Function MatchServerUrlsNotInSettingsEmptySsh() As Threading.Tasks.Task
            Dim handler As BitbucketServerHandler


            handler = Await CreateHandlerAsync({New ServerUrl("https://local-bitbucket:7990/context", Nothing)})

            Assert.True(handler.IsMatch(GetHttpsRemoteUrl()))
        End Function

        <Fact()>
        Public Async Function MatchServerUrlsNotInSettingsEmptyBaseUrl() As Threading.Tasks.Task
            Dim handler As BitbucketServerHandler


            handler = Await CreateHandlerAsync({New ServerUrl(Nothing, "git@local-bitbucket:7999")})

            Assert.True(handler.IsMatch(GetGitRemoteUrl()))
        End Function


        <Fact()>
        Public Async Function DoesNotMatchServerUrlsNotInSettings() As Threading.Tasks.Task
            Dim handler As BitbucketServerHandler


            handler = Await CreateHandlerAsync()

            Assert.False(handler.IsMatch("https://codeplex.com/foo/bar.git"))
        End Function

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(BitbucketServerHandlerTests))>
        Public Async Function CreatesCorrectLinkFromRemoteUrl(remote As String) As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "lib\server\main.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at=refs%2Fheads%2Fmaster",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(BitbucketServerHandlerTests))>
        Public Async Function CreatesCorrectLinkFromHttpRemote(remote As String) As Threading.Tasks.Task
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
                handler = Await CreateHandlerAsync({New ServerUrl("http://local-bitbucket:7990/context", "git@local-bitbucket:7999")})

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "http://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at=refs%2Fheads%2Fmaster",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWhenServerUrlEndsWithSlash() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, GetHttpsRemoteUrl())
                fileName = Path.Combine(dir.FullPath, "lib\server\main.cs")
                handler = Await CreateHandlerAsync({New ServerUrl("https://local-bitbucket:7990/context/", "git@local-bitbucket:7999")})

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at=refs%2Fheads%2Fmaster",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWhenPathContainsSpace() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, GetGitRemoteUrl())
                fileName = Path.Combine(dir.FullPath, "lib\sub dir\main.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/sub%20dir/main.cs?at=refs%2Fheads%2Fmaster",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWithSingleLineSelection() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, GetGitRemoteUrl())
                fileName = Path.Combine(dir.FullPath, "lib\server\main.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at=refs%2Fheads%2Fmaster#2",
                    handler.MakeUrl(info, fileName, New LineSelection(2, 2, 1, 1))
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWithMultiLineSelection() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, GetGitRemoteUrl())
                fileName = Path.Combine(dir.FullPath, "lib\server\main.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at=refs%2Fheads%2Fmaster#10-23",
                    handler.MakeUrl(info, fileName, New LineSelection(10, 23, 1, 1))
                )
            End Using
        End Function


        <Fact()>
        Public Async Function UsesCurrentBranch() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, GetGitRemoteUrl())
                fileName = Path.Combine(dir.FullPath, "lib\server\main.cs")
                handler = Await CreateHandlerAsync(linkType:=LinkType.Branch)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    LibGit2Sharp.Commands.Checkout(repo, repo.CreateBranch("feature/thing"))
                End Using

                Assert.Equal(
                    "https://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at=refs%2Fheads%2Ffeature%2Fthing",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function UsesCurrentHash() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketServerHandler
                Dim info As GitInfo
                Dim fileName As String
                Dim sha As String


                info = New GitInfo(dir.FullPath, GetGitRemoteUrl())
                fileName = Path.Combine(dir.FullPath, "lib\server\main.cs")
                handler = Await CreateHandlerAsync(linkType:=LinkType.Hash)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    sha = repo.Head.Tip.Sha
                End Using

                Assert.Equal(
                    $"https://local-bitbucket:7990/context/projects/bb/repos/my-code/browse/lib/server/main.cs?at={sha}",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function
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


    Private Shared Async Function CreateHandlerAsync(
            Optional servers() As ServerUrl = Nothing,
            Optional linkType As LinkType = LinkType.Branch
        ) As Threading.Tasks.Task(Of BitbucketServerHandler)

        Dim options As Mock(Of IOptions)
        Dim provider As TestAsyncServiceProvider
        Dim handler As BitbucketServerHandler


        If servers Is Nothing Then
            servers = {New ServerUrl("https://local-bitbucket:7990/context", "git@local-bitbucket:7999")}
        End If

        options = New Mock(Of IOptions)
        options.SetupGet(Function(x) x.BitbucketServerUrls).Returns(servers)
        options.SetupGet(Function(x) x.LinkType).Returns(linkType)

        provider = New TestAsyncServiceProvider
        provider.AddService(options.Object)

        handler = New BitbucketServerHandler()
        Await handler.InitializeAsync(provider)

        Return handler
    End Function

End Class
