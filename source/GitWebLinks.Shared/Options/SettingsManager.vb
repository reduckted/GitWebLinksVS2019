Imports Microsoft.VisualStudio.Settings
Imports Microsoft.VisualStudio.Shell.Interop
Imports Microsoft.VisualStudio.Shell.Settings


Public Class SettingsManager
    Implements IAsyncInitializable
    Implements ISettingsManager


    Private Const CollectionPath As String = "GitWebLinks"


    Private cgStore As WritableSettingsStore


    Public Async Function InitializeAsync(provider As IAsyncServiceProvider) As Threading.Tasks.Task _
        Implements IAsyncInitializable.InitializeAsync

        Dim serviceProvider As IVsSettingsManager


        serviceProvider = Await provider.GetServiceAsync(Of SVsSettingsManager, IVsSettingsManager)

        cgStore = New ShellSettingsManager(serviceProvider).GetWritableSettingsStore(SettingsScope.UserSettings)
    End Function


    Public Function Contains(name As String) As Boolean _
        Implements ISettingsManager.Contains

        Return cgStore.PropertyExists(CollectionPath, name)
    End Function


    Public Function GetString(name As String) As String _
        Implements ISettingsManager.GetString

        Return cgStore.GetString(CollectionPath, name)
    End Function


    Public Sub SetString(name As String, value As String) _
        Implements ISettingsManager.SetString

        If Not cgStore.CollectionExists(CollectionPath) Then
            cgStore.CreateCollection(CollectionPath)
        End If

        cgStore.SetString(CollectionPath, name, value)
    End Sub

End Class
