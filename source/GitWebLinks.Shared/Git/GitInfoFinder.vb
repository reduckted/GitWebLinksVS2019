Imports LibGit2Sharp
Imports System.Threading.Tasks

Public Class GitInfoFinder
    Implements IAsyncInitializable
    Implements IGitInfoFinder


    Private cgLogger As ILogger


    Public Async Function InitializeAsync(provider As IAsyncServiceProvider) As Task _
        Implements IAsyncInitializable.InitializeAsync

        cgLogger = Await provider.GetServiceAsync(Of ILogger)
    End Function


    Public Function Find(solutionDirectory As String) As GitInfo _
        Implements IGitInfoFinder.Find

        Dim root As String


        cgLogger.Log($"Searching for Git repository from solution directory '{solutionDirectory}'.")

        root = FindGitDirectory(solutionDirectory)

        If root IsNot Nothing Then
            cgLogger.Log($"Found Git repository at '{root}'.")

            Using repository As New Repository(root)
                Dim remote As Remote


                remote = FindRemote(repository)

                If remote IsNot Nothing Then
                    cgLogger.Log($"Using remote '{remote.Url}'.")
                    Return New GitInfo(root, remote.Url)

                Else
                    cgLogger.Log("No remotes found.")
                End If
            End Using

        Else
            cgLogger.Log("Git repository not found.")
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
