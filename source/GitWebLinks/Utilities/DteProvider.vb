Imports EnvDTE
Imports Microsoft.VisualStudio.Shell
Imports System.ComponentModel.Composition


<Export(GetType(IDteProvider))>
Public Class DteProvider
    Implements IDteProvider


    <ImportingConstructor()>
    Public Sub New(serviceProvider As SVsServiceProvider)
        Dte = DirectCast(DirectCast(serviceProvider, IServiceProvider).GetService(GetType(DTE)), DTE)
    End Sub


    Public ReadOnly Property Dte As DTE _
        Implements IDteProvider.Dte

End Class
