Imports LibGit2Sharp
Imports System.IO


Public Class GitInfoFinderTests

    Public Class FindMethod

        <Fact()>
        Public Async Function ReturnsNullWhenNoGitRepositoryFound() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim finder As GitInfoFinder


                finder = Await CreateFinder()

                Assert.Null(finder.Find(dir.FullPath))
            End Using
        End Function


        <Fact()>
        Public Async Function FindsGitRootWhenSolutionIsAtRootOfGitRepository() As Threading.Tasks.Task
            Using temp As New TempDirectory
                Dim finder As GitInfoFinder
                Dim info As GitInfo


                finder = Await CreateFinder()

                Using repo = InitializeRepository(temp.FullPath)
                    repo.Network.Remotes.Add("foo", "bar")
                End Using

                info = finder.Find(temp.FullPath)

                Assert.NotNull(info)
                Assert.Equal(temp.FullPath, info.RootDirectory)
            End Using
        End Function


        <Fact()>
        Public Async Function FindsGitRootWhenSolutionIsUnderneathGitRepository() As Threading.Tasks.Task
            Using temp As New TempDirectory
                Dim finder As GitInfoFinder
                Dim info As GitInfo
                Dim grandchild As String


                finder = Await CreateFinder()

                Using repo = InitializeRepository(temp.FullPath)
                    repo.Network.Remotes.Add("bar", "meep")
                End Using

                grandchild = Path.Combine(temp.FullPath, "child", "grandchild")
                Directory.CreateDirectory(grandchild)

                info = finder.Find(grandchild)

                Assert.NotNull(info)
                Assert.Equal(temp.FullPath, info.RootDirectory)
            End Using
        End Function


        <Fact()>
        Public Async Function ReturnsNullWhenNoGitRemoteFound() As Threading.Tasks.Task
            Using temp As New TempDirectory
                Dim finder As GitInfoFinder
                Dim info As GitInfo


                finder = Await CreateFinder()

                Using repo = InitializeRepository(temp.FullPath)
                End Using

                info = finder.Find(temp.FullPath)

                Assert.Null(info)
            End Using
        End Function


        <Fact()>
        Public Async Function UsesOriginRemoteIfItExists() As Threading.Tasks.Task
            Dim finder As GitInfoFinder
            Dim info As GitInfo


            finder = Await CreateFinder()

            Using temp As New TempDirectory
                Using repo = InitializeRepository(temp.FullPath)
                    repo.Network.Remotes.Add("aaa", "foo")
                    repo.Network.Remotes.Add("origin", "bar")
                End Using

                info = finder.Find(temp.FullPath)

                Assert.Equal("bar", info.RemoteUrl)
            End Using
        End Function


        <Fact()>
        Public Async Function UsesFirstRemoteAlphabeticallyIfNoOriginRemote() As Threading.Tasks.Task
            Dim finder As GitInfoFinder
            Dim info As GitInfo


            finder = Await CreateFinder()

            Using temp As New TempDirectory
                Using repo = InitializeRepository(temp.FullPath)
                    repo.Network.Remotes.Add("ccc", "foo")
                    repo.Network.Remotes.Add("aaa", "bar")
                    repo.Network.Remotes.Add("bbb", "meep")
                End Using

                info = finder.Find(temp.FullPath)

                Assert.Equal("bar", info.RemoteUrl)
            End Using
        End Function


        Private Shared Function InitializeRepository(dir As String) As Repository
            Dim repository As Repository
            Dim fileName As String
            Dim author As Signature


            repository = New Repository(Repository.Init(dir))

            Try
                fileName = Path.Combine(dir, "file")
                File.WriteAllText(fileName, "")

                LibGit2Sharp.Commands.Stage(repository, fileName)

                author = New Signature(New Identity("foo", "foo@foo.com"), Date.Now)
                repository.Commit("Initial", author, author)

            Catch ex As Exception
                repository.Dispose()
                Throw
            End Try

            Return repository
        End Function


        Private Shared Async Function CreateFinder() As Threading.Tasks.Task(Of GitInfoFinder)
            Dim finder As GitInfoFinder


            finder = New GitInfoFinder
            Await finder.InitializeAsync(New TestAsyncServiceProvider)

            Return finder
        End Function

    End Class

End Class
