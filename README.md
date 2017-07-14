# TechnoSetup
Setup Builder that outputs a WPF application that will install and uninstall aplications.

# Current progress:
- Can generate an installer that copies files to directories.
- Generated installer can run as administrator.
- Installer can only be generated from console application with the class.
- Output Installer assembly attributes configurable. (like AssemblyVersion, AssemblyCopyright, AssemblyTitle, AssemblyProduct, GuidAttribute, etc...)

# Future:
  - Setup will be:
    - Generated from an xml configuration.
    - Able to register .NET assemblies on the GAC.
    - Able to register .ocx activex dll's.
    - Able to Download files.
    - Able to execute files before installing and after installing.

- Setup will have a Customizable Window.
- Maybe build an IDE that generated and edits the xml configration file.


