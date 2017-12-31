Imports LibGit2Sharp
Imports System.IO

Public Class VisualStudioTeamServicesHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Sub ReturnsVisualStudioTeamServices()
            Assert.Equal("Visual Studio Team Services", CreateHandler().Name)
        End Sub

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(VisualStudioTeamServicesHandlerTests))>
        Public Sub MatchesVisualStudioTeamServicesServers(remote As String)
            Dim handler As VisualStudioTeamServicesHandler


            handler = CreateHandler()

            Assert.True(handler.IsMatch(remote))
        End Sub


        <Fact()>
        Public Sub DoesNotMatchOtherServerUrls()
            Dim handler As VisualStudioTeamServicesHandler


            handler = CreateHandler()

            Assert.False(handler.IsMatch("https://codeplex.com/foo/bar.git"))
        End Sub

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(GetNonCollectionRemotes), MemberType:=GetType(VisualStudioTeamServicesHandlerTests))>
        Public Sub CreatesCorrectLinkFromRemoteUrlWithoutCollection(remote As String)
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://foo.visualstudio.com/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBmaster",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Theory()>
        <MemberData(NameOf(GetCollectionRemotes), MemberType:=GetType(VisualStudioTeamServicesHandlerTests))>
        Public Sub CreatesCorrectLinkFromRemoteUrlWithCollection(remote As String)
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://foo.visualstudio.com/DefaultCollection/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBmaster",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWhenPathContainsSpace()
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "ssh://foo@vs-ssh.visualstudio.com:22/_ssh/MyRepo")
                fileName = Path.Combine(dir.FullPath, "src\sub dir\file.cs")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://foo.visualstudio.com/_git/MyRepo?path=%2Fsrc%2Fsub%20dir%2Ffile.cs&version=GBmaster&line=2",
                    handler.MakeUrl(info, fileName, New LineSelection(2, 2))
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWithSingleLineSelection()
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "ssh://foo@vs-ssh.visualstudio.com:22/_ssh/MyRepo")
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://foo.visualstudio.com/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBmaster&line=2",
                    handler.MakeUrl(info, fileName, New LineSelection(2, 2))
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWithMultiLineSelection()
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "ssh://foo@vs-ssh.visualstudio.com:22/_ssh/MyRepo")
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://foo.visualstudio.com/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBmaster&line=1&lineEnd=3",
                    handler.MakeUrl(info, fileName, New LineSelection(1, 3))
                )
            End Using
        End Sub


        <Fact()>
        Public Sub UsesCurrentBranch()
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "ssh://foo@vs-ssh.visualstudio.com:22/_ssh/MyRepo")
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = CreateHandler(linkType:=LinkType.Branch)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    LibGit2Sharp.Commands.Checkout(repo, repo.CreateBranch("feature/work"))
                End Using

                Assert.Equal(
                    "https://foo.visualstudio.com/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBfeature%2Fwork",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Fact()>
        Public Sub UsesCurrentHash()
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String
                Dim sha As String


                info = New GitInfo(dir.FullPath, "ssh://foo@vs-ssh.visualstudio.com:22/_ssh/MyRepo")
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = CreateHandler(linkType:=LinkType.Hash)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    sha = repo.Head.Tip.Sha
                End Using

                Assert.Equal(
                    $"https://foo.visualstudio.com/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GC{sha}",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub

    End Class


    Public Shared Function GetRemotes() As IEnumerable(Of Object())
        Return GetNonCollectionRemotes().Concat(GetCollectionRemotes())
    End Function


    Public Shared Iterator Function GetNonCollectionRemotes() As IEnumerable(Of Object())
        Yield {"https://foo.visualstudio.com/_git/MyRepo"}
        Yield {"ssh://foo@vs-ssh.visualstudio.com:22/_ssh/MyRepo"}
    End Function


    Public Shared Iterator Function GetCollectionRemotes() As IEnumerable(Of Object())
        Yield {"https://foo.visualstudio.com/DefaultCollection/_git/MyRepo"}
        Yield {"ssh://foo@vs-ssh.visualstudio.com:22/DefaultCollection/_ssh/MyRepo"}
    End Function


    Private Shared Function CreateHandler(Optional linkType As LinkType = LinkType.Branch) As VisualStudioTeamServicesHandler
        Dim options As Mock(Of IOptions)


        options = New Mock(Of IOptions)
        options.SetupGet(Function(x) x.LinkType).Returns(linkType)

        Return New VisualStudioTeamServicesHandler(options.Object)
    End Function

End Class
