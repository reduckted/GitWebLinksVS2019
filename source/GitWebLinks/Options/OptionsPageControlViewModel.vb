Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Runtime.CompilerServices


Public Class OptionsPageControlViewModel
    Implements INotifyPropertyChanged


    Private cgUseCurrentHash As Boolean
    Private cgUseCurrentBranch As Boolean
    Private cgEnableDebugLogging As Boolean


    Public Sub New()
        GitHubEnterpriseUrls = New ObservableCollection(Of ServerUrlModel)
        BitbucketServerUrls = New ObservableCollection(Of ServerUrlModel)
    End Sub


    Public Property UseCurrentHash As Boolean
        Get
            Return cgUseCurrentHash
        End Get

        Set
            If cgUseCurrentHash <> Value Then
                cgUseCurrentHash = Value
                RaisePropertyChanged()
            End If
        End Set
    End Property


    Public Property UseCurrentBranch As Boolean
        Get
            Return cgUseCurrentBranch
        End Get

        Set
            If cgUseCurrentBranch <> Value Then
                cgUseCurrentBranch = Value
                RaisePropertyChanged()
            End If
        End Set
    End Property


    Public Property EnableDebugLogging As Boolean
        Get
            Return cgEnableDebugLogging
        End Get

        Set
            If cgEnableDebugLogging <> Value Then
                cgEnableDebugLogging = Value
                RaisePropertyChanged()
            End If
        End Set
    End Property


    Public ReadOnly Property GitHubEnterpriseUrls As ObservableCollection(Of ServerUrlModel)


    Public ReadOnly Property BitbucketServerUrls As ObservableCollection(Of ServerUrlModel)


    Private Sub RaisePropertyChanged(<CallerMemberName()> Optional propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub


    Public Event PropertyChanged As PropertyChangedEventHandler _
        Implements INotifyPropertyChanged.PropertyChanged

End Class
