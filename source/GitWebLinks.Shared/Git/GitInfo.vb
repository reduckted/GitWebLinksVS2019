Public Class GitInfo

    Public Sub New(
            rootDirectory As String,
            remoteUrl As String
        )

        Me.RootDirectory = rootDirectory
        Me.RemoteUrl = remoteUrl
    End Sub


    Public ReadOnly Property RootDirectory As String


    Public ReadOnly Property RemoteUrl As String

End Class
