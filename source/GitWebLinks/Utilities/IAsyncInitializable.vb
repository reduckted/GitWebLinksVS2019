Imports Microsoft.VisualStudio.Shell
Imports System.Threading


Public Interface IAsyncInitializable

    Function InitializeAsync(provider As IAsyncServiceProvider) As Tasks.Task

End Interface
