Imports System.IO


Public NotInheritable Class TempDirectory
    Implements IDisposable


    Public Sub New()
        FullPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"))
        Directory.CreateDirectory(FullPath)
    End Sub


    Public ReadOnly Property FullPath As String


    Public Sub Dispose() _
        Implements IDisposable.Dispose

        ' Git repositories have readonly files, so we need to clear the
        ' attributes first, otherwise we won't be able to delete them.
        For Each f In Directory.GetFiles(FullPath, "*", SearchOption.AllDirectories)
            File.SetAttributes(f, FileAttributes.Normal)
        Next f

        Directory.Delete(FullPath, True)
    End Sub

End Class
