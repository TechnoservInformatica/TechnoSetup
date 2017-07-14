Imports System.Reflection
Imports System.Windows

Namespace ModelView

    <PropertyChanged.ImplementPropertyChanged>
    Public Class MainModelView

        Public Property ApplicationLog As String
        Public Property CurrentOper As String

        Public Sub acInit()
            If FnExtractFiles() Then
                MsgInfo("Files Extracted!", "Info")
            End If
        End Sub

        Private Function FnExtractFiles() As Boolean
            AppMainModelView.ApplicationLog &= InstallConfig.FileCount & " files to be extracted." & vbNewLine

            Dim nAsm = Assembly.GetEntryAssembly
            If DebugGeneratedEXE AndAlso GeneratedAsm IsNot Nothing Then
                nAsm = GeneratedAsm
            End If

            'ToDo move code to MainWindowView
            'Path of extracted files
            Dim FilesExtracted As New List(Of String)


            Dim sTargetDirs As New List(Of String)
            Dim sFolder As String
            Dim nTotalSize As Long
            Dim nCountFiles As Long
            Dim Progress As Double
            Dim sPathFile As String

            Using str As IO.Stream = nAsm.GetManifestResourceStream("Files.zip")
                Using newFile = Ionic.Zip.ZipFile.Read(str)
                    For Each nFile In InstallConfig.InstallFiles
                        sTargetDirs.Clear()
                        For Each sDir In nFile.Dirs.Split(";")
                            'ToDo, Treat Tags like {system} etc
                            'sTargetDirs.Add(FnHandleTags(sDir))
                            sTargetDirs.Add(sDir)
                        Next

                        For Each sDir In sTargetDirs
                            'sDir = FnHandleTags(sDir)
                            sDir = Replace(sDir, "/", "\")

                            sFolder = vbNullString
                            For Each sNewDir In Split(sDir, "\")
                                sFolder &= sNewDir & "\"
                                If Not IO.Directory.Exists(sFolder) Then
                                    'If Not InstallConfig.UninstallFolders.Contains(sFolder) Then
                                    'InstallConfig.UninstallFolders.Add(sFolder)
                                    'End If
                                End If
                            Next

                            If Not IO.Directory.Exists(sDir) Then
                                IO.Directory.CreateDirectory(sDir)
                            End If
                        Next

                        newFile.ToList.ForEach(Sub(nEntry)
                                                   If IO.Path.GetDirectoryName(nEntry.FileName) = nFile.File Then
                                                       For Each sDir In sTargetDirs
                                                           nTotalSize += nEntry.UncompressedSize
                                                           nCountFiles += 1
                                                           nEntry.FileName = IO.Path.GetFileName(nEntry.FileName)

                                                           If FnExtractFile(nEntry, sDir) Then
                                                               AppMainModelView.ApplicationLog &= $"Extracted: {nEntry.FileName } to {sDir}" & vbNewLine
                                                               FilesExtracted.Add(sPathFile)
                                                               AppMainModelView.CurrentOper = $"Extracting files {nCountFiles} of {InstallConfig.FileCount}..."
                                                               Progress = ((nCountFiles / InstallConfig.FileCount) * 100)
                                                           Else
                                                               AppMainModelView.ApplicationLog &= "Not possible to extract: " & IO.Path.GetFileName(nEntry.FileName) & vbNewLine
                                                           End If
                                                       Next
                                                   End If
                                               End Sub)
                    Next
                End Using
            End Using

            Return True
        End Function

        Private Function FnExtractFile(entry As Ionic.Zip.ZipEntry, ToPath As String) As Boolean
            Try
                entry.Extract(ToPath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently)
                '   entry.ExtractToFile(ToPath, True)
                Return True


            Catch ex As Exception
                'ToDo: check where the file is open

                Dim nProcess As Process()
                'Dim nProcess As Process() = Process.GetProcessesByName(IO.Path.GetFileName(sFileName).Replace(".exe", ""))
                'Dim nProcess getFileProcesses(sFileName)
                If InStr(ToPath, ".exe", CompareMethod.Text) > 0 Then
                    nProcess = Process.GetProcessesByName(IO.Path.GetFileName(ToPath).Replace(".exe", ""))
                End If

                Dim msg As String = ex.Message & vbNewL2
                If nProcess.Count > 0 Then
                    msg &= "Process opened: " & vbNewLine
                    For Each proc As Process In nProcess
                        msg &= "- " & proc.Id & ": " & proc.ProcessName & vbNewLine
                    Next

                    msg &= vbNewLine & "Would you like the setup to end the process automatically?"
                Else
                    msg &= "Try again?"
                End If
                If MsgQuestion(msg, "Error") = MessageBoxResult.Yes Then
                    If nProcess.Count = 0 Then
                        Return FnExtractFile(entry, ToPath)
                    Else
                        For Each proc As Process In nProcess
                            Try
                                proc.Kill()
                            Catch ex2 As Exception
                            End Try
                        Next
                        'DoEvents()
                        System.Threading.Thread.Sleep(100)
                        Return FnExtractFile(entry, ToPath)
                    End If
                End If
            End Try
            Return False
        End Function

    End Class

End Namespace

