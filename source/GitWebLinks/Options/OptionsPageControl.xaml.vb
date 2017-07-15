Imports Microsoft.VisualStudio.Shell
Imports System.Windows
Imports System.Windows.Controls


Public Class OptionsPageControl

    Public Sub New()
        InitializeComponent()

        HandleEnterKey(GitHubEnterpriseDataGrid, BitbucketServerDataGrid)
    End Sub


    Private Sub HandleEnterKey(ParamArray grids() As DataGrid)
        For Each grid In grids
            grid.AddHandler(
                UIElementDialogPage.DialogKeyPendingEvent,
                New RoutedEventHandler(AddressOf OnDialogKeyPendingEvent)
            )
        Next grid
    End Sub


    Private Sub OnDialogKeyPendingEvent(
            sender As Object,
            e As RoutedEventArgs
        )

        Dim args As DialogKeyEventArgs


        args = TryCast(e, DialogKeyEventArgs)

        If (args IsNot Nothing) AndAlso (args.Key = Input.Key.Enter) Then
            e.Handled = True
        End If
    End Sub

End Class
