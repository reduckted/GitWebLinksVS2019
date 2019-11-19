Imports LibGit2Sharp
Imports System.IO


Public Class AzureDevOpsHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Async Function ReturnsVisualStudioTeamServices() As Threading.Tasks.Task
            Assert.Equal("Azure DevOps", (Await CreateHandlerAsync()).Name)
        End Function

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(AzureDevOpsHandlerTests))>
        Public Async Function MatchesAzureDevOpsServers(remote As String) As Threading.Tasks.Task
            Dim handler As AzureDevOpsHandler


            handler = Await CreateHandlerAsync()

            Assert.True(handler.IsMatch(remote))
        End Function


        <Fact()>
        Public Async Function DoesNotMatchOtherServerUrls() As Threading.Tasks.Task
            Dim handler As AzureDevOpsHandler


            handler = Await CreateHandlerAsync()

            Assert.False(handler.IsMatch("https://codeplex.com/foo/bar.git"))
        End Function

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(AzureDevOpsHandlerTests))>
        Public Async Function CreatesCorrectLinkFromRemoteUrl(remote As String) As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As AzureDevOpsHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://dev.azure.com/user/MyProject/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBmaster",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(AzureDevOpsHandlerTests))>
        Public Async Function CreatesCorrectLinkWhenPathContainsSpace(remote As String) As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As AzureDevOpsHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\sub dir\file.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://dev.azure.com/user/MyProject/_git/MyRepo?path=%2Fsrc%2Fsub%20dir%2Ffile.cs&version=GBmaster&line=2&lineStartColumn=2&lineEndColumn=2",
                    handler.MakeUrl(info, fileName, New LineSelection(2, 2, 2, 2))
                )
            End Using
        End Function


        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(AzureDevOpsHandlerTests))>
        Public Async Function CreatesCorrectLinkWithSingleLineSelection(remote As String) As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As AzureDevOpsHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://dev.azure.com/user/MyProject/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBmaster&line=2&lineStartColumn=2&lineEndColumn=2",
                    handler.MakeUrl(info, fileName, New LineSelection(2, 2, 2, 2))
                )
            End Using
        End Function


        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(AzureDevOpsHandlerTests))>
        Public Async Function CreatesCorrectLinkWithMultiLineSelection(remote As String) As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As AzureDevOpsHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://dev.azure.com/user/MyProject/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBmaster&line=1&lineStartColumn=1&lineEndColumn=1&lineEnd=3",
                    handler.MakeUrl(info, fileName, New LineSelection(1, 3, 1, 1))
                )
            End Using
        End Function


        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(AzureDevOpsHandlerTests))>
        Public Async Function UsesCurrentBranch(remote As String) As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As AzureDevOpsHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = Await CreateHandlerAsync(linkType:=LinkType.Branch)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    LibGit2Sharp.Commands.Checkout(repo, repo.CreateBranch("feature/work"))
                End Using

                Assert.Equal(
                    "https://dev.azure.com/user/MyProject/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GBfeature%2Fwork",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(AzureDevOpsHandlerTests))>
        Public Async Function UsesCurrentHash(remote As String) As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As AzureDevOpsHandler
                Dim info As GitInfo
                Dim fileName As String
                Dim sha As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "src\file.cs")
                handler = Await CreateHandlerAsync(linkType:=LinkType.Hash)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    sha = repo.Head.Tip.Sha
                End Using

                Assert.Equal(
                    $"https://dev.azure.com/user/MyProject/_git/MyRepo?path=%2Fsrc%2Ffile.cs&version=GC{sha}",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function

    End Class


    Public Shared Iterator Function GetRemotes() As IEnumerable(Of Object())
        Yield {"https://user@dev.azure.com/user/MyProject/_git/MyRepo"}
        Yield {"git@ssh.dev.azure.com:v3/user/MyProject/MyRepo"}
    End Function


    Private Shared Async Function CreateHandlerAsync(Optional linkType As LinkType = LinkType.Branch) As Threading.Tasks.Task(Of AzureDevOpsHandler)
        Dim options As Mock(Of IOptions)
        Dim provider As TestAsyncServiceProvider
        Dim handler As AzureDevOpsHandler


        options = New Mock(Of IOptions)
        options.SetupGet(Function(x) x.LinkType).Returns(linkType)

        provider = New TestAsyncServiceProvider
        provider.AddService(options.Object)

        handler = New AzureDevOpsHandler
        Await handler.InitializeAsync(provider)

        Return handler
    End Function

End Class
