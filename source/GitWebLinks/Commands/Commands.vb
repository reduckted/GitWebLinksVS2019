Imports System.ComponentModel.Design


Public NotInheritable Class Commands

    Public Shared ReadOnly CommandSet As New Guid("69d3bbf2-3b03-47a3-8048-1aeb0a3cff92")

    Public Shared ReadOnly CopyLinkToSelection As New CommandID(CommandSet, 200)
    Public Shared ReadOnly CopyLinkToCurrentFile As New CommandID(CommandSet, 201)

    Public Shared ReadOnly CopyLinkToFileItem As New CommandID(CommandSet, 300)
    Public Shared ReadOnly CopyLinkToFolderItem As New CommandID(CommandSet, 301)

End Class
