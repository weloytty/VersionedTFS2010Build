using System;
using System.IO;
using System.Activities;
using System.Linq;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace CustomBuildActivities
{

    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class SetReadOnlyFlag : CodeActivity
    {
        [RequiredArgument]
        public InArgument<string> FileMask { get; set; }

        [RequiredArgument]
        public InArgument<bool> ReadOnlyFlagValue { get; set; }

        [RequiredArgument]
        public InArgument<Workspace> Workspace { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var fileMasks = context.GetValue(FileMask);
            var workspace = context.GetValue(Workspace);
            var readOnlyFlagValue = context.GetValue(ReadOnlyFlagValue);

            foreach (var folder in workspace.Folders)
            {
                foreach (string fileMask in
                    fileMasks.Split(new char[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()))
                {
                    foreach (var file in Directory.GetFiles(folder.LocalItem, fileMask, SearchOption.AllDirectories))
                    {
                        var attributes = File.GetAttributes(file);
                        if (readOnlyFlagValue)
                            File.SetAttributes(file, attributes | FileAttributes.ReadOnly);
                        else
                            File.SetAttributes(file, attributes & ~FileAttributes.ReadOnly);
                    }
                }
            }
        }
    }
}

