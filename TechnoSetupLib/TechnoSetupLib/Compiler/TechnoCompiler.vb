Imports System.CodeDom.Compiler
Imports System.IO
Imports System.Reflection
Imports System.Text

Friend Class TechnoCompiler
    Private CResults As CompilerResults
    Private CParams As CompilerParameters
    Private providerOptions = New Dictionary(Of String, String)()
    Private CodeProvider As VBCodeProvider

    Public Property CompilerErrors As CompilerErrorCollection

    Public Property ListFilesEmbedResources As New List(Of String)


    Sub New()
        providerOptions.Add("CompilerVersion", "v4.0")
        CodeProvider = New VBCodeProvider(providerOptions)
    End Sub

    Friend Function FnCompileInstaller(Config As SetupInstallModel, OutPutFile As String, TechnoSetupLibPath As String, sManifestPath As String) As Boolean
        Try
            CompilerErrors = New CompilerErrorCollection
            CParams = New CompilerParameters With {
                .CompilerOptions = "/platform:" & Config.TargetCPU.ToString & " /target:winexe ", '& sExtraArgs 'Trim(sAppIcon) & " " & Trim(sManifest) '"/optimize" '"/t:library
                .GenerateInMemory = False,
                .GenerateExecutable = True,
                .IncludeDebugInformation = DebugGeneratedEXE,
                .TempFiles = New TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), False),
                .OutputAssembly = OutPutFile,
                .MainClass = "Mod1"
            }

            Dim sTempFolder As String = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\TechnoSetup\Temporary\" & Replace(Config.AppGuid.ToString, "-", "")
            If Not IO.Directory.Exists(sTempFolder) Then
                Directory.CreateDirectory(sTempFolder)
            End If

            Config.SetupName = Path.GetFileName(OutPutFile)

            If DebugGeneratedEXE Then
                CParams.CompilerOptions &= "/debug:pdbonly"
            Else
                CParams.CompilerOptions &= "/optimize"
            End If

            If Config.RequiresAdministrator Then
                CParams.CompilerOptions &= " /win32manifest:" & sManifestPath
            End If

            'If icons exists, applies icon to the Installer, and saves it as a Embedded Resource to use on the Uninstaller
            If Config.AppIcon <> vbNullString AndAlso File.Exists(Config.AppIcon) AndAlso LCase(IO.Path.GetExtension(Config.AppIcon)) = ".ico" Then
                CParams.CompilerOptions &= " /win32icon:" & Config.AppIcon
                Dim b As Byte() = File.ReadAllBytes(Config.AppIcon)
                b = Zip(b)
                File.WriteAllBytes(sTempFolder & "\Icon.ico", b)
                CParams.EmbeddedResources.Add(sTempFolder & "\Icon.ico")
            End If

            CParams.ReferencedAssemblies.Add(TechnoSetupLibPath)
            CParams.EmbeddedResources.Add(TechnoSetupLibPath)

            For Each FileResource In ListFilesEmbedResources
                CParams.EmbeddedResources.Add(FileResource)
            Next

            Dim sInstallConfigFilePath As String = sTempFolder & "\Config.bin"
            Dim configBytes As Byte() = Config.SerializeToByte
            configBytes = Zip(configBytes)
            File.WriteAllBytes(sInstallConfigFilePath, configBytes)
            CParams.EmbeddedResources.Add(sInstallConfigFilePath)

            CResults = CodeProvider.CompileAssemblyFromSource(CParams, FnGetCompilerCode(True, Config))

            If CResults.Errors.Count <> 0 Then
                Dim sMsgErr As String
                Me.CompilerErrors = CResults.Errors
                sMsgErr = vbNullString
                For Each er As CompilerError In Me.CompilerErrors
                    sMsgErr &= er.ErrorText & vbNewLine
                Next
                Console.WriteLine(vbNewLine & "***** COMPILATION ERROR ****" & vbNewLine & sMsgErr & vbNewLine & "****************************" & vbNewLine)

                If Not ConsoleMode Then
                    MsgError(sMsgErr, "Error")
                Else
                    Console.WriteLine("Press enter key to continue...")
                    Console.ReadLine()
                End If
            Else
                Dim nFileInfo As New FileInfo(OutPutFile)
                Console.WriteLine(nFileInfo.DirectoryName)
                If DebugGeneratedEXE Then
                    Dim asm As Assembly = CResults.CompiledAssembly
                    GeneratedAsm = asm
                    Console.WriteLine("Starting Installer")
                    asm.EntryPoint.Invoke(Nothing, Nothing)
                    If Not AutoExit Then Console.ReadLine()
                End If
                Return True
            End If

        Catch ex As Exception
            MsgErr("FnCompile", ex)
        End Try
        Return False
    End Function

    ''' <summary>
    ''' After Installing complete, uninstaller will be generated
    ''' </summary>
    ''' <param name="Config"></param>
    ''' <param name="AllowDebug"></param>
    ''' <param name="TargetCPU"></param>
    ''' <returns></returns>
    Friend Function FnCompileUninstaller(Config As SetupUnisntallModel, AllowDebug As Boolean, TargetCPU As SetupModelSubClasses.enTargetCPU) As Boolean
        Try
            CParams = New CompilerParameters With {
                .CompilerOptions = "/platform:" & TargetCPU.ToString & " /target:winexe ", '& sExtraArgs 'Trim(sAppIcon) & " " & Trim(sManifest) '"/optimize" '"/t:library
                .GenerateInMemory = False,
                .GenerateExecutable = True,
                .IncludeDebugInformation = AllowDebug,
                .TempFiles = New TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), True),
                .OutputAssembly = "D:\TesteDLL\TechnoSetupLibTeste.exe",
                .MainClass = "Mod1"
            }

            If AllowDebug Then
                CParams.CompilerOptions &= "/debug:pdbonly"
            Else
                CParams.CompilerOptions &= "/optimize"
            End If

            CParams.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly.Location)
            CParams.EmbeddedResources.Add(Assembly.GetExecutingAssembly.Location)

            For Each FileResource In ListFilesEmbedResources
                CParams.EmbeddedResources.Add(FileResource)
            Next

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

        Catch ex As Exception
            MsgErr("FnCompileUninstaller", ex)
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Returns the code that will be compiled for application Startup
    ''' </summary>
    ''' <param name="bForInstaller">True for Installer, False for Uninstaller</param>
    ''' <returns></returns>
    Private Function FnGetCompilerCode(bForInstaller As Boolean, Config As SetupInstallModel) As String
        Dim sb As StringBuilder = New StringBuilder("")
        sb.Append("Imports System" & vbCrLf)
        sb.Append("Imports System.Reflection" & vbCrLf)
        sb.Append("Imports System.Diagnostics" & vbCrLf)
        sb.Append("Imports Microsoft.VisualBasic" & vbCrLf)
        sb.Append("Imports System.Runtime.InteropServices" & vbCrLf)


        sb.Append("<Assembly: GuidAttribute(" & Chr(34) & Config.AppGuid.ToString & Chr(34) & ")> " & vbCrLf)
        sb.Append("<Assembly: AssemblyVersion(" & Chr(34) & Config.SetupVersion & Chr(34) & ")> " & vbCrLf)
        sb.Append("<Assembly: AssemblyFileVersion(" & Chr(34) & Config.SetupVersion & Chr(34) & ")> " & vbCrLf)
        'sb.Append("<Assembly: AssemblyDescription(""teste"")> " & vbCrLf)
        sb.Append("<Assembly: AssemblyTitle(""" & Config.AppName & """)> " & vbCrLf)
        sb.Append("<Assembly: AssemblyProduct(""" & Config.AppName & """)> " & vbCrLf)
        If Not String.IsNullOrWhiteSpace(Config.CopyRight) Then
            sb.Append("<Assembly: AssemblyCopyright(""Copyright @  " & Config.CopyRight & Chr(34) & ")>" & vbCrLf)
        End If
        If Config.CompanyName <> vbNullString Then
            sb.Append("<Assembly: AssemblyCompany(""" & Config.CompanyName & """)> " & vbCrLf)
        End If

        sb.Append("Module Mod1" & vbCrLf & vbCrLf)

        sb.Append("Sub Main()" & vbCrLf)
        sb.Append("AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf LoadFromDLLFolder" & vbCrLf)
        sb.Append("acRun()" & vbCrLf & vbCrLf)
        sb.Append("End Sub" & vbCrLf & vbCrLf)

        sb.Append("Sub acRun()" & vbCrLf)

        'True for Installer
        If bForInstaller Then
            sb.Append("TechnoSetupLib.acStartApp(true)" & vbCrLf)
        Else
            'False for Uninstaller
            sb.Append("TechnoSetupLib.acStartApp(false)" & vbCrLf)
        End If

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
        Return sb.ToString
    End Function
End Class
