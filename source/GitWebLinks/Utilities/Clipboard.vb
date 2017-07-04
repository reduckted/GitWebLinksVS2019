Imports System.ComponentModel.Composition


<Export(GetType(IClipboard))>
Public Class Clipboard
    Implements IClipboard


    Public Sub SetText(text As String) _
        Implements IClipboard.SetText

        Windows.Clipboard.SetText(text)
    End Sub

End Class