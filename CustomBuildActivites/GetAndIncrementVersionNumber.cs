using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace CustomBuildActivities
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class GetAndIncrementVersionNumber : CodeActivity<String>
    {
        [RequiredArgument]
        public InArgument<string> BuildNumberFormat { get; set; }

        [RequiredArgument]
        public InOutArgument<Version> NewVersion { get; set; }

        [RequiredArgument]
        public InArgument<Boolean> IsMajorBuild { get; set; }

        [RequiredArgument]
        public InArgument<Boolean> IsMinorBuild { get; set; }

        [RequiredArgument]
        public InArgument<Boolean> IsEmergencyBuild { get; set; }

        [RequiredArgument]
        public InArgument<Workspace> CurrentWorkspace { get; set; }

        [RequiredArgument]
        public InArgument<String> VersionFileLocation { get; set; }

        [RequiredArgument]
        public InArgument<String> VersionFileName { get; set; }

        protected override String Execute(CodeActivityContext context)
        {
            // Pull ou the arguments in to locals
            var buildNumberFormat = context.GetValue(BuildNumberFormat);
            Workspace currentWorkspace = context.GetValue(CurrentWorkspace);
            string versionFileLocation = context.GetValue(VersionFileLocation);
            if (versionFileLocation.LastIndexOf("/") != versionFileLocation.Length - 1)
                versionFileLocation = versionFileLocation + "/";
            string versionFileName = context.GetValue(VersionFileName);

            // Get the version from source
            string localVersionPath;
            Version oldVersion = GetVersionFromTFS(currentWorkspace, versionFileLocation, versionFileName, out localVersionPath);

            // Checkout the version file
            currentWorkspace.PendEdit(localVersionPath);
            
            // Increase the version number
            Version newVersion = IncrementVersion(context, oldVersion);

            // Save off the version number for later processes
            context.SetValue(NewVersion, newVersion);

            // Update and check back in the version file.
            UpdateVersionBackToTFS(currentWorkspace, versionFileLocation, versionFileName, newVersion, localVersionPath);
            
            // return the version in the name of the build.
            return buildNumberFormat.Replace("$(Version)", newVersion.ToString());
        }

        private Version IncrementVersion(CodeActivityContext context, Version oldVersion)
        {
            bool isMajor = context.GetValue(IsMajorBuild);
            bool isMinor = context.GetValue(IsMinorBuild);
            bool isEmergency = context.GetValue(IsEmergencyBuild);
            int major = oldVersion.Major;
            int minor = oldVersion.Minor;
            int emergency = oldVersion.Build;
            if (isMajor)
            {
                major++;
                minor = 0;
                emergency = 0;
            }

            if (isMinor)
            {
                minor++;
                emergency = 0;
            }
            
            if (isEmergency) emergency++;

            return new Version(major, minor, emergency, oldVersion.Revision + 1);
        }

        private Version GetVersionFromTFS(Workspace currentWorkspace, string versionFileLocation, string versionFileName, out string localVersionPath)
        {
            // Make sure we have a map to the version file
            if (!currentWorkspace.IsServerPathMapped(versionFileLocation))
            {
                // Map the version file to somewhere.
                currentWorkspace.Map(versionFileLocation, @"C:\temp\BuildVersions" + Guid.NewGuid());
            }

            // Make sure we have the latest from source control.
            GetRequest getRequest = new GetRequest(new ItemSpec(versionFileLocation + versionFileName,RecursionType.None), VersionSpec.Latest);
            currentWorkspace.Get(getRequest, GetOptions.Overwrite);           

            localVersionPath = currentWorkspace.GetLocalItemForServerItem(versionFileLocation + versionFileName);

            string oldVersion = "1.0.0.0";
            if (File.Exists(localVersionPath))              
                oldVersion = File.ReadAllText(localVersionPath);

            return new Version(oldVersion);
        }

        private void UpdateVersionBackToTFS(Workspace currentWorkspace, string versionFileLocation, string versionFileName, Version newVersion, String localVersionPath)
        {
            File.WriteAllText(localVersionPath, newVersion.ToString()); 
            
            WorkspaceCheckInParameters parameters = new WorkspaceCheckInParameters(new[] {new ItemSpec(versionFileLocation + versionFileName, RecursionType.None)}, "***NO_CI*** - Updating Version" );

            currentWorkspace.CheckIn(parameters);

        }
    }
}
