Imports System.Xml.Serialization

<PropertyChanged.ImplementPropertyChanged>
Public MustInherit Class SetupBaseModel
    Public Property AppGuid As Guid = Guid.NewGuid

    ''' <summary>
    ''' Setup Name, if application is registered on Windows registry it will present this Name
    ''' </summary>
    ''' <returns></returns>
    Public Property AppName As String

    Public Property CopyRight As String

    ''' <summary>
    ''' Version of the Application, alphanumeric, if application is registered on Windows registry it will present this value on the Version column
    ''' </summary>
    ''' <returns></returns>
    Public Property AppVersion As String

    ''' <summary>
    ''' Application ICON
    ''' </summary>
    ''' <returns></returns>
    Public Property AppIcon As String

    Public Property CompanyName As String

    ''' <summary>
    ''' The generated Setup Output will request to Run as an Administrator
    ''' </summary>
    ''' <returns></returns>
    Public Property RequiresAdministrator As Boolean = True

    ''' <summary>
    ''' When Adding applications ShortCuts to the SpecialFolder.CommonStartMenu, the folder will be created as SpecialFolder.CommonStartMenu\VALUE OF THIS FIELD
    ''' </summary>
    ''' <returns></returns>
    Public Property Group As String

    Public Property AppPublisher As String
    Public Property AppPublisherURL As String
    Public Property AppSupportURL As String
    Public Property AppUpdatesURL As String
    ''' <summary>
    ''' Setup Version, will be on the AssemblyVersion and AssemblyFileVersion Attributes
    ''' </summary>
    ''' <returns></returns>
    Public Property SetupVersion As String = "1.0.0.0"

    Public Property InstallDeleteFolder As List(Of String)
    Public Function ShouldSerializeInstallDeleteFolder() As Boolean
        Return InstallDeleteFolder IsNot Nothing AndAlso InstallDeleteFolder.Count > 0
    End Function

    <XmlArrayItem(ElementName:="File", Type:=GetType(SetupModelSubClasses.TechnoFile))>
    Public Property InstallDeleteFiles As List(Of SetupModelSubClasses.TechnoFile)
    Public Function ShouldSerializeInstallDeleteFiles() As Boolean
        Return InstallDeleteFiles IsNot Nothing AndAlso InstallDeleteFiles?.Count > 0
    End Function

    ''' <summary>
    ''' Can save Tags, for Example {App}, whenever this tag is used on the XML it will be converted on the value presented on the value corresponding to the Key
    ''' </summary>
    ''' <returns></returns>
    <XmlArrayItem(ElementName:="Tag", Type:=GetType(SetupModelSubClasses.TagsClass))>
    Public Property Tags As New List(Of SetupModelSubClasses.TagsClass)
    Public Function ShouldSerializeTags() As Boolean
        Return Tags IsNot Nothing AndAlso Tags?.Count > 0
    End Function

End Class
