Imports LibGit2Sharp
Imports System.IO


Public Class VisualStudioTeamServicesHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Async Function ReturnsVisualStudioTeamServices() As Threading.Tasks.Task
            Assert.Equal("Visual Studio Team Services", (Await CreateHandlerAsync()).Name)
        End Function

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(VisualStudioTeamServicesHandlerTests))>
        Public Async Function MatchesVisualStudioTeamServicesServers(remote As String) As Threading.Tasks.Task
            Dim handler As VisualStudioTeamServicesHandler


            handler = Await CreateHandlerAsync()

            Assert.True(handler.IsMatch(remote))
        End Function


        <Fact()>
        Public Async Function DoesNotMatchOtherServerUrls() As Threading.Tasks.Task
            Dim handler As VisualStudioTeamServicesHandler


            handler = Await CreateHandlerAsync()

            Assert.False(handler.IsMatch("https://codeplex.com/foo/bar.git"))
        End Function

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(GetNonCollectionRemotes), MemberType:=GetType(VisualStudioTeamServicesHandlerTests))>
        Public Async Function CreatesCorrectLinkFromRemoteUrlWithoutCollection(remote As String) As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://foo.visualstudio.com/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBmaster",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Theory()>
        <MemberData(NameOf(GetCollectionRemotes), MemberType:=GetType(VisualStudioTeamServicesHandlerTests))>
        Public Async Function CreatesCorrectLinkFromRemoteUrlWithCollection(remote As String) As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://foo.visualstudio.com/DefaultCollection/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBmaster",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWhenPathContainsSpace() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "ssh://foo@vs-ssh.visualstudio.com:22/_ssh/MyRepo")
                fileName = Path.Combine(dir.FullPath, "src\sub dir\file.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://foo.visualstudio.com/_git/MyRepo?path=%2Fsrc%2Fsub%20dir%2Ffile.cs&version=GBmaster&line=2",
                    handler.MakeUrl(info, fileName, New LineSelection(2, 2))
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWithSingleLineSelection() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "ssh://foo@vs-ssh.visualstudio.com:22/_ssh/MyRepo")
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://foo.visualstudio.com/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBmaster&line=2",
                    handler.MakeUrl(info, fileName, New LineSelection(2, 2))
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWithMultiLineSelection() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "ssh://foo@vs-ssh.visualstudio.com:22/_ssh/MyRepo")
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://foo.visualstudio.com/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBmaster&line=1&lineEnd=3",
                    handler.MakeUrl(info, fileName, New LineSelection(1, 3))
                )
            End Using
        End Function


        <Fact()>
        Public Async Function UsesCurrentBranch() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "ssh://foo@vs-ssh.visualstudio.com:22/_ssh/MyRepo")
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = Await CreateHandlerAsync(linkType:=LinkType.Branch)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    LibGit2Sharp.Commands.Checkout(repo, repo.CreateBranch("feature/work"))
                End Using

                Assert.Equal(
                    "https://foo.visualstudio.com/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBfeature%2Fwork",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function UsesCurrentHash() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As VisualStudioTeamServicesHandler
                Dim info As GitInfo
                Dim fileName As String
                Dim sha As String


                info = New GitInfo(dir.FullPath, "ssh://foo@vs-ssh.visualstudio.com:22/_ssh/MyRepo")
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = Await CreateHandlerAsync(linkType:=LinkType.Hash)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    sha = repo.Head.Tip.Sha
                End Using

                Assert.Equal(
                    $"https://foo.visualstudio.com/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GC{sha}",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function

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


    Private Shared Async Function CreateHandlerAsync(Optional linkType As LinkType = LinkType.Branch) As Threading.Tasks.Task(Of VisualStudioTeamServicesHandler)
        Dim options As Mock(Of IOptions)
        Dim provider As TestAsyncServiceProvider
        Dim handler As VisualStudioTeamServicesHandler


        options = New Mock(Of IOptions)
        options.SetupGet(Function(x) x.LinkType).Returns(linkType)

        provider = New TestAsyncServiceProvider
        provider.AddService(options.Object)

        handler = New VisualStudioTeamServicesHandler
        Await handler.InitializeAsync(provider)

        Return handler
    End Function

End Class
