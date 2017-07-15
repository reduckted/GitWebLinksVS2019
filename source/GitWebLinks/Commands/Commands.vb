Imports System.ComponentModel.Design


Public NotInheritable Class Commands

    Public Shared ReadOnly CommandSet As New Guid("69d3bbf2-3b03-47a3-8048-1aeb0a3cff92")

    Public Shared ReadOnly CopyLinkToSelection As New CommandID(CommandSet, 200)
    Public Shared ReadOnly CopyLinkToCurrentFile As New CommandID(CommandSet, 201)
    Public Shared ReadOnly CopyLinkToSolutionExplorerItem As New CommandID(CommandSet, 202)

End Class
