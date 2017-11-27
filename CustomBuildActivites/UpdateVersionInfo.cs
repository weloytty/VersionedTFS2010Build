using System;
using System.IO;
using System.Activities;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Build.Client;

namespace CustomBuildActivities
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class UpdateVersionInfo : CodeActivity
    {
        [RequiredArgument]
        public InArgument<string> AssemblyInfoFileMask { get; set; }

        [RequiredArgument]
        public InArgument<string> SourcesDirectory { get; set; }

        [RequiredArgument]
        public InArgument<string> VersionFilePath { get; set; }

        [RequiredArgument]
        public InArgument<Version> NewVersion { get; set; }

        [RequiredArgument]
        public InArgument<string> VersionFileName { get; set; }

        public InArgument<bool> IsPreReleaseBuild { get; set; }

        public InArgument<string> PreReleaseTag { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var sourcesDirectory = context.GetValue(SourcesDirectory);
            var assemblyInfoFileMask = context.GetValue(AssemblyInfoFileMask);
            var versionFile = context.GetValue(VersionFilePath) + @"\" + context.GetValue(VersionFileName);
            string preReleaseTag;
            bool isPreReleaseBuild;
            try
            {
                isPreReleaseBuild = context.GetValue(IsPreReleaseBuild);
                preReleaseTag = context.GetValue(PreReleaseTag);
            }
            catch (Exception)
            {
                // This is an older build template that does not support pre-release
                isPreReleaseBuild = false;
                preReleaseTag = "";
            }

            //Load the version info into memory
            //var versionText = "1.0.0.0";
            //if (File.Exists(versionFile))
            //    versionText = File.ReadAllText(versionFile);
            //var currentVersion = new Version(versionText);
            //var newVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build + 1, currentVersion.Revision);
            var newVersion = context.GetValue(NewVersion);

            File.WriteAllText(versionFile, newVersion.ToString());

            bool changedContents;
            foreach (var file in Directory.EnumerateFiles(sourcesDirectory, assemblyInfoFileMask, SearchOption.AllDirectories))
            {
                var text = File.ReadAllText(file);
                changedContents = false;
                // we want to find 'AssemblyVersion("1.0.0.0")' etc
                foreach (var attribute in new[] { "AssemblyVersion", "AssemblyFileVersion", "AssemblyInformationalVersion" })
                {
                    string newVersionToWrite;

                    // If this is the NuGet field we need to see if this is a pre release build.
                    if ((attribute == "AssemblyInformationalVersion") && (isPreReleaseBuild))
                    {
                        // This is a pre-release build.  Add the prerelease tag (custom or default).
                        if (string.IsNullOrWhiteSpace(preReleaseTag))
                            newVersionToWrite = newVersion + "-PreRelease";
                        else
                            newVersionToWrite = newVersion + "-" + preReleaseTag;
                    }
                    else
                    {
                        newVersionToWrite = newVersion.ToString();
                    }

                    var regex = new Regex(attribute + @"\(""\d+\.\d+\.\d+\.\d+""\)");
                    var match = regex.Match(text);
                    if (!match.Success) continue;
                    text = regex.Replace(text, attribute + "(\"" + newVersionToWrite + "\")");

                    changedContents = true;
                }
                if (changedContents)
                    File.WriteAllText(file, text);
            }
        }
    }
}
