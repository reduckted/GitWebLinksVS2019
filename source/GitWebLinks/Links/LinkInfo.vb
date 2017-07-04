Public Class LinkInfo

    Public Sub New(
            gitInfo As GitInfo,
            handler As ILinkHandler
        )

        Me.GitInfo = gitInfo
        Me.Handler = handler
    End Sub


    Public ReadOnly Property GitInfo As GitInfo


    Public ReadOnly Property Handler As ILinkHandler

End Class
