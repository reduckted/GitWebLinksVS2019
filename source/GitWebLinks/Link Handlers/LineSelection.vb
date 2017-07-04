Public Class LineSelection

    Public Sub New(
            startLineNumber As Integer,
            endLineNumber As Integer
        )

        Me.StartLineNumber = startLineNumber
        Me.EndLineNumber = endLineNumber
    End Sub


    Public ReadOnly Property StartLineNumber As Integer


    Public ReadOnly Property EndLineNumber As Integer

End Class
