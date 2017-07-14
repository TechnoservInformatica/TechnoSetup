Imports System.IO
Imports System.IO.Compression
Imports System.Reflection
Imports System.Text
Imports System.Windows
Imports System.Windows.Threading
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Windows.Media
Imports System.Windows.Media.Imaging

Public Module mShared

    Public Property InstallConfig As SetupInstallModel
    Public Property AppMainModelView As ModelView.MainModelView
    Public Property SetupLibAssembly As Assembly = Assembly.GetExecutingAssembly

    Private IONICBytes As Byte()
    Public Const vbNewL2 As String = vbNewLine & vbNewLine

    ''' <summary>
    ''' Application MainWindow
    ''' </summary>
    ''' <returns></returns>
    Public Property MyMainWindow As MainWindow

    ''' <summary>
    ''' Activate when on a console app, so messages are printed to console
    ''' </summary>
    ''' <returns></returns>
    Public Property ConsoleMode As Boolean

    ''' <summary>
    ''' If activated after compiling the Generated Installer and Uninstaller will be debugged
    ''' </summary>
    ''' <returns></returns>
    Public Property DebugGeneratedEXE As Boolean
    Public Property GeneratedAsm As Assembly
    ''' <summary>
    ''' True when App Running is an Installer, False when App Running is an Uninstaller
    ''' </summary>
    ''' <returns></returns>
    Friend Property IsInstalling As Boolean

    ''' <summary>
    ''' After Compiling Application it wont trigger a Console.ReadLine(), in case of ConsoleMode
    ''' </summary>
    ''' <returns></returns>
    Public Property AutoExit As Boolean

    Sub New()
        acLoadIONIC()
        'Assembly resolve to load IoNic Zip Lib
        AddHandler AppDomain.CurrentDomain.AssemblyResolve, New ResolveEventHandler(Function(sender As Object, args As ResolveEventArgs)
                                                                                        Dim nome As String = New AssemblyName(args.Name).Name + ".dll"
                                                                                        If args.Name.ToLower.Contains("ionic") Then
                                                                                            For Each asm As Assembly In AppDomain.CurrentDomain.GetAssemblies
                                                                                                If asm.GetName.Name = args.Name Then
                                                                                                    Dim path As String = asm.Location
                                                                                                    Return Assembly.ReflectionOnlyLoad(IONICBytes)
                                                                                                End If
                                                                                            Next
                                                                                            Return Assembly.Load(IONICBytes)
                                                                                        End If
                                                                                    End Function)
    End Sub

    ''' <summary>
    ''' Starts Application
    ''' </summary>
    Public Sub acStartApp(bInstallMode As Boolean)
        If MyMainWindow IsNot Nothing Then Return
        IsInstalling = bInstallMode
        Dim app As New TechnoSetupLib.SetupApp
        app.ShutdownMode = Windows.ShutdownMode.OnMainWindowClose
        app.Run()
    End Sub

    Private Sub acLoadIONIC()
        If IONICBytes Is Nothing OrElse IONICBytes.Count = 0 Then
            Dim strZIP = SetupLibAssembly.GetManifestResourceStream("TechnoSetupLib.Ionic.gzip")
            Using compressionStream As New GZipStream(strZIP, CompressionMode.Decompress)
                Using memStr As New MemoryStream
                    compressionStream.CopyTo(memStr)
                    IONICBytes = memStr.ToArray
                End Using
            End Using

            If IONICBytes Is Nothing OrElse IONICBytes.Count < 1 Then
                Environment.Exit(-1)
            End If
        End If
    End Sub


    <System.Runtime.CompilerServices.Extension>
    Public Function Compile(Config As SetupConfigModel) As Boolean
        Dim sPathTempFiles As String = vbNullString
        Try
            If Config.AppGuid = Nothing OrElse Config.AppGuid = Guid.Empty Then
                If ConsoleMode Then
                    Console.WriteLine("AppGuid is required!" & vbNewLine & "Press enter key to continue...")
                    Console.ReadLine()
                Else
                    MsgError("AppGuid is Required!")
                End If
                Return False
            End If

            If Config.OutPutFile = vbNullString Then
                If ConsoleMode Then
                    Console.WriteLine("OutPutFile is required!" & vbNewLine & "Press enter key to continue...")
                    Console.ReadLine()
                Else
                    MsgError("OutPutFile is Required!")
                End If
                Return False
            End If

            Dim sDirOutput As String = Path.GetDirectoryName(Config.OutPutFile)
            If Not IO.Directory.Exists(sDirOutput) Then
                Directory.CreateDirectory(sDirOutput)
            End If

            If Config.AutoIncrementSetupVersion Then
                acFixVersion(Config)
            End If

            If String.IsNullOrWhiteSpace(Config.SetupVersion) Then
                Config.SetupVersion = "1.0.0.0"
            End If

            sPathTempFiles = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\TechnoSetup\Temporary\" & Replace(Config.AppGuid.ToString, "-", "") 'System.IO.Path.GetTempPath & "\TechnoSetupR3\" & CurrentConfig.Guid.ToString
            If IO.Directory.Exists(sPathTempFiles) Then
                IO.Directory.Delete(sPathTempFiles, True)
            End If
            If Not IO.Directory.Exists(sPathTempFiles) Then
                Directory.CreateDirectory(sPathTempFiles)
            End If

            'Creates a copy of the lib to be an Embedded Resource of the Installer
            Dim sPathAssembly As String = SetupLibAssembly.Location
            If Not IO.File.Exists(sPathAssembly) Then
                sPathAssembly = sPathTempFiles & "\TechnoSetupLib.dll"
                Dim strAssembly = Assembly.GetEntryAssembly.GetManifestResourceStream("TechnoSetupLib.dll")
                If strAssembly Is Nothing Then
                    If ConsoleMode Then
                        Console.WriteLine("Invalid project!" & vbNewLine & "Press enter key to continue...")
                        Console.ReadLine()
                    Else
                        MsgError("Invalid project!")
                    End If
                    Return False
                End If
                Using FileAssembly = File.Open(sPathAssembly, FileMode.OpenOrCreate, FileAccess.Read)
                    strAssembly.CopyTo(FileAssembly)
                End Using
            End If

            Dim configInstall As New SetupInstallModel
            acCloneConfigProperties(Config, configInstall)

            Dim PathZipFile As String = sPathTempFiles & "\Files.zip"

            'Zip Required Files
            If ZipFiles(PathZipFile, Config, configInstall) Then
                Dim comp As New TechnoCompiler
                comp.ListFilesEmbedResources.Add(PathZipFile)

                Dim sManifestPath As String = sPathTempFiles & "\app.manifest"

                'If required create manifest file from resource to temporary folder
                If Config.RequiresAdministrator Then
                    Using Str As IO.Stream = Assembly.GetExecutingAssembly.GetManifestResourceStream($"{Assembly.GetExecutingAssembly.GetName.Name}.app.manifest")
                        Using tempManifest = File.Open(sManifestPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read)
                            Str.CopyTo(tempManifest)
                            comp.ListFilesEmbedResources.Add(sManifestPath)
                        End Using
                    End Using
                End If
                Return comp.FnCompileInstaller(configInstall, Config.OutPutFile, sPathAssembly, sManifestPath)
            End If

        Catch ex As Exception
            MsgErr("Compile", ex,, Not ConsoleMode)
        Finally
            'Delete Temp Files
            DeleteFolder(sPathTempFiles, False, True)
        End Try
        Return False
    End Function

    Private Function ZipFiles(PathZipFile As String, ByRef Config As SetupConfigModel, ByRef ConfigInstall As SetupInstallModel) As Boolean
        Dim ListaFilesToZip As New List(Of FileOrganizer)
        Try
            Console.WriteLine("Verifying Files")
            Dim sPattern, sPath, sDirName As String
            Dim sFileName As String
            If Config.Files.Count > 0 Then
                Dim nFileInfo As FileInfo
                For Each nGroup In Config.Files
                    Dim nCurrentFileOrganizer As New FileOrganizer
                    For Each sDir In nGroup.Dir
                        nCurrentFileOrganizer.Dirs.Add(sDir)
                    Next
                    For Each nFile In nGroup.Files
                        If nFile.Name.Contains("*") Then
                            sPattern = IO.Path.GetFileName(nFile.Name)              'Will Return Filename that will be the pattern like *.dll, or *.*
                            sPath = IO.Path.GetDirectoryName(nFile.Name)            'Will Return Full Path without FileName
                            If Not IO.Directory.Exists(sPath) Then
                                Console.WriteLine("Directory not found: " & sPath)
                                Console.ReadLine()
                                Continue For
                            End If
                            For Each sFile In IO.Directory.GetFiles(sPath, sPattern, IO.SearchOption.TopDirectoryOnly)
                                nFileInfo = New FileInfo(sFile)
                                If nCurrentFileOrganizer.Source.Contains(nFileInfo.FullName) Then
                                    Console.WriteLine("File Replicated: " & sFile)
                                    Continue For
                                End If
                                nCurrentFileOrganizer.Source.Add(sFile)
                            Next

                            If nFile.GACRegister Then
                                For Each sDir In nCurrentFileOrganizer.Dirs
                                    For Each sFile In nCurrentFileOrganizer.Source
                                        sFileName = Path.Combine(sDir, IO.Path.GetFileName(sFile))
                                        If Not ConfigInstall.GACRegister.Any(Function(x) LCase(x) = LCase(sFileName)) Then
                                            ConfigInstall.GACRegister.Add(sFileName)
                                        End If
                                    Next
                                Next
                            End If

                            If Not nFile.TopOnly Then
                                For Each sDir As String In Directory.GetDirectories(sPath)
                                    Try
                                        Dim ListDirs As New List(Of String)
                                        sDirName = New DirectoryInfo(sDir).Name
                                        nCurrentFileOrganizer.Dirs.ForEach(Sub(x) ListDirs.Add(x & "\" & sDirName))
                                        AddSubDirAndFiles(sDir, ListDirs, sPattern, ListaFilesToZip)
                                    Catch ex As Exception
                                        Console.WriteLine(ex.Message & vbNewLine & ex.InnerException?.Message & vbNewLine & ex.StackTrace)
                                    End Try
                                Next
                            End If
                        Else
                            If Not IO.File.Exists(nFile.Name) Then
                                Console.WriteLine("File not found: " & nFile.Name)
                                Console.ReadLine()
                            Else
                                If nCurrentFileOrganizer.Source.Contains(nFile.Name) Then
                                    Console.WriteLine("File Replicated: " & nFile.Name)
                                Else
                                    If nFile.COMRegister Then
                                        For Each sDir In nCurrentFileOrganizer.Dirs
                                            sFileName = Path.Combine(sDir, IO.Path.GetFileName(nFile.Name))
                                            If Not ConfigInstall.COMRegister.Any(Function(x) LCase(x) = LCase(sFileName)) Then
                                                ConfigInstall.COMRegister.Add(sFileName)
                                            End If
                                        Next
                                    End If
                                    If nFile.GACRegister Then
                                        For Each sDir In nCurrentFileOrganizer.Dirs
                                            sFileName = Path.Combine(sDir, IO.Path.GetFileName(nFile.Name))
                                            If Not ConfigInstall.GACRegister.Any(Function(x) LCase(x) = LCase(sFileName)) Then
                                                ConfigInstall.GACRegister.Add(sFileName)
                                            End If
                                        Next
                                    End If
                                    nFileInfo = New FileInfo(nFile.Name)
                                    nCurrentFileOrganizer.Source.Add(nFileInfo.FullName)
                                End If
                            End If
                        End If
                    Next
                    ListaFilesToZip.Add(nCurrentFileOrganizer)
                Next

                Dim nTotalFilesToZip As Integer = ListaFilesToZip.Sum(Function(x) x.Source.Count)
                Console.WriteLine($"Total de {nTotalFilesToZip} para serem Zipados.")
                Dim nCountZip As Integer
                If nTotalFilesToZip > 0 Then
                    Dim nCount As Integer = 0
                    'Zip files will be by FileGroup, Group 1, Group 2

                    Using newFile As Ionic.Zip.ZipFile = New Ionic.Zip.ZipFile(PathZipFile)
                        For Each nFile In ListaFilesToZip
                            If nFile.Source.Count = 0 Then Continue For
                            nCount += 1
                            Dim nGroup As New SetupModelSubClasses.TechnoFilesInstall
                            nGroup.File = nCount
                            Dim sDirs As String = vbNullString
                            For Each sDir In nFile.Dirs
                                sDirs &= sDir & ";"
                            Next
                            sDirs = Strings.Left(sDirs, sDirs.Length - 1)       'Remove last ;
                            nGroup.Dirs = sDirs
                            ConfigInstall.InstallFiles.Add(nGroup)
                            For Each sFile In nFile.Source
                                newFile.AddFile(sFile, nCount)
                                ' newFile.CreateEntryFromFile(sFile, nCount & "\" & IO.Path.GetFileName(sFile))
                                nCountZip += 1
                                Console.WriteLine($"Compactando arquivos: {nCountZip} de {nTotalFilesToZip}")
                                Console.SetCursorPosition(0, Console.CursorTop - 1)
                            Next
                        Next
                        'If ConfigInstall.PathLicence <> vbNullString AndAlso File.Exists(ConfigInstall.PathLicence) Then
                        '    newFile.CreateEntryFromFile(ConfigInstall.PathLicence, "Licence" & IO.Path.GetExtension(ConfigInstall.PathLicence))
                        'End If

                        newFile.Save()
                    End Using
                    Console.SetCursorPosition(0, Console.CursorTop + 1)
                End If

                ConfigInstall.FileCount = ListaFilesToZip.Sum(Function(x) x.Source.Count * x.Dirs.Count)
            End If




            Return True

        Catch ex As Exception
            MsgErr("ZipFiles", ex,, Not ConsoleMode)
        End Try

        Return False
    End Function

    Private Sub AddSubDirAndFiles(sDirToAdd As String, targetDirs As List(Of String), sFilesExtension As String, ListToAdd As List(Of FileOrganizer))
        Dim nFileSubs As New FileOrganizer
        targetDirs.ForEach(Sub(x) nFileSubs.Dirs.Add(x))
        If sFilesExtension = vbNullString Then
            sFilesExtension = "*.*"
        End If

        Dim nFileInfo As FileInfo

        For Each sFileGet As String In Directory.GetFiles(sDirToAdd, sFilesExtension, IO.SearchOption.TopDirectoryOnly)
            nFileInfo = New FileInfo(sFileGet)
            nFileSubs.Source.Add(nFileInfo.FullName)
        Next

        If nFileSubs.Source.Count > 0 AndAlso nFileSubs.Dirs.Count > 0 Then
            ListToAdd.Add(nFileSubs)
        End If

        Dim nSplit As String()
        Dim sDirName As String
        For Each sDir As String In Directory.GetDirectories(sDirToAdd, sFilesExtension)
            sDirName = vbNullString
            Dim ListaDirs As New List(Of String)
            nSplit = sDir.Split("\")
            sDirName = Trim(nSplit.Last)
            If sDirName = vbNullString Then
                sDirName = nSplit(nSplit.Length - 2)
            End If
            nFileSubs.Dirs.ForEach(Sub(x) ListaDirs.Add(x & "\" & sDirName))
            AddSubDirAndFiles(sDir, ListaDirs, sFilesExtension, ListToAdd)
        Next
    End Sub

    Friend Sub DeleteFolder(sFolder As String, bShowMessageOnError As Boolean, Optional bRecursive As Boolean = False)
        Try
            If sFolder <> vbNullString Then
                If IO.Directory.Exists(sFolder) Then
                    Directory.Delete(sFolder, bRecursive)
                End If
            End If
        Catch ex As Exception
            If bShowMessageOnError Then
                MsgErr("DeleteFolder", ex,, Not ConsoleMode)
            End If
        End Try
    End Sub

    Private Sub acFixVersion(ByRef Config As SetupConfigModel)
        Dim nVersion As New Version(Config.SetupVersion)
        Dim nRevision As Long = nVersion.Revision + 1
        Dim nBuild As Long = nVersion.Build
        Dim nMinor As Long = nVersion.Minor
        Dim nMajor As Long = nVersion.Major
        If nRevision > 9999 Then
            nRevision = 0
            nBuild += 1
            If nBuild > 9999 Then
                nBuild = 0
                nMinor += 1
                If nMinor > 9999 Then
                    nMinor = 0
                    nMajor += 1
                End If
            End If
        End If
        nVersion = New Version(nMajor, nMinor, nBuild, nRevision)
        Config.SetupVersion = nVersion.ToString
    End Sub

    Private Sub acCloneConfigProperties(ByRef Config As SetupConfigModel, ByRef ConfigInstall As SetupInstallModel)
        For Each Prop As PropertyInfo In GetType(SetupConfigModel).GetProperties
            For Each propTarget As PropertyInfo In GetType(SetupInstallModel).GetProperties
                If Prop.Name = propTarget.Name Then
                    propTarget.SetValue(ConfigInstall, Prop.GetValue(Config, Nothing), Nothing)
                End If
            Next
        Next
    End Sub

    Public Sub MsgErr(Title As String, ex As Exception, Optional ExtraMsg As String = vbNullString, Optional bShowMsgBox As Boolean = True)
        Dim msg As String = vbNullString

        Dim ExceptionInfo As String = vbNullString, ExceptionInfoInner As String = vbNullString
        If ex IsNot Nothing Then
            'msg = "Erro:" & vbNewL2
            msg = ex.Message & vbNewLine & vbNewLine

            ExceptionInfo = GetExceptionInfo(ex)

            If ExceptionInfo <> vbNullString Then msg &= ExceptionInfo & vbNewLine
            If ex.InnerException IsNot Nothing Then
                ExceptionInfoInner = GetExceptionInfo(ex.InnerException)
                If ExceptionInfoInner <> vbNullString Then
                    ExceptionInfoInner = "InnerEx:" & vbNewLine & ExceptionInfoInner
                End If
                msg = msg & ex.InnerException.Message & vbNewLine
                If ex.InnerException.InnerException IsNot Nothing Then
                    msg = msg & ex.InnerException.InnerException.Message & vbNewLine
                End If
                If ExceptionInfoInner <> vbNullString Then msg &= ExceptionInfoInner & vbNewLine
            End If


        End If

        If ExtraMsg <> vbNullString Then
            msg &= ExtraMsg & vbNewL2
        End If

        msg = "Method: " & Title & vbNewLine & msg

        Console.WriteLine(msg)

        If bShowMsgBox Then
            MsgError(msg, "Erro")   '  Disp.Invoke(Sub() MessageBox.Show(msg, "Erro", MessageBoxButton.OK, MessageBoxImage.Error))
        End If

        If ConsoleMode Then
            Console.WriteLine("Press enter key to continue...")
            Console.ReadLine()
        End If
    End Sub

    Public Function GetExceptionInfo(ex As Exception) As String
        Try
            Dim Result As String = vbNullString
            Dim hr As Integer = Runtime.InteropServices.Marshal.GetHRForException(ex)
            Dim st As StackTrace
            If TypeOf ex Is System.AggregateException Then
                st = New StackTrace(TryCast(ex, System.AggregateException).InnerException, True)
            Else
                st = New StackTrace(ex, True)
            End If

            Dim nCount As Integer = 0

            For Each sf As StackFrame In st.GetFrames
                If sf.GetFileLineNumber() > 0 Then
                    Dim sClassName As FileInfo = New FileInfo(sf.GetFileName())
                    nCount = nCount + 1

                    Result &= "Doc:" & vbTab & sClassName.Name.Replace(".vb", "") & vbNewLine

                    Dim nLine As Long = sf.GetFileLineNumber()
                    If nLine > 0 Then
                        Result &= "Lin:" & vbTab & nLine & vbNewLine & "¢"
                    Else
                        Result &= "¢"
                    End If
                End If
            Next
            If nCount > 5 Then
                Dim arSplit As String() = Split(Result, "¢")
                If arSplit(arSplit.Length - 1) = Nothing Then
                    Array.Resize(arSplit, arSplit.Length - 1)
                End If
                Dim nArrayCount As Long = arSplit.Length
                If nArrayCount >= 5 Then
                    Result = vbNullString
                    For i = 0 To 4
                        Result &= arSplit(i)
                    Next
                End If
            Else
                Result = Replace(Result, "¢", "")
            End If
            Return Result

        Catch exc As Exception
            MsgError("Error returning Line: " & exc.Message, "Error")
        End Try

        Return vbNullString

    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function ToImageSource(icon As System.Drawing.Icon) As ImageSource
        Dim imageSource As ImageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
        Return imageSource
    End Function

#Region "Extensions Configs"

    <System.Runtime.CompilerServices.Extension>
    Public Function SerializeToText(Config As SetupConfigModel) As String
        Return XML.NewSerializeToText(Config)
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function SerializeToByte(Config As SetupConfigModel) As Byte()
        Return XML.NewSerialize(Config)
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function SerializeToText(Config As SetupInstallModel) As String
        Return XML.NewSerializeToText(Config)
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function SerializeToByte(Config As SetupInstallModel) As Byte()
        Return XML.NewSerialize(Config)
    End Function

#End Region

    Public NotInheritable Class XML
        Public Shared Function NewSerialize(obj As Object) As Byte()
            Dim ws As New XmlWriterSettings()
            ws.NewLineHandling = NewLineHandling.Entitize
            Dim serializer = New XmlSerializer(obj.GetType)

            Using memStm = New MemoryStream()
                Using xw = XmlWriter.Create(memStm, ws)
                    serializer.Serialize(xw, obj)
                    Return memStm.ToArray()
                End Using
            End Using
        End Function

        Public Shared Function NewSerializeToText(obj As Object, Optional bRemoveDeclaration As Boolean = True, Optional bRemoveNameSpaces As Boolean = True) As String
            Using memStm = New MemoryStream(NewSerialize(obj))
                Using rd = XmlReader.Create(memStm)
                    Dim xmlDoc As New XmlDocument
                    xmlDoc.Load(rd)
                    If bRemoveNameSpaces Then
                        For Each node As XmlNode In xmlDoc.ChildNodes
                            If Not node.NodeType = XmlNodeType.XmlDeclaration Then
                                node.Attributes.RemoveAll()
                            End If
                        Next
                    End If

                    Return Beautify(xmlDoc, bRemoveDeclaration)
                End Using
            End Using
        End Function

        Private Shared Function stripNS(root As XElement) As XElement
            Return New XElement(root.Name.LocalName, If(root.HasElements, root.Elements().[Select](Function(el) stripNS(el)), DirectCast(root.Value, Object)))

        End Function


        Public Shared Function NewDeSerialize(Of T)(b As Byte()) As T
            Dim Deserializer = New XmlSerializer(GetType(T))
            Using memStm = New MemoryStream(b)
                Return Deserializer.Deserialize(memStm)
            End Using
        End Function

        Public Shared Function NewDeSerialize(Of T)(str As String) As T
            Dim Deserializer = New XmlSerializer(GetType(T))
            Using xmlR As XmlReader = XmlReader.Create(New StringReader(str))
                Return Deserializer.Deserialize(xmlR)
            End Using
        End Function

        Public Shared Function Beautify(doc As XmlDocument, Optional OmitXmlDeclaration As Boolean = True) As String
            '== Formata o Texto XML ==
            Dim sb As New StringBuilder()
            Dim settings As New XmlWriterSettings()
            settings.Indent = True
            settings.IndentChars = "  "
            settings.NewLineChars = vbCr & vbLf
            settings.Encoding = Encoding.Unicode 'ASCIIEncoding.ASCII
            settings.NewLineHandling = NewLineHandling.Replace
            settings.OmitXmlDeclaration = OmitXmlDeclaration

            Using writer As XmlWriter = XmlWriter.Create(sb, settings)
                doc.Save(writer)
            End Using
            Return sb.ToString()
        End Function

    End Class


    Public Function Zip(b As Byte()) As Byte()
        Dim buffer As Byte() = b
        Dim ms As New MemoryStream()
        Using zipStream As New System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, True)
            zipStream.Write(buffer, 0, buffer.Length)
        End Using

        ms.Position = 0
        Dim outStream As New MemoryStream()

        Dim compressed As Byte() = New Byte(ms.Length - 1) {}
        ms.Read(compressed, 0, compressed.Length)

        Dim gzBuffer As Byte() = New Byte(compressed.Length + 3) {}
        System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length)
        System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4)
        Return gzBuffer
    End Function

    Public Function UnZip(b As Byte()) As Byte()
        Dim gzBuffer As Byte() = b
        Using ms As New MemoryStream()
            Dim msgLength As Integer = BitConverter.ToInt32(gzBuffer, 0)
            ms.Write(gzBuffer, 4, gzBuffer.Length - 4)

            Dim buffer As Byte() = New Byte(msgLength - 1) {}

            ms.Position = 0
            Using zipStream As New System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress)
                zipStream.Read(buffer, 0, buffer.Length)
            End Using

            Return buffer
        End Using
    End Function

End Module

Public Module MsgBox2Module

    Private Function MsgGeneric(Message As String, Optional Title As String = vbNullString, Optional Buttons As MessageBoxButton = MessageBoxButton.OK,
                                Optional Image As MessageBoxImage = MessageBoxImage.Exclamation, Optional Owner As Window = Nothing, Optional FooterMessage As String = vbNullString) As MessageBoxResult

        'If CommandArgs.bSilent Then Return MessageBoxResult.None

        Dim Disp As Dispatcher
        If Dispatcher.CurrentDispatcher.Thread.GetApartmentState = System.Threading.ApartmentState.STA Then
            Disp = Dispatcher.CurrentDispatcher
        Else
            Disp = Application.Current.Dispatcher
        End If

        '   Dim Ret As MsgBox2View = Nothing
        Dim Ret As MessageBoxResult
        Disp.Invoke(Sub()
                        ' Dim win As New MsgBox2
                        ' Ret = win.View

                        If Owner Is Nothing AndAlso MyMainWindow IsNot Nothing AndAlso MyMainWindow.IsLoaded Then
                            Owner = MyMainWindow
                        End If

                        'Select Case Image
                        '    Case MessageBoxImage.Information
                        '        win.Title = "Informação"
                        '        Ret.PathIcon = MsgBox2View.PathMsgBoxInfo
                        '    Case MessageBoxImage.Error
                        '        win.Title = "Erro"
                        '        Ret.PathIcon = MsgBox2View.PathMsgBoxError
                        '    Case MessageBoxImage.Exclamation
                        '        win.Title = "Alerta"
                        '           'Padrão: Ret.PathIcon=MsgBox2View.PathMsgBoxAttention
                        '    Case MessageBoxImage.Question
                        '        win.Title = "Pergunta"
                        '        Ret.PathIcon = MsgBox2View.PathMsgBoxInterroga
                        'End Select

                        'If Title = vbNullString Then
                        '    Ret.Title = win.Title
                        'Else
                        '    Ret.Title = Title
                        'End If

                        'Ret.Message = Message
                        'If FooterMessage <> vbNullString Then Ret.FooterMessage = FooterMessage
                        'Ret.ButtonOkIsVisible = (Buttons = MessageBoxButton.OK) OrElse (Buttons = MessageBoxButton.OKCancel)
                        'Ret.ButtonNoIsVisible = (Buttons = MessageBoxButton.YesNo) OrElse (Buttons = MessageBoxButton.YesNoCancel)
                        'Ret.ButtonYesIsVisible = (Buttons = MessageBoxButton.YesNo) OrElse (Buttons = MessageBoxButton.YesNoCancel)
                        'Ret.ButtonCancelIsVisible = (Buttons = MessageBoxButton.YesNoCancel) OrElse (Buttons = MessageBoxButton.OKCancel)

                        'win.ShowDialog()
                        If Owner IsNot Nothing Then
                            Ret = MessageBox.Show(Owner, Message, Title, Buttons, Image)
                        Else
                            Ret = MessageBox.Show(Message, Title, Buttons, Image)
                        End If
                    End Sub)

        Return Ret
    End Function

    Public Sub Msg(Message As String, Optional Title As String = vbNullString, Optional Owner As Window = Nothing, Optional FooterMessage As String = vbNullString)
        MsgGeneric(Message, Title, , MessageBoxImage.Exclamation, Owner, FooterMessage)
    End Sub

    Public Sub MsgInfo(Message As String, Optional Title As String = vbNullString, Optional Owner As Window = Nothing, Optional FooterMessage As String = vbNullString)
        MsgGeneric(Message, Title, , MessageBoxImage.Information, Owner, FooterMessage)
    End Sub

    Public Function MsgQuestion(Message As String, Optional Title As String = vbNullString, Optional Owner As Window = Nothing, Optional FooterMessage As String = vbNullString) As MessageBoxResult
        Return MsgGeneric(Message, Title, MessageBoxButton.YesNo, MessageBoxImage.Question, Owner, FooterMessage)
    End Function

    Public Sub MsgError(Message As String, Optional Title As String = vbNullString, Optional Owner As Window = Nothing, Optional FooterMessage As String = vbNullString)
        MsgGeneric(Message, Title, , MessageBoxImage.Error, Owner, FooterMessage)
    End Sub

End Module