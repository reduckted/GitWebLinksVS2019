Imports LibGit2Sharp


Public Class GitInfoFinder
    Implements IGitInfoFinder


    Public Function Find(solutionDirectory As String) As GitInfo _
        Implements IGitInfoFinder.Find

        Dim root As String


        root = FindGitDirectory(solutionDirectory)

        If root IsNot Nothing Then
            Using repository As New Repository(root)
                Dim remote As Remote


                remote = FindRemote(repository)

                If remote IsNot Nothing Then
                    Return New GitInfo(root, remote.Url)
                End If
            End Using
        End If

        Return Nothing
    End Function


    Private Function FindGitDirectory(startingDirectory As String) As String
        Dim dir As String


        dir = startingDirectory

        Do While dir IsNot Nothing
            If IO.Directory.Exists(IO.Path.Combine(dir, ".git")) Then
                Return dir
            End If

            dir = IO.Path.GetDirectoryName(dir)
        Loop

        Return Nothing
    End Function


    Private Function FindRemote(repository As Repository) As Remote
        ' Use the "origin" remote if it exists; 
        ' otherwise, just use the first remote.
        Return If(
            repository.Network.Remotes("origin"),
            repository.Network.Remotes.OrderBy(Function(x) x.Name).FirstOrDefault()
        )
    End Function

End Class
