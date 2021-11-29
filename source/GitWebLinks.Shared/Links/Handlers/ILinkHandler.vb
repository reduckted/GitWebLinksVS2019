Public Interface ILinkHandler

    ReadOnly Property Name As String


    Function IsMatch(remoteUrl As String) As Boolean


    Function MakeUrl(
            gitInfo As GitInfo,
            filePath As String,
            selection As LineSelection
        ) As String

End Interface
