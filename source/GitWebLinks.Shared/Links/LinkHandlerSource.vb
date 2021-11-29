Imports System.Threading.Tasks


Public Class LinkHandlerSource
    Implements IAsyncInitializable
    Implements ILinkHandlerSource


    Private ReadOnly cgHandlers As New List(Of ILinkHandler)


    Public Async Function InitializeAsync(provider As IAsyncServiceProvider) As Task _
        Implements IAsyncInitializable.InitializeAsync

        Dim types As IEnumerable(Of Type)
        Dim logger As ILogger


        types = (
            From type In Me.GetType().Assembly.GetTypes()
            Where Not type.IsAbstract
            Where GetType(ILinkHandler).IsAssignableFrom(type)
        )

        For Each type In types
            Dim handler As ILinkHandler
            Dim initializable As IAsyncInitializable


            handler = DirectCast(Activator.CreateInstance(type), ILinkHandler)
            initializable = TryCast(handler, IAsyncInitializable)

            If initializable IsNot Nothing Then
                Await initializable.InitializeAsync(provider)
            End If

            cgHandlers.Add(handler)
        Next type

        logger = Await provider.GetServiceAsync(Of ILogger)

        logger.Log("Available link handlers:")

        For Each handler In cgHandlers
            logger.Log($" * {handler.Name}")
        Next handler
    End Function


    Public Function GetHandlers() As IReadOnlyCollection(Of ILinkHandler) _
        Implements ILinkHandlerSource.GetHandlers

        Return cgHandlers
    End Function

End Class
