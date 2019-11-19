Public Class LineSelection

    Public Sub New(
            startLineNumber As Integer,
            endLineNumber As Integer,
            startColumnNumber As Integer,
            endColumnNumber As Integer
        )

        Me.StartLineNumber = startLineNumber
        Me.EndLineNumber = endLineNumber
        Me.StartColumnNumber = startColumnNumber
        Me.EndColumnNumber = endColumnNumber
    End Sub


    Public ReadOnly Property StartLineNumber As Integer


    Public ReadOnly Property EndLineNumber As Integer

    Public ReadOnly Property StartColumnNumber As Integer

    Public ReadOnly Property EndColumnNumber As Integer
End Class
