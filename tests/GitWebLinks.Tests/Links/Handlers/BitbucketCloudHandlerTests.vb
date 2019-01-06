Imports LibGit2Sharp
Imports System.IO


Public Class BitbucketCloudHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Async Function ReturnsBitbucket() As Threading.Tasks.Task
            Assert.Equal("Bitbucket", (Await CreateHandlerAsync()).Name)
        End Function

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(BitbucketCloudHandlerTests))>
        Public Async Function MatchesBitbucketCloudServers(remote As String) As Threading.Tasks.Task
            Dim handler As BitbucketCloudHandler


            handler = Await CreateHandlerAsync()

            Assert.True(handler.IsMatch(remote))
        End Function


        <Fact()>
        Public Async Function DoesNotMatchOtherServerUrls() As Threading.Tasks.Task
            Dim handler As BitbucketCloudHandler


            handler = Await CreateHandlerAsync()

            Assert.False(handler.IsMatch("https://codeplex.com/foo/bar.git"))
        End Function

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(BitbucketCloudHandlerTests))>
        Public Async Function CreatesCorrectLinkFromRemoteUrl(remote As String) As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/master/lib/puppet/feature/restclient.rb",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWhenPathContainsSpace() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@bitbucket.org:atlassian/atlassian-bamboo_rest.git")
                fileName = Path.Combine(dir.FullPath, "lib\sub dir\restclient.rb")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/master/lib/sub%20dir/restclient.rb",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWithSingleLineSelection() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@bitbucket.org:atlassian/atlassian-bamboo_rest.git")
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/master/lib/puppet/feature/restclient.rb#restclient.rb-2",
                    handler.MakeUrl(info, fileName, New LineSelection(2, 2))
                )
            End Using
        End Function


        <Fact()>
        Public Async Function CreatesCorrectLinkWithMultiLineSelection() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@bitbucket.org:atlassian/atlassian-bamboo_rest.git")
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = Await CreateHandlerAsync()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/master/lib/puppet/feature/restclient.rb#restclient.rb-1:3",
                    handler.MakeUrl(info, fileName, New LineSelection(1, 3))
                )
            End Using
        End Function


        <Fact()>
        Public Async Function UsesCurrentBranch() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@bitbucket.org:atlassian/atlassian-bamboo_rest.git")
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = Await CreateHandlerAsync(linkType:=LinkType.Branch)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    LibGit2Sharp.Commands.Checkout(repo, repo.CreateBranch("feature/thing"))
                End Using

                Assert.Equal(
                    "https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/feature/thing/lib/puppet/feature/restclient.rb",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function UsesCurrentHash() As Threading.Tasks.Task
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String
                Dim sha As String


                info = New GitInfo(dir.FullPath, "git@bitbucket.org:atlassian/atlassian-bamboo_rest.git")
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = Await CreateHandlerAsync(linkType:=LinkType.Hash)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    sha = repo.Head.Tip.Sha
                End Using

                Assert.Equal(
                    $"https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/{sha}/lib/puppet/feature/restclient.rb",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Function

    End Class


    Public Shared Iterator Function GetRemotes() As IEnumerable(Of Object())
        Yield {"https://bitbucket.org/atlassian/atlassian-bamboo_rest.git"}
        Yield {"https://username@bitbucket.org/atlassian/atlassian-bamboo_rest.git"}
        Yield {"git@bitbucket.org:atlassian/atlassian-bamboo_rest.git"}
        Yield {"ssh://git@bitbucket.org:atlassian/atlassian-bamboo_rest.git"}
    End Function


    Private Shared Async Function CreateHandlerAsync(Optional linkType As LinkType = LinkType.Branch) As Threading.Tasks.Task(Of BitbucketCloudHandler)
        Dim options As Mock(Of IOptions)
        Dim provider As TestAsyncServiceProvider
        Dim handler As BitbucketCloudHandler


        options = New Mock(Of IOptions)
        options.SetupGet(Function(x) x.LinkType).Returns(linkType)

        provider = New TestAsyncServiceProvider
        provider.AddService(options.Object)

        handler = New BitbucketCloudHandler()
        Await handler.InitializeAsync(provider)

        Return handler
    End Function

End Class
