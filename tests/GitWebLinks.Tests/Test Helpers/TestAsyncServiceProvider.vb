Imports System.Threading.Tasks


Public Class TestAsyncServiceProvider
    Implements IAsyncServiceProvider


    Private ReadOnly cgServices As New Dictionary(Of Type, Object)


    Public Sub New()
        cgServices.Add(GetType(ILogger), Mock.Of(Of ILogger))
    End Sub


    Public Overloads Sub AddService(Of T)(instance As T)
        cgServices(GetType(T)) = instance
    End Sub


    Public Function GetServiceAsync(Of T)() As Task(Of T) _
        Implements IAsyncServiceProvider.GetServiceAsync

        Return GetServiceAsync(Of T, T)()
    End Function


    Public Function GetServiceAsync(Of TInterface, TResult)() As Task(Of TResult) _
        Implements IAsyncServiceProvider.GetServiceAsync

        Dim service As Object = Nothing


        If cgServices.TryGetValue(GetType(TInterface), service) Then
            Return Task.FromResult(DirectCast(service, TResult))
        End If

        Throw New InvalidOperationException($"Service of type '{GetType(TInterface).Name}' has not been registered.")
    End Function

End Class
