Public Interface ISettingsManager

    Function Contains(name As String) As Boolean


    Function GetString(name As String) As String


    Sub SetString(name As String, value As String)

End Interface