Imports System.ComponentModel
Imports System.Runtime.CompilerServices


Public Class ServerUrlModel
    Implements INotifyPropertyChanged


    Private cgBaseUrl As String
    Private cgSshUrl As String


    Public Property BaseUrl As String
        Get
            Return cgBaseUrl
        End Get

        Set
            If Not String.Equals(cgBaseUrl, Value) Then
                cgBaseUrl = Value
                RaisePropertyChanged()
            End If
        End Set
    End Property


    Public Property SshUrl As String
        Get
            Return cgSshUrl
        End Get

        Set
            If Not String.Equals(cgSshUrl, Value) Then
                cgSshUrl = Value
                RaisePropertyChanged()
            End If
        End Set
    End Property


    Private Sub RaisePropertyChanged(<CallerMemberName()> Optional propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub


    Public Event PropertyChanged As PropertyChangedEventHandler _
        Implements INotifyPropertyChanged.PropertyChanged

End Class
