Imports System.IO
Imports System.Reflection
Imports System.Windows

Public Class MainWindow
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        acLoadIcon()
    End Sub

    Private Sub acLoadIcon()
        Try
            Dim str = Assembly.GetEntryAssembly.GetManifestResourceStream("Icon.ico")
            If str IsNot Nothing Then
                Using memStr As New MemoryStream
                    str.CopyTo(memStr)
                    Dim b As Byte() = memStr.ToArray
                    b = UnZip(b)
                    Using unZipStream As New MemoryStream(b)
                        Me.Icon = New System.Drawing.Icon(unZipStream).ToImageSource
                    End Using
                End Using
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Window_MouseLeftButtonDown(sender As Object, e As Windows.Input.MouseButtonEventArgs)
        If e.LeftButton = Windows.Input.MouseButtonState.Pressed Then
            Me.DragMove()
        End If
    End Sub

    Private Sub acMinimize(sender As Object, e As Windows.RoutedEventArgs)
        Me.WindowState = Windows.WindowState.Minimized
    End Sub

    Private Sub Window_ContentRendered(sender As Object, e As EventArgs)
        AppMainModelView.acInit
    End Sub


End Class
