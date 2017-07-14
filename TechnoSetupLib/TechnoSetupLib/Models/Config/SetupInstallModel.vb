Imports System.Xml.Serialization

''' <summary>
''' Constains the Installer configuration
''' </summary>

Public Class SetupInstallModel
    Inherits SetupBaseModel

    Public Property Password As String

    <XmlArrayItem(ElementName:="Zip", Type:=GetType(SetupModelSubClasses.TechnoFilesInstall))>
    Public Property InstallFiles As New List(Of SetupModelSubClasses.TechnoFilesInstall)

    ''' <summary>
    ''' Will Contain the Path of the files that should Register with regsvr32.exe
    ''' </summary>
    ''' <returns></returns>
    <XmlArrayItem(ElementName:="File", Type:=GetType(String))>
    Public Property COMRegister As New List(Of String)
    Public Function ShouldSerializeCOMRegister() As Boolean
        Return COMRegister?.Count > 0
    End Function

    ''' <summary>
    ''' Will Contain the Path of the files that should Register with System.EnterpriseServices.Internal.Publish GACInstall
    ''' </summary>
    ''' <returns></returns>
    <XmlArrayItem(ElementName:="File", Type:=GetType(String))>
    Public Property GACRegister As New List(Of String)
    Public Function ShouldSerializeGACRegister() As Boolean
        Return GACRegister?.Count > 0
    End Function

    ''' <summary>
    ''' OutPut Setup TargetCPU
    ''' </summary>
    ''' <returns></returns>
    Public Property TargetCPU As SetupModelSubClasses.enTargetCPU = SetupModelSubClasses.enTargetCPU.AnyCPU

    Public Property SetupName As String

    Public Property FileCount As Long
End Class

