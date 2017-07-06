Imports LibGit2Sharp
Imports System.IO

Public Class BitbucketCloudHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Sub ReturnsBitbucket()
            Assert.Equal("Bitbucket", New BitbucketCloudHandler(MockOptions()).Name)
        End Sub

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <MemberData(NameOf(GetBitbucketCloudRemotes), MemberType:=GetType(BitbucketCloudHandlerTests))>
        Public Sub MatchesBitBucketCloudRemotes(remote As String)
            Dim handler As BitbucketCloudHandler


            handler = New BitbucketCloudHandler(MockOptions({"https://bitbucket.org", "git@bitbucket.org"}))

            Assert.True(handler.IsMatch(remote))
        End Sub


        <Fact()>
        Public Sub DoesNotMatchServerUrlsNotInSettings()
            Dim handler As BitbucketCloudHandler


            handler = New BitbucketCloudHandler(MockOptions({"https://codeplex.com"}))

            Assert.False(handler.IsMatch("https://bitbucket.org/atlassian/atlassian-bamboo_rest.git"))
        End Sub

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(GetBitbucketCloudRemotes), MemberType:=GetType(BitbucketCloudHandlerTests))>
        Public Sub CreatesCorrectLinkFromRemoteUrl(remote As String)
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = New BitbucketCloudHandler(MockOptions())

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/master/lib/puppet/feature/restclient.rb",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWhenServerUrlEndsWithSlash()
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "https://bitbucket.org/atlassian/atlassian-bamboo_rest.git")
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = New BitbucketCloudHandler(MockOptions({"https://bitbucket.org/"}))

                Using LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                End Using

                Assert.Equal(
                    "https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/master/lib/puppet/feature/restclient.rb",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub


        <Fact()>
        Public Sub CreatesCorrectLinkWhenServerUrlEndsWithColon()
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, "git@bitbucket.org:atlassian/atlassian-bamboo_rest.git")
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = New BitbucketCloudHandler(MockOptions({"git@bitbucket.org:"}))

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
                handler = New BitbucketCloudHandler(MockOptions())

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
                handler = New BitbucketCloudHandler(MockOptions())

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
                handler = New BitbucketCloudHandler(MockOptions())

                Using repo = LinkHandlerHelpers.InitializeRepository(dir.FullPath)
                    LibGit2Sharp.Commands.Checkout(repo, repo.CreateBranch("dev"))
                End Using

                Assert.Equal(
                    "https://bitbucket.org/atlassian/atlassian-bamboo_rest/src/dev/lib/puppet/feature/restclient.rb",
                    handler.MakeUrl(info, fileName, Nothing)
                )
            End Using
        End Sub

    End Class


    Public Shared Iterator Function GetBitbucketCloudRemotes() As IEnumerable(Of Object())
        Yield {"https://bitbucket.org/atlassian/atlassian-bamboo_rest.git"}
        Yield {"git@bitbucket.org:atlassian/atlassian-bamboo_rest.git"}
    End Function


    Private Shared Function MockOptions(Optional servers() As String = Nothing) As IOptions
        Dim options As Mock(Of IOptions)


        If servers Is Nothing Then
            servers = {"https://bitbucket.org", "git@bitbucket.org"}
        End If

        options = New Mock(Of IOptions)
        options.SetupGet(Function(x) x.BitbucketCloudUrls).Returns(servers)

        Return options.Object
    End Function

End Class
