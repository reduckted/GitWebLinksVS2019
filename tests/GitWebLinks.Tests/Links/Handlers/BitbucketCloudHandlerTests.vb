Imports LibGit2Sharp
Imports System.IO

Public Class BitbucketCloudHandlerTests

    Public Class NameProperty

        <Fact()>
        Public Sub ReturnsBitbucket()
            Assert.Equal("Bitbucket", New BitbucketCloudHandler().Name)
        End Sub

    End Class


    Public Class IsMatchMethod

        <Theory()>
        <MemberData(NameOf(LinkHandlerHelpers.GetBitbucketCloudRemotes), MemberType:=GetType(LinkHandlerHelpers))>
        Public Sub MatchesBitBucketCloudRemotes(remote As String)
            Dim handler As BitbucketCloudHandler


            handler = New BitbucketCloudHandler

            Assert.True(handler.IsMatch(remote))
        End Sub


        <Theory()>
        <MemberData(NameOf(LinkHandlerHelpers.GetNonBitbucketCloudRemotes), MemberType:=GetType(LinkHandlerHelpers))>
        Public Sub DoesNotMatchOtherRemotes(remote As String)
            Dim handler As BitbucketCloudHandler


            handler = New BitbucketCloudHandler

            Assert.False(handler.IsMatch(remote))
        End Sub

    End Class


    Public Class MakeUrlMethod

        <Theory()>
        <MemberData(NameOf(LinkHandlerHelpers.GetBitbucketCloudRemotes), MemberType:=GetType(LinkHandlerHelpers))>
        Public Sub CreatesCorrectLinkFromRemoteUrl(remote As String)
            Using dir As New TempDirectory
                Dim handler As BitbucketCloudHandler
                Dim info As GitInfo
                Dim fileName As String


                info = New GitInfo(dir.FullPath, remote)
                fileName = Path.Combine(dir.FullPath, "lib\puppet\feature\restclient.rb")
                handler = New BitbucketCloudHandler

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
                handler = New BitbucketCloudHandler

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
                handler = New BitbucketCloudHandler

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
                handler = New BitbucketCloudHandler

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

End Class
