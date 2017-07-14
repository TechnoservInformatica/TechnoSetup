Imports TechnoSetupLib
Module Module1

    Sub Main()

        Dim InstallerConfig As New TechnoSetupLib.SetupConfigModel
        With InstallerConfig
            .AppName = "TESTE APP"
            .OutPutFile = "\OutPutAssembly\TechnoSetupLibTest.exe"
            .Files.Add(New SetupModelSubClasses.TechnoFilesConfig)
            .AppGuid = New Guid("6255c397-d1d6-4a30-bb30-e179f27cb710")
            With .Files(0)
                .Dir.Add(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\TechnoSetup\Test")
                .Dir.Add(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\TechnoSetup\Test2")
                .Files.Add(New SetupModelSubClasses.TechnoFile With {.Name = ".\\TesteFiles\*.txt", .GACRegister = True, .TopOnly = True})
            End With
            ' .Tags.Add(New SetupModelSubClasses.TagsClass With {.TagName = "App", .Value = "Technoserv"})
            '.Tags.Add(New SetupModelSubClasses.TagsClass With {.TagName = "App2", .Value = "Technoserv2"})

        End With

        ConsoleMode = True
        '   DebugGeneratedEXE = True
        AutoExit = True

        InstallerConfig.Compile
    End Sub

End Module
