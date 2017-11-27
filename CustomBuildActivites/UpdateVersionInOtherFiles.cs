using System;
using System.Activities;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Build.Client;

namespace CustomBuildActivities
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class UpdateVersionInOtherFiles : CodeActivity
    {
        [RequiredArgument]
        public InArgument<Version> NewVersion { get; set; }

        [RequiredArgument]
        public InArgument<string> SourcesDirectory { get; set; }

        [RequiredArgument]
        public InArgument<bool> VersionUpdateOtherFileTypes { get; set; }

        [RequiredArgument]
        public InArgument<string> VersionUpdateOtherFileMasks { get; set; }

        [RequiredArgument]
        public InArgument<string[]> VersionUpdateOtherFileRegexReplacements { get; set; }


        protected override void Execute(CodeActivityContext context)
        {
            if (!context.GetValue(VersionUpdateOtherFileTypes)) return;
            var sourcesDirectory = context.GetValue(SourcesDirectory);
            var versionUpdateOtherFileMasks = context.GetValue(VersionUpdateOtherFileMasks);
            var versionUpdateOtherFileRegexReplacements = context.GetValue(VersionUpdateOtherFileRegexReplacements);
            var newVersion = context.GetValue(NewVersion);


            // foreach file mask e.g. "app.config, *.wxs; web.config"
            foreach (string versionUpdateOtherFileMask in
                versionUpdateOtherFileMasks.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()))
            {
                // for each file found maching this mask.
                foreach (var file in
                    Directory.EnumerateFiles(sourcesDirectory, versionUpdateOtherFileMask, SearchOption.AllDirectories))
                {
                    var text = File.ReadAllText(file);
                    bool changedContents = false;

                    // foreach search pattern and replacement string specified e.g. {"Version *= *""-?1.0.0.0"";Version = ""$version""", "version *= *""-?1.0.0.0"";version = ""$version"""}
                    foreach (string attribute in versionUpdateOtherFileRegexReplacements)
                    {
                        var x = attribute.Split(new char[1] { ';' }, 2);
                        var searchPattern = x[0];
                        var replacement = x[1].Replace("$version", newVersion.ToString());
                        var regex = new Regex(searchPattern);
                        var match = regex.Match(text);
                        if (!match.Success)
                        {
                            continue;
                        }

                        text = regex.Replace(text, replacement);
                        changedContents = true;
                    }
                    if (changedContents)
                        File.WriteAllText(file, text);
                }
            }
        }
    }
}