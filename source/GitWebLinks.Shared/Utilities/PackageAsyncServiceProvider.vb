Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.Shell


Public Class PackageAsyncServiceProvider
    Implements IAsyncServiceProvider


    Private ReadOnly cgPackage As AsyncPackage


    Public Sub New(package As AsyncPackage)
        cgPackage = package
    End Sub


    Public Function GetServiceAsync(Of T)() As Task(Of T) _
        Implements IAsyncServiceProvider.GetServiceAsync

        Return GetServiceAsync(Of T, T)()
    End Function


    Public Async Function GetServiceAsync(Of TInterface, TResult)() As Task(Of TResult) _
        Implements IAsyncServiceProvider.GetServiceAsync

        Return DirectCast(Await cgPackage.GetServiceAsync(GetType(TInterface)), TResult)
    End Function

End Class
