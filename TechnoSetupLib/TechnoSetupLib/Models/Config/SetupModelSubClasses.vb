Imports System.Collections.ObjectModel
Imports System.Xml.Serialization

Public Class SetupModelSubClasses
    <Serializable, PropertyChanged.ImplementPropertyChanged, XmlRoot("File")>
    Public Class TechnoFilesConfig
        <XmlElement("Source")>
        Public Property Files As New ObservableCollection(Of TechnoFile)

        <XmlElement("Dir")>
        Public Property Dir As New ObservableCollection(Of String)

        Public Function ShouldSerializeDir() As Boolean
            Return Dir IsNot Nothing AndAlso Dir.Count > 0
        End Function

        Public Function ShouldSerializeFiles() As Boolean
            Return Files IsNot Nothing AndAlso Files.Count > 0
        End Function
    End Class

    <Serializable, PropertyChanged.ImplementPropertyChanged>
    Public Class TechnoFile
        <XmlText>
        Public Property Name As String

        <XmlAttribute>
        Public Property TopOnly As Boolean
        Public Function ShouldSerializeTopOnly() As Boolean
            Return TopOnly
        End Function

        <XmlAttribute("GAC")>
        Public Property GACRegister As Boolean
        Public Function ShouldSerializeGACRegister() As Boolean
            Return GACRegister
        End Function

        <XmlAttribute("COMRegister")>
        Public Property COMRegister As Boolean
        Public Function ShouldSerializeCOMRegister() As Boolean
            Return COMRegister
        End Function
    End Class

    <PropertyChanged.ImplementPropertyChanged>
    Public Class TagsClass
        <XmlAttribute>
        Public Property TagName As String
        <XmlText>
        Public Property Value As String
    End Class

    <XmlRoot("Zip")>
    Public Class TechnoFilesInstall
        ''' <summary>
        ''' Zip file Number that contains Files
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("N")>
        Public Property File As String

        ''' <summary>
        ''' Target Directories Where the Files will be Extracted To
        ''' </summary>
        ''' <returns></returns>
        <XmlText>
        Public Property Dirs As String
    End Class

    <Serializable>
    Public Enum enTargetCPU
        AnyCPU = 0
        x64 = 1
        x86 = 2
    End Enum
End Class

Friend Class FileOrganizer
    Public Source As New List(Of String)
    Public Dirs As New List(Of String)
End Class