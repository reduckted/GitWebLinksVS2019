Imports EnvDTE
Imports Microsoft.VisualStudio.Shell


Public Class DteProviderTests

    <Fact()>
    Public Sub GetDteFromServiceProvider()
        Dim dteProvider As DteProvider
        Dim serviceProvider As Mock(Of SVsServiceProvider)
        Dim dte As DTE


        dte = Mock.Of(Of DTE)

        serviceProvider = New Mock(Of SVsServiceProvider)
        serviceProvider.Setup(Function(x) x.GetService(GetType(DTE))).Returns(dte)

        dteProvider = New DteProvider(serviceProvider.Object)

        Assert.Same(dte, dteProvider.Dte)
    End Sub

End Class
