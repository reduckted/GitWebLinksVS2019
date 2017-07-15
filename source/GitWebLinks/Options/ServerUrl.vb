Public Class ServerUrl

    Public Sub New(
            baseUrl As String,
            sshUrl As String
        )

        Me.BaseUrl = baseUrl
        Me.SshUrl = sshUrl
    End Sub


    Public ReadOnly Property BaseUrl As String


    Public ReadOnly Property SshUrl As String

End Class
