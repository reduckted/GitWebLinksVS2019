Imports LibGit2Sharp
Imports System.IO


Public Class LinkHandlerHelpers

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
