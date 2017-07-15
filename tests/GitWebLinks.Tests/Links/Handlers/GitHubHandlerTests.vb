Imports LibGit2Sharp
Imports System.IO


Public Class GitHubHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Sub ReturnsGitHub()
            Assert.Equal("GitHub", CreateHandler().Name)
        End Sub

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <InlineData("https://github.com/dotnet/corefx.git")>
        <InlineData("git@github.com:dotnet/corefx.git")>
        <InlineData("ssh://git@github.com:dotnet/corefx.git")>
        Public Sub MatchesGitHubServerUrls(remote As String)
            Dim handler As GitHubHandler


            handler = CreateHandler({New ServerUrl("https://local-github", "git@local-github")})

            Assert.True(handler.IsMatch(remote))
        End Sub


        <Theory()>
        <InlineData("https://local-github/dotnet/corefx.git")>
        <InlineData("git@local-github:dotnet/corefx.git")>
        <InlineData("ssh://git@local-github:dotnet/corefx.git")>
        Public Sub MatchesServerUrlsFromSettings(remote As String)
            Dim handler As GitHubHandler


            handler = CreateHandler({New ServerUrl("https://local-github", "git@local-github")})

            Assert.True(handler.IsMatch(remote))
        End Sub


        <Fact()>
        Public Sub DoesNotMatchServerUrlsNotInSettings()
            Dim handler As GitHubHandler


            handler = CreateHandler({New ServerUrl("https://local-github", "git@local-github")})

            Assert.False(handler.IsMatch("https://codeplex.com/foo/bar.git"))
        End Sub

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(GetCloudRemotes), MemberType:=GetType(GitHubHandlerTests))>
        Public Sub CreatesCorrectLinkFromRemoteUrl(remote As String)
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://github.com/dotnet/corefx/blob/master/src/System.IO.FileSystem/src/System/IO/Directory.cs",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWhenServerUrlEndsWithSlash()
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "https://local-github/dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = CreateHandler({New ServerUrl("https://local-github/", "git@local-github")})

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-github/dotnet/corefx/blob/master/src/System.IO.FileSystem/src/System/IO/Directory.cs",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWhenServerUrlEndsWithColon()
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@local-github:dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = CreateHandler({New ServerUrl("https://local-github", "git@local-github:")})

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://local-github/dotnet/corefx/blob/master/src/System.IO.FileSystem/src/System/IO/Directory.cs",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWithSingleLineSelection()
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@github.com:dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://github.com/dotnet/corefx/blob/master/src/System.IO.FileSystem/src/System/IO/Directory.cs#L38",
                    handler.MakeUrl(info, fileName, New LineSelection(38, 38))
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWithMultiLineSelection()
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@github.com:dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://github.com/dotnet/corefx/blob/master/src/System.IO.FileSystem/src/System/IO/Directory.cs#L38-L49",
                    handler.MakeUrl(info, fileName, New LineSelection(38, 49))
                )
            End Using
        End Sub


        <Fact()>
        Public Sub UsesCurrentBranch()
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@github.com:dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = CreateHandler()

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    LibGit2Sharp.Commands.Checkout(repo, repo.CreateBranch("feature/thing"))
                End Using

                Assert.Equal(
                    "https://github.com/dotnet/corefx/blob/feature/thing/src/System.IO.FileSystem/src/System/IO/Directory.cs",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub

    End Class


    Private Shared Iterator Function GetCloudRemotes() As IEnumerable(Of Object())
        Yield {"https://github.com/dotnet/corefx.git"}
        Yield {"https://username@github.com/dotnet/corefx.git"}
        Yield {"git@github.com:dotnet/corefx.git"}
        Yield {"ssh://git@github.com:dotnet/corefx.git"}
    End Function


    Private Shared Function CreateHandler(Optional servers() As ServerUrl = Nothing) As GitHubHandler
        Dim options As Mock(Of IOptions)


        If servers Is Nothing Then
            servers = {}
        End If

        options = New Mock(Of IOptions)
        options.SetupGet(Function(x) x.GitHubEnterpriseUrls).Returns(servers)

        Return New GitHubHandler(options.Object)
    End Function

End Class
