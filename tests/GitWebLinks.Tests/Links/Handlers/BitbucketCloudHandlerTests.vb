Imports LibGit2Sharp
Imports System.IO

Public Class BitbucketCloudHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Sub ReturnsBitbucket()
            Assert.Equal("Bitbucket", CreateHandler().Name)
        End Sub

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(BitbucketCloudHandlerTests))>
        Public Sub MatchesBitbucketCloudServers(remote As String)
            Dim handler As BitbucketCloudHandler


            handler = CreateHandler()

            Assert.True(handler.IsMatch(remote))
        End Sub


        <Fact()>
        Public Sub DoesNotMatchOtherServerUrls()
            Dim handler As BitbucketCloudHandler


            handler = CreateHandler()

            Assert.False(handler.IsMatch("https://codeplex.com/foo/bar.git"))
        End Sub

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(GetRemotes), MemberType:=GetType(BitbucketCloudHandlerTests))>
        Public Sub CreatesCorrectLinkFromRemoteUrl(remote As String)
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/master/lib/puppet/feature/restclient.rb",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWithSingleLineSelection()
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@bitbucket.org:atlassian/atlassian-bamboo_rest.git")
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/master/lib/puppet/feature/restclient.rb#restclient.rb-2",
                    handler.MakeUrl(info, fileName, New LineSelection(2, 2))
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWithMultiLineSelection()
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@bitbucket.org:atlassian/atlassian-bamboo_rest.git")
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = CreateHandler()

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/master/lib/puppet/feature/restclient.rb#restclient.rb-1:3",
                    handler.MakeUrl(info, fileName, New LineSelection(1, 3))
                )
            End Using
        End Sub


        <Fact()>
        Public Sub UsesCurrentBranch()
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@bitbucket.org:atlassian/atlassian-bamboo_rest.git")
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = CreateHandler(linkType:=LinkType.Branch)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    LibGit2Sharp.Commands.Checkout(repo, repo.CreateBranch("feature/thing"))
                End Using

                Assert.Equal(
                    "https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/feature/thing/lib/puppet/feature/restclient.rb",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Fact()>
        Public Sub UsesCurrentHash()
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String
                Dim sha As String


                info = New GitInfo(dir.FullPath, "git@bitbucket.org:atlassian/atlassian-bamboo_rest.git")
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = CreateHandler(linkType:=LinkType.Hash)

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    sha = repo.Head.Tip.Sha
                End Using

                Assert.Equal(
                    $"https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/{sha}/lib/puppet/feature/restclient.rb",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub

    End Class


    Public Shared Iterator Function GetRemotes() As IEnumerable(Of Object())
        Yield {"https://bitbucket.org/atlassian/atlassian-bamboo_rest.git"}
        Yield {"https://username@bitbucket.org/atlassian/atlassian-bamboo_rest.git"}
        Yield {"git@bitbucket.org:atlassian/atlassian-bamboo_rest.git"}
        Yield {"ssh://git@bitbucket.org:atlassian/atlassian-bamboo_rest.git"}
    End Function


    Private Shared Function CreateHandler(Optional linkType As LinkType = LinkType.Branch) As BitbucketCloudHandler
        Dim options As Mock(Of IOptions)


        options = New Mock(Of IOptions)
        options.SetupGet(Function(x) x.LinkType).Returns(linkType)

        Return New BitbucketCloudHandler(options.Object)
    End Function

End Class
