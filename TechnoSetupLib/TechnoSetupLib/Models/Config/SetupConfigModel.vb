Imports System.Collections.ObjectModel
Imports System.Xml.Serialization
''' <summary>
''' Setup Configuration, will be Saved as an XML, can be manually edited.
''' </summary>
<PropertyChanged.ImplementPropertyChanged, XmlRoot("TechnoConfig")>
Public Class SetupConfigModel
    Inherits SetupBaseModel

    ''' <summary>
    ''' OutPut Setup TargetCPU
    ''' </summary>
    ''' <returns></returns>
    Public Property TargetCPU As SetupModelSubClasses.enTargetCPU = SetupModelSubClasses.enTargetCPU.AnyCPU

    ''' <summary>
    ''' Name of The OutputFile, must be an EXECUTABLE (.exe)
    ''' </summary>
    ''' <returns></returns>
    Public Property OutPutFile As String

    Public Property AutoIncrementSetupVersion As Boolean

    Public Property Password As String

    <XmlArrayItem(ElementName:="File", Type:=GetType(SetupModelSubClasses.TechnoFilesConfig))>
    Public Property Files As New List(Of SetupModelSubClasses.TechnoFilesConfig)
    Public Function ShouldSerializeFiles() As Boolean
        Return Files IsNot Nothing AndAlso Files?.Count > 0
    End Function

End Class

