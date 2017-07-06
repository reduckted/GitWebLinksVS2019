Imports LibGit2Sharp
Imports System.IO


Public Class GitHubHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Sub ReturnsGitHub()
            Assert.Equal("GitHub", New GitHubHandler(MockOptions()).Name)
        End Sub

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <MemberData(NameOf(GetGitHubRemotes), MemberType:=GetType(GitHubHandlerTests))>
        Public Sub MatchesServerUrlsFromSettings(remote As String)
            Dim handler As GitHubHandler


            handler = New GitHubHandler(MockOptions({"https://github.com", "git@github.com"}))

            Assert.True(handler.IsMatch(remote))
        End Sub


        <Fact()>
        Public Sub DoesNotMatchServerUrlsNotInSettings()
            Dim handler As GitHubHandler


            handler = New GitHubHandler(MockOptions({"https://codeplex.com"}))

            Assert.False(handler.IsMatch("https://github.com/dotnet/corefx.git"))
        End Sub

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(GetGitHubRemotes), MemberType:=GetType(GitHubHandlerTests))>
        Public Sub CreatesCorrectLinkFromRemoteUrl(remote As String)
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = New GitHubHandler(MockOptions())

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


                info = New GitInfo(dir.FullPath, "https://github.com/dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = New GitHubHandler(MockOptions({"https://github.com/"}))

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://github.com/dotnet/corefx/blob/master/src/System.IO.FileSystem/src/System/IO/Directory.cs",
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


                info = New GitInfo(dir.FullPath, "git@github.com:dotnet/corefx.git")
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = New GitHubHandler(MockOptions({"git@github.com:"}))

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://github.com/dotnet/corefx/blob/master/src/System.IO.FileSystem/src/System/IO/Directory.cs",
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
                handler = New GitHubHandler(MockOptions())

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
                handler = New GitHubHandler(MockOptions())

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
                handler = New GitHubHandler(MockOptions())

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    LibGit2Sharp.Commands.Checkout(repo, repo.CreateBranch("dev"))
                End Using

                Assert.Equal(
                    "https://github.com/dotnet/corefx/blob/dev/src/System.IO.FileSystem/src/System/IO/Directory.cs",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub

    End Class


    Private Shared Iterator Function GetGitHubRemotes() As IEnumerable(Of Object())
        Yield {"https://github.com/dotnet/corefx.git"}
        Yield {"git@github.com:dotnet/corefx.git"}
    End Function


    Private Shared Function MockOptions(Optional servers() As String = Nothing) As IOptions
        Dim options As Mock(Of IOptions)


        If servers Is Nothing Then
            servers = {"https://github.com", "git@github.com"}
        End If

        options = New Mock(Of IOptions)
        options.SetupGet(Function(x) x.GitHubUrls).Returns(servers)

        Return options.Object
    End Function

End Class
