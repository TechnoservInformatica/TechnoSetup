Imports System.IO
Imports System.Reflection
Imports System.Windows

Public Class SetupApp
    Inherits System.Windows.Application

    Sub New()

    End Sub

    Private Sub AppStarted(sender As Object, e As StartupEventArgs) Handles Me.Startup
        Try

            AppMainModelView = New ModelView.MainModelView
            AppMainModelView.ApplicationLog = "Reading config file" & vbNewLine

            Dim nAsm = Assembly.GetEntryAssembly
            If DebugGeneratedEXE AndAlso GeneratedAsm IsNot Nothing Then
                nAsm = GeneratedAsm
            End If

            Using str As IO.Stream = nAsm.GetManifestResourceStream("Config.bin")
                Dim b As Byte() = New Byte(str.Length) {}
                str.Read(b, 0, str.Length)
                b = UnZip(b)
                'Will be an Install
                If mShared.IsInstalling Then
                    InstallConfig = XML.NewDeSerialize(Of SetupInstallModel)(b)
                Else
                    'Will be an Uninstall

                End If
            End Using

            MyMainWindow = New MainWindow With {.DataContext = AppMainModelView}
            Application.Current.MainWindow = MyMainWindow
            MyMainWindow.ShowDialog()

        Catch ex As Exception
            Dim msg As String = ex.Message & vbNewLine & (ex.InnerException?.Message & vbNewLine) & ex.StackTrace
            File.WriteAllText(IO.Path.GetDirectoryName(Assembly.GetEntryAssembly.Location) & "\log.txt", msg)
            MsgBox(msg, vbCritical Or vbApplicationModal, "Error")
        Finally
            Shutdown()
        End Try
    End Sub

End Class
