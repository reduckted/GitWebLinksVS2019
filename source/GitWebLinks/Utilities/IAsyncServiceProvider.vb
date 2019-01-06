Imports System.Threading.Tasks


Public Interface IAsyncServiceProvider

    Function GetServiceAsync(Of T)() As Task(Of T)


    Function GetServiceAsync(Of TInterface, TResult)() As Task(Of TResult)

End Interface
