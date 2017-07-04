Imports LibGit2Sharp
Imports System.IO


Public Class LinkHandlerHelpers

    Public Shared Iterator Function GetGitHubRemotes() As IEnumerable(Of Object())
        Yield {"https://github.com/dotnet/corefx.git"}
        Yield {"git@github.com:dotnet/corefx.git"}
    End Function


    Public Shared Iterator Function GetBitbucketCloudRemotes() As IEnumerable(Of Object())
        Yield {"https://bitbucket.org/atlassian/atlassian-bamboo_rest.git"}
        Yield {"git@bitbucket.org:atlassian/atlassian-bamboo_rest.git"}
    End Function


    Public Shared Function GetNonGitHubRemotes() As IEnumerable(Of Object())
        Return GetBitbucketCloudRemotes()
    End Function


    Public Shared Function GetNonBitbucketCloudRemotes() As IEnumerable(Of Object())
        Return GetGitHubRemotes()
    End Function


    Public Shared Function InitializeRepository(dir As String) As Repository
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

End Class
