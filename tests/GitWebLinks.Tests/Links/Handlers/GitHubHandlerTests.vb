Imports LibGit2Sharp
Imports System.IO

Public Class GitHubHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Sub ReturnsGitHub()
            Assert.Equal("GitHub", New GitHubHandler().Name)
        End Sub

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <MemberData(NameOf(LinkHandlerHelpers.GetGitHubRemotes), MemberType:=GetType(LinkHandlerHelpers))>
        Public Sub MatchesGitHubRemotes(remote As String)
            Dim handler As GitHubHandler


            handler = New GitHubHandler

            Assert.True(handler.IsMatch(remote))
        End Sub


        <Theory()>
        <MemberData(NameOf(LinkHandlerHelpers.GetNonGitHubRemotes), MemberType:=GetType(LinkHandlerHelpers))>
        Public Sub DoesNotMatchOtherRemotes(remote As String)
            Dim handler As GitHubHandler


            handler = New GitHubHandler

            Assert.False(handler.IsMatch(remote))
        End Sub

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(LinkHandlerHelpers.GetGitHubRemotes), MemberType:=GetType(LinkHandlerHelpers))>
        Public Sub CreatesCorrectLinkFromRemoteUrl(remote As String)
            Using dir As New TempDirectory
                Dim handler As GitHubHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\System.IO.FileSystem\src\System\IO\Directory.cs")
                handler = New GitHubHandler

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
                handler = New GitHubHandler

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
                handler = New GitHubHandler

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
                handler = New GitHubHandler

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

End Class
