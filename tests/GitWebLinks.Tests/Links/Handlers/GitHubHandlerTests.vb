Imports LibGit2Sharp
Imports System.IO


Public Class GitHubHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Async Function ReturnsGitHub() As Threading.Tasks.Task
            Assert.Equal("GitHub", (Await CreateHandlerAsync()).Name)
        End Function

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <MemberData(NameOf(GetCloudRemotes), MemberType:=GetType(GitHubHandlerTests))>
        Public Async Function MatchesGitHubServerUrls(remote As String) As Threading.Tasks.Task
            Dim handler As GitHubHandler


            handler = Await CreateHandlerAsync({New ServerUrl("https://local-github", "git@local-github")})

            Assert.True(handler.IsMatch(remote))
        End Function


        <Theory()>
        <InlineData("https://local-github/dotnet/corefx.git")>
        <InlineData("git@local-github:dotnet/corefx.git")>
        <InlineData("ssh://git@local-github:dotnet/corefx.git")>
        Public Async Function MatchesServerUrlsFromSettings(remote As String) As Threading.Tasks.Task
            Dim handler As GitHubHandler


            handler = Await CreateHandlerAsync({New ServerUrl("https://local-github", "git@local-github")})

            Assert.True(handler.IsMatch(remote))
        End Function


        <Fact()>
        Public Async Function DoesNotMatchServerUrlsNotInSettings() As Threading.Tasks.Task
            Dim handler As GitHubHandler


            handler = Await CreateHandlerAsync({New ServerUrl("https://local-github", "git@local-github")})

            Assert.False(handler.IsMatch("https://codeplex.com/foo/bar.git"))
        End Function

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(GetCloudRemotes), MemberType:=GetType(GitHubHandlerTests))>
        Public Async Function CreatesCorrectLinkFromRemoteUrl(remote As String) As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://github.com/dotnet/corefx/blob/master/src/System.IO.FileSystem/src/System/IO/Directory.cs",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWhenServerUrlEndsWithSlash() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "https://local-github/dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = Await CreateHandlerAsync({New ServerUrl("https://local-github/", "git@local-github")})

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-github/dotnet/corefx/blob/master/src/System.IO.FileSystem/src/System/IO/Directory.cs",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWhenServerUrlEndsWithColon() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@local-github:dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = Await CreateHandlerAsync({New ServerUrl("https://local-github", "git@local-github:")})

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-github/dotnet/corefx/blob/master/src/System.IO.FileSystem/src/System/IO/Directory.cs",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWhenPathContainsSpace() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@github.com:dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\sub dir\Directory.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://github.com/dotnet/corefx/blob/master/src/sub%20dir/Directory.cs",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWithSingleLineSelection() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@github.com:dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://github.com/dotnet/corefx/blob/master/src/System.IO.FileSystem/src/System/IO/Directory.cs#L38",
                    handler.MakeUrl(info, fileName, New LineSelection(38, 38))
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWithMultiLineSelection() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@github.com:dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://github.com/dotnet/corefx/blob/master/src/System.IO.FileSystem/src/System/IO/Directory.cs#L38-L49",
                    handler.MakeUrl(info, fileName, New LineSelection(38, 49))
                )
            End Using
        End Function


        <Fact()>
        Public Async Function UsesCurrentBranch() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@github.com:dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = Await CreateHandlerAsync(linkType:=LinkType.Branch)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    LibGit2Sharp.Commands.Checkout(repo, repo.CreateBranch("feature/thing"))
                End Using

                Assert.Equal(
                    "https://github.com/dotnet/corefx/blob/feature/thing/src/System.IO.FileSystem/src/System/IO/Directory.cs",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function UsesCurrentHash() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String
                Dim sha As String


                info = New GitInfo(dir.FullPath, "git@github.com:dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = Await CreateHandlerAsync(linkType:=LinkType.Hash)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    sha = repo.Head.Tip.Sha
                End Using

                Assert.Equal(
                    $"https://github.com/dotnet/corefx/blob/{sha}/src/System.IO.FileSystem/src/System/IO/Directory.cs",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function

    End Class


    Public Shared Iterator Function GetCloudRemotes() As IEnumerable(Of Object())
        Yield {"https://github.com/dotnet/corefx.git"}
        Yield {"https://username@github.com/dotnet/corefx.git"}
        Yield {"git@github.com:dotnet/corefx.git"}
        Yield {"ssh://git@github.com:dotnet/corefx.git"}
    End Function


    Private Shared Async Function CreateHandlerAsync(
            Optional servers() As ServerUrl = Nothing,
            Optional linkType As LinkType = LinkType.Branch
        ) As Threading.Tasks.Task(Of GitHubHandler)

        Dim options As Mock(Of IOptions)
        Dim provider As TestAsyncServiceProvider
        Dim handler As GitHubHandler


        If servers Is Nothing Then
            servers = {}
        End If

        options = New Mock(Of IOptions)
        options.SetupGet(Function(x) x.GitHubEnterpriseUrls).Returns(servers)
        options.SetupGet(Function(x) x.LinkType).Returns(linkType)

        provider = New TestAsyncServiceProvider
        provider.AddService(options.Object)

        handler = New GitHubHandler()
        Await handler.InitializeAsync(provider)

        Return handler
    End Function

End Class
