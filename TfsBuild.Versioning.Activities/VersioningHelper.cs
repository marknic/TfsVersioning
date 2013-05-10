using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
    public static class VersioningHelper
    {
        public const string CsAssembly = "[assembly: {0}(\"{1}\")]";
        public const string CppAssembly = "[assembly: {0}Attribute(\"{1}\")];";
        public const string VbAssembly = "<Assembly: {0}(\"{1}\")>";
        public const string FsAssembly = "[<assembly: {0}(\"{1}\")>]";
        public const string FsDoRegex = @".*(\(\s*\)|do\s*\(\s*\))";
        public const string FsDoStatement = "do ()";
        public const string PropertyNotFound = "Property Not Found";
        public const string AssemblyVersionPropertyName = "AssemblyVersion";
        public const string AssemblyFileVersionPropertyName = "AssemblyFileVersion";

        public static readonly string[] AlternateAssemblyProperties;

        public const string RegExPropertyMatch =
            @"(?<AssemblyPropertyLine>(?<PropertyPrefix>[\[<].*{0}.*\(\x22)(?<PropertyValue>.*)(?<PropertyPostfix>\x22\)[\]>]+))";

        static VersioningHelper()
        {
            AlternateAssemblyProperties = new[] { "AssemblyTitle", "AssemblyDescription", "AssemblyConfiguration", 
                "AssemblyCompany", "AssemblyProduct", "AssemblyCopyright", "AssemblyTrademark", 
                "AssemblyCulture", "AssemblyInformationalVersion" };
        }

        public static ProjectTypes GetProjectTypeFromFileName(string filename)
        {
            if (String.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException("filename");
            }

            var fileExtension = Path.GetExtension(filename);

            if (String.IsNullOrWhiteSpace(fileExtension))
            {
                // nothing there? just pass in something that definitely won't match
                fileExtension = ".xxx";
            }

            var normalizedFileExtension = fileExtension.ToLower();

            switch (normalizedFileExtension)
            {
                case ".cs":
                    return ProjectTypes.Cs;

                case ".vb":
                    return ProjectTypes.Vb;

                case ".cpp":
                    return ProjectTypes.Cpp;

                case ".fs":
                    return ProjectTypes.Fs;

                default:
                    throw new ArgumentException(String.Format("{0} is not one of the accepted file types (.cs, .vb, .cpp, .fs)", filename));
            }
        }

        public static string GetVersionShellByType(ProjectTypes projectType)
        {
            switch (projectType)
            {
                case ProjectTypes.Cs:
                    return CsAssembly;

                case ProjectTypes.Vb:
                    return VbAssembly;

                case ProjectTypes.Cpp:
                    return CppAssembly;

                case ProjectTypes.Fs:
                    return FsAssembly;
            }

            throw new ArgumentException("projectType");
        }

        public static string InsertAssemblyInfoProperty(string fileData, string assemblyInfoPropertyName, string assemblyInfoPropertyValue, ProjectTypes projectType)
        {
            var fileDataOut = new StringBuilder();

            var versionShell = GetVersionShellByType(projectType);

            // if working with F# files, remove the "do binding" so we can make sure that the "do" is at the end of the file)
            //if (projectType == ProjectTypes.Fs)
            //{
            //    var regex = new Regex(FsDoRegex);
            //    fileData = regex.Replace(fileData, "");
            //}

            fileDataOut.Append(fileData);

            var versionLine = String.Format(versionShell, assemblyInfoPropertyName, assemblyInfoPropertyValue);

            fileDataOut.AppendLine();
            fileDataOut.Append(versionLine);
            fileDataOut.AppendLine();

            //// for F#, put the do() binding back in
            //if (projectType == ProjectTypes.Fs)
            //{
            //    fileDataOut.Append(FsDoStatement);
            //    fileDataOut.AppendLine();
            //}

            return fileDataOut.ToString();
        }

        public static IList<KeyValuePair<string, string>> GetArgumentValues(CodeActivityContext context, string argumentStartsWith)
        {
            PropertyDescriptorCollection properties = context.DataContext.GetProperties();
            IList<KeyValuePair<string, string>> arguments = new ReadOnlyCollectionBuilder<KeyValuePair<string, string>>();

            foreach (PropertyDescriptor property in properties)
            {
                // Get the name of the property/argument
                var name = property.DisplayName; 

                // must be a string
                if ((property.PropertyType != typeof (string))) continue;

                // if an argumentStartsWith has a value and name starts with it
                if ((!string.IsNullOrWhiteSpace(argumentStartsWith)) && (!name.StartsWith(argumentStartsWith)))
                    continue;

                var value = (string) property.GetValue(context.DataContext);

                // the property/argument must have a value
                if (string.IsNullOrWhiteSpace(value)) continue;

                arguments.Add(new KeyValuePair<string, string>(name, value));
            }

            return arguments;
        }

        public static IList<KeyValuePair<string, string>> GetArgumentValues(IList<KeyValuePair<string, string>> inputValues)
        {
            IList<KeyValuePair<string, string>> arguments = new ReadOnlyCollectionBuilder<KeyValuePair<string, string>>();

            foreach (KeyValuePair<string, string> argumentValuePair in inputValues)
            {
                if ((!string.IsNullOrWhiteSpace(argumentValuePair.Value)) && (argumentValuePair.Value != PropertyNotFound))
                {
                    arguments.Add(new KeyValuePair<string, string>(argumentValuePair.Key, argumentValuePair.Value));    
                }
            }

            return arguments;
        }

        /// <summary>
        /// This method will walk through all the patterns in the property value and replace them with actual values
        /// </summary>
        /// <param name="propertyValue"></param>
        /// <param name="buildDetail"></param>
        /// <param name="buildNumberPrefix"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ReplacePatternsInPropertyValue(string propertyValue, IBuildDetail buildDetail, int buildNumberPrefix, DateTime date)
        {
            const string regExFindTokenPattern = @"\$(?<token>\w+)";
            const string regExReplaceTokenPattern = @"\${0}";
            var modifiedPropertyValue = propertyValue;

            var regex = new Regex(regExFindTokenPattern);
            var matches = regex.Matches(propertyValue);

            foreach (Match match in matches)
            {
                string token = match.Value.Remove(0, 1);

                string convertedValue = ReplacePatternWithValue(token, buildDetail, buildDetail.BuildNumber, buildNumberPrefix, date);

                var regExReplace = new Regex(string.Format(regExReplaceTokenPattern, token));

                modifiedPropertyValue = regExReplace.Replace(modifiedPropertyValue, convertedValue);
            }

            modifiedPropertyValue = modifiedPropertyValue.Replace('\\', ':');

            return modifiedPropertyValue;
        }

        public static string ReplacePatternWithValue(string pattern, string buildNumber, int buildNumberPrefix, DateTime date)
        {
            return ReplacePatternWithValue(pattern, null, buildNumber, buildNumberPrefix, date);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="buildDetail"></param>
        /// <param name="buildNumber">The full build number - left in for versioning (backward compatibility) reasons</param>
        /// <param name="buildNumberPrefix"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ReplacePatternWithValue(string pattern, IBuildDetail buildDetail, string buildNumber, int buildNumberPrefix, DateTime date)
        {
            var patternUpper = pattern.ToUpper();
            string convertedValue;

            string internalBuildNumber = buildDetail == null ? buildNumber : buildDetail.BuildNumber;

            switch (patternUpper)
            {
                case "TPROJ":
                    if (buildDetail == null) throw new ArgumentNullException("buildDetail");
                    convertedValue = buildDetail.TeamProject;
                    break;

                case "REQBY":
                    if (buildDetail == null) throw new ArgumentNullException("buildDetail");
                    convertedValue = StripDomain(buildDetail.RequestedBy);
                    break;

                case "BNAME":
                    if (buildDetail == null) throw new ArgumentNullException("buildDetail");
                    convertedValue = buildDetail.BuildDefinition.Name;
                    break;

                case "UTIME":
                    convertedValue = date.ToUniversalTime().ToString();
                    break;

                case "LDATE":
                    convertedValue = date.ToLongDateString();
                    break;

                case "LTIME":
                    convertedValue = date.ToLongTimeString();
                    break;

                case "SDATE":
                    convertedValue = date.ToShortDateString();
                    break;

                case "STIME":
                    convertedValue = date.ToShortTimeString();
                    break;

                case "BNUM":
                    if (buildDetail == null) throw new ArgumentNullException("buildDetail");
                    convertedValue = buildDetail.BuildNumber;
                    break;

                case "YYYY":
                    convertedValue = date.ToString("yyyy");
                    break;

                case "YY":
                    convertedValue = date.ToString("yy");
                    break;

                case "M":
                case "MM":
                    convertedValue = date.Month.ToString();
                    break;

                case "D":
                case "DD":
                    convertedValue = date.Day.ToString();
                    break;

                case "J":
                    convertedValue = string.Format("{0}{1}", date.ToString("yy"), string.Format("{0:000}", date.DayOfYear));
                    break;

                case "B":
                    if (string.IsNullOrEmpty(buildNumber))
                    {
                        throw new ArgumentException("BuildNumber must contain the build value: use $(Rev:.r) at the end of the Build Number Format");
                    }

                    int buildNumberValue;

                    // Attempt to parse - this should probably fail since it will only work if the only thing passed 
                    //  in through the BuildNumber is a number.  This is typically something like: "Buildname.year.month.buildNumber"
                    var isNumber = int.TryParse(internalBuildNumber, out buildNumberValue);

                    if (!isNumber)
                    {
                        var buildNumberArray = internalBuildNumber.Split('.');

                        const string exceptionString = "'Build Number Format' in the build definition must end with $(Rev:.r) to use the build number in the version pattern.  Suggested pattern: $(BuildDefinitionName)_$(Year:yyyy).$(Month).$(DayOfMonth)$(Rev:.r)";

                        if (buildNumberArray.Length < 2)
                        {
                            throw new ArgumentException(exceptionString);
                        }

                        isNumber = int.TryParse(buildNumberArray[buildNumberArray.Length - 1], out buildNumberValue);

                        if (isNumber == false)
                        {
                            throw new ArgumentException(exceptionString);
                        }
                    }

                    buildNumberValue = AddBuildNumberPrefixIfNecessary(buildNumberPrefix, buildNumberValue);
                    convertedValue = buildNumberValue.ToString();
                    break;

                default:
                    convertedValue = pattern;
                    break;
            }

            return convertedValue;
        }

        public static string StripDomain(string userId)
        {
            var convertedValue = userId;

            if (userId.Contains("\\"))
            {
                var splitVals = userId.Split('\\');

                convertedValue = splitVals[1];
            }

            return convertedValue;
        }

        public static int AddBuildNumberPrefixIfNecessary(int buildNumberPrefix, int buildNumberValue)
        {
            // If a BuildNumberPrefix is in place and the BuildNumber pattern is used then 
            // attempt to prefix the build number with the BuildNumberPrefix
            // The value of 10 is used since the prefix would have to be at least 10 to be at all useable
            if (buildNumberPrefix > 0)
            {
                if ((buildNumberValue >= buildNumberPrefix) || (buildNumberPrefix < 10))
                {
                    throw new ArgumentException("When the BuildNumberPrefix is used it must be at least 10 and also larger than the Build Number.");
                }

                // Prefix the build number to set it apart from any other build definition
                buildNumberValue += buildNumberPrefix;
            }

            return buildNumberValue;
        }
    }
}
