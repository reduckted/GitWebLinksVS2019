Imports Microsoft.VisualStudio.Settings
Imports Microsoft.VisualStudio.Shell
Imports Microsoft.VisualStudio.Shell.Settings
Imports System.ComponentModel.Composition


<Export(GetType(ISettingsManager))>
Public Class SettingsManager
    Implements ISettingsManager


    Private Const CollectionPath As String = "GitWebLinks"


    Private ReadOnly cgStore As WritableSettingsStore


    <ImportingConstructor()>
    Public Sub New(serviceProvider As SVsServiceProvider)
        cgStore = New ShellSettingsManager(serviceProvider).GetWritableSettingsStore(SettingsScope.UserSettings)
    End Sub


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
