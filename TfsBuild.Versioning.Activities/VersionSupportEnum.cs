// ==============================================================================================
// http://tfsversioning.codeplex.com/
//
// Author: Mark S. Nichols
//
// Copyright (c) 2011 Microsoft Corporation
//
// This source is subject to the Microsoft Permissive License. 
// ==============================================================================================

namespace TfsBuild.Versioning.Activities
{
    /// <summary>
    /// The version number to be changed
    /// </summary>
    public enum VersionTypeOptions
    {
        AssemblyVersion,
        AssemblyFileVersion
    }

    public enum ProjectTypes
    {
        Cs,
        Vb,
        Cpp,
        Fs
    }
}

