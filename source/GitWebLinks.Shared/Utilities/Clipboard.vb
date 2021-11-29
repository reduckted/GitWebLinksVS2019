Public Class Clipboard
    Implements IClipboard


    Public Sub SetText(text As String) _
        Implements IClipboard.SetText

        System.Windows.Clipboard.SetText(text)
    End Sub

End Class
