TfsVersioning Version 2.0 Released (TFS 2012)
=============
 Versioning is part of Visual Studio projects within the AssemblyInfo file. It’s easy to update the version once but how many people actually and effectively manage an application’s version through this mechanism?  I don’t actually know but I would bet that most only do it sporadically and eventually give up. Manual editing of the AssemblyVersion and AssemblyFileVersion is much harder that it needs to be.

The goal of this project is to create a way to modify the automated build process of TFS so that versioning is automatic while giving the user the flexibility that they need given the project’s process requirements. 

This solution provides versioning using patterns against the Assembly Version and Assembly File Version and includes:
Automated incrementing of the version values 
Centralized version management of any number of solutions 
Mechanism to tie an assembly back to the specific build definition as well as the source code that built it 
Automated check-in of the “assemblyInfo” files 
Works on C#, VB and C++ projects 
Operates as an addition to the TFS 2012 build workflow (WF 4.5 XAML) 

## # This is the source code only. # ##

Download the assembly package from CodePlex: [http://tfsversioning.codeplex.com](http://tfsversioning.codeplex.com)
