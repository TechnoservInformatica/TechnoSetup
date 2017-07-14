Imports System.CodeDom.Compiler
Imports System.Globalization
Imports System.Reflection
Imports System.Text

Public Class Class1
    Private CResults As CompilerResults
    Private CParams As CompilerParameters
    Private providerOptions = New Dictionary(Of String, String)()
    Private CodeProvider As VBCodeProvider

    Private m_oCompilerErrors As CompilerErrorCollection
    Public Property CompilerErrors() As CompilerErrorCollection
        Get
            Return m_oCompilerErrors
        End Get
        Set(ByVal Value As CompilerErrorCollection)
            m_oCompilerErrors = Value
        End Set
    End Property

    Sub New()
        m_oCompilerErrors = New CompilerErrorCollection
        providerOptions.Add("CompilerVersion", "v4.0")
        CodeProvider = New VBCodeProvider(providerOptions)
    End Sub

    Public Sub acCreate()
        CParams = New CompilerParameters

        CParams.CompilerOptions = "/platform:anycpu /optimize /target:winexe /debug:pdbonly" '& sExtraArgs 'Trim(sAppIcon) & " " & Trim(sManifest) '"/optimize" '"/t:library
        CParams.GenerateInMemory = False
        CParams.GenerateExecutable = True
        CParams.IncludeDebugInformation = True
        CParams.TempFiles = New TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), True)
        CParams.OutputAssembly = "D:\TesteDLL\TechnoSetupLibTeste.exe"
        CParams.MainClass = "Mod1"

        CParams.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly.Location)
        CParams.EmbeddedResources.Add(Assembly.GetExecutingAssembly.Location)

        Dim sb As StringBuilder = New StringBuilder("")
        sb.Append("Imports System" & vbCrLf)
        sb.Append("Imports System.Reflection" & vbCrLf)
        sb.Append("Imports System.Diagnostics" & vbCrLf)
        sb.Append("Imports Microsoft.VisualBasic" & vbCrLf)
        sb.Append("Imports System.Runtime.InteropServices" & vbCrLf)
        sb.Append("Imports TechnoSetupLib" & vbCrLf)

        sb.Append("Module Mod1" & vbCrLf & vbCrLf)
        sb.Append("Sub Main()" & vbCrLf)
        sb.Append("AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf LoadFromDLLFolder" & vbCrLf)
        'sb.Append("Debugger.Launch()" & vbCrLf & vbCrLf)
        sb.Append("acRun()" & vbCrLf & vbCrLf)
        sb.Append("End Sub" & vbCrLf & vbCrLf)

        sb.Append("Sub acRun()" & vbCrLf)
        sb.Append("TechnoSetupLib.acStartApp()" & vbCrLf)
        sb.Append("End Sub" & vbCrLf & vbCrLf)

        sb.Append("Function LoadFromDLLFolder(ByVal sender As Object, ByVal args As ResolveEventArgs) As Assembly" & vbCrLf & vbCrLf)
        sb.Append("Dim nome As String" & vbCrLf)
        sb.Append("nome = New AssemblyName(args.Name).Name & " & Chr(34) & ".dll" & Chr(34) & vbCrLf)
        sb.Append("if LCase(Nome) = LCase(" & Chr(34) & "TechnoSetupLib.dll" & Chr(34) & ") Then" & vbCrLf)
        sb.Append("Dim str As IO.Stream = Assembly.GetEntryAssembly.GetManifestResourceStream(""TechnoSetupLib.dll"")" & vbCrLf)
        sb.Append("If str IsNot Nothing Then" & vbCrLf)
        sb.Append("Dim b As Byte() = New Byte(Str.Length) {}" & vbCrLf)
        sb.Append("Str.Read(b, 0, str.Length)" & vbCrLf)
        sb.Append("Dim asmRet As Assembly = Assembly.Load(b)" & vbCrLf)
        sb.Append("Str.Close()" & vbCrLf)
        sb.Append("str.Dispose()" & vbCrLf)
        sb.Append("Str = Nothing" & vbCrLf)
        sb.Append("If asmRet IsNot Nothing Then" & vbCrLf)
        sb.Append("Return asmRet" & vbCrLf)
        sb.Append("End If" & vbCrLf)
        sb.Append("End If" & vbCrLf)
        sb.Append("End If" & vbCrLf)
        sb.Append("Return Nothing" & vbCrLf)
        sb.Append("End Function" & vbCrLf & vbCrLf)
        sb.Append("End Module")




        CResults = CodeProvider.CompileAssemblyFromSource(CParams, sb.ToString)

        'Dim fileExtension As String = "pdb"
        'If (CParams.CompilerOptions IsNot Nothing) AndAlso (CultureInfo.InvariantCulture.CompareInfo.IndexOf(CParams.CompilerOptions, "/debug:pdbonly", CompareOptions.IgnoreCase) <> -1) Then
        '    CResults.TempFiles.AddExtension(fileExtension, True)
        'Else
        '    CResults.TempFiles.AddExtension(fileExtension)
        'End If

        If CResults.Errors.Count <> 0 Then
            Dim msg As String
            Me.CompilerErrors = CResults.Errors
            msg = vbNullString
            For Each er As CompilerError In Me.CompilerErrors
                msg &= er.ErrorText & vbNewLine
            Next
            Console.WriteLine(vbNewLine & "***** COMPILATION ERROR ****" & vbNewLine & msg & vbNewLine & "****************************" & vbNewLine)
            'If IDEMode Then
            'MessageBox.Show(msg, "Compilation Error", MessageBoxButton.OK, MessageBoxImage.Error)
            '   MsgError(msg, "Compilation Error")
            'Else
            Console.ReadLine()
        Else
            Dim asm As Assembly = CResults.CompiledAssembly
            Console.WriteLine("Starting app")
            asm.EntryPoint.Invoke(Nothing, Nothing)


            '` Dim proc = Process.Start("D:\TesteDLL\TechnoSetupLibTeste.exe")
            ' Debugger.Break()
            Console.ReadLine()
        End If
        ' Return False
        ' End If
    End Sub

End Class
