Imports Microsoft.VisualStudio.Shell
Imports System.Windows
Imports System.Windows.Controls


Public Class OptionsPageControl

    Public Sub New()
        InitializeComponent()

        HandleEnterKey(GitHubUrlsTextBox, BitbucketCloudUrlsTextBox, BitbucketServerUrlsTextBox)
    End Sub


    Private Sub HandleEnterKey(ParamArray boxes() As TextBox)
        For Each box In boxes
            box.AddHandler(
                UIElementDialogPage.DialogKeyPendingEvent,
                New RoutedEventHandler(AddressOf OnDialogKeyPendingEvent)
            )
        Next box
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


    Public Property GitHubUrls As String
        Get
            Return GitHubUrlsTextBox.Text
        End Get

        Set
            GitHubUrlsTextBox.Text = Value
        End Set
    End Property


    Public Property BitbucketCloudUrls As String
        Get
            Return BitbucketCloudUrlsTextBox.Text
        End Get

        Set
            BitbucketCloudUrlsTextBox.Text = Value
        End Set
    End Property


    Public Property BitbucketServerUrls As String
        Get
            Return BitbucketServerUrlsTextBox.Text
        End Get

        Set
            BitbucketServerUrlsTextBox.Text = Value
        End Set
    End Property

End Class
