using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Build.Client;

namespace CustomBuildActivities
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class UpdateBuildVersionLabel
    {
        [RequiredArgument]
        public InArgument<string> BuildNumberFormat { get; set; }

        [RequiredArgument]
        public InArgument<Version> NewVersion { get; set; }

        protected override string Execute(CodeActivityContext context)
        {
            var buildNumberFormat = context.GetValue(BuildNumberFormat);
            Version currentVersion = new Version(1, 2, 5, 61);
            Version newVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build, currentVersion.Revision + 1);

            buildNumberFormat.Replace("$(Version)", newVersion.ToString());


            return newVersion.ToString();
        }
    }
}
