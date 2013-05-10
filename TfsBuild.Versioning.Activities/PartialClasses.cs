using System.Drawing;
using Microsoft.TeamFoundation.Build.Client;

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
    /// Adds the necessary annotations to the UpdateAssemblyInfoFileVersion XAML object
    /// </summary>    
    [ToolboxBitmap(typeof(UpdateAssemblyInfoFileVersion), "Resources.version.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed partial class UpdateAssemblyInfoFileVersion
    {
    }

    /// <summary>
    /// Adds the necessary annotations to the VersionAssemblyInfoFiles XAML object
    /// </summary>    
    [ToolboxBitmap(typeof(VersionAssemblyInfoFiles), "Resources.version.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed partial class VersionAssemblyInfoFiles
    {
    }
}
