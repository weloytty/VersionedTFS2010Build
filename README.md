# VersionedTFS2010Build
Grabbed from codeplex because I need to use it and I don't want it to disappear

https://versionedtfsbuild.codeplex.com/


**_Original Readme From Codeplex:_**

# Project Description
Versioned Build for TFS 2010. Allows for 4 version numbers (Called Major, Minor, Emergency and Build), Will also update the build number into any AssemblyInfo files in the project before building (effectively giving your binaries the same version number).
# Setup
Note: These instructions assume you have a basic build and build controller already created.
If you do not there are lots of tutorials on the web on how to set these up.

## File Setup

        Download the latest release
        Check the dlls into TFS 2010 in a common location for all your projects
            For example $/TFSCommon/CustomBuildActions
        Check the VersionedBuildProcess.xaml into $/YourProjectToBuild/BuildProcessTemplates
        Commit these pending changes to TFS 2010 (so they are really on the server).

## Build Controller Setup

        In Team Explorer right click on the Build node of your project and select Manage Build Controllers.
        In the resulting dialog select your controller and then select properties
        In the resulting dialog find the "Version control path to custom assemblies:" field.
            Enter the location that you put the dlls (ie $/TFSCommon/CustomBuildActions)

## Setup the build

        In Team Explorer right click on the Build node of your project and select Refresh.
        Right click on your build (under the Build node of your project).
        Select Edit Build Definition
        Select Process from the list on the right.
        Select Show details ShowDetails.PNG
        Select New
        In the resulting dialog, select the Select an existing XAML file radio button
        Enter $/YourProjectToBuild/BuildProcessTemplates/VersionedBuildProcess.xaml into the Version control path:
        Select OK
        In the dropdown below Build process file (Windows Workflow XAML): make sure that VersionedBuildProcess.xaml is selected.
        In the grid below Build process parameters find the VersionFileLocation parameter
            Enter $/YourProjectToBuild/BuildProcessTemplates for the VersionFileLocation
        Create and check to TFS 2010 a file called Version.txt
            File file MUST have only one line with four version numbers separated by a period (ie. 1.0.0.0)
            Check the file into $/YourProjectToBuild/BuildProcessTemplates
        Save and close the Build Definition changes


    That should be it. Run your build and you should get build numbers for your TFS Build. Also check the binaries output by your build. They should have the same version number as the build does. 

## Usage
This covers the other options found in the Build process parameters grid. (See above for how to get to this grid.)

    ##Increasing Version Numbers

        IsMajorBuild: Will increase the first number in the version when the next build happens. (Example 1.0.0.0 would become 2.0.0.0)
            This is can (and should) be set when the build is queued by selecting the Parameters tab in the Queue Build dialog box.
        IsMinorBuild: Will increase the second number in the version when the next build happens. (Example 1.0.0.0 would become 1.1.0.0)
            This is can (and should) be set when the build is queued by selecting the Parameters tab in the Queue Build dialog box.
        IsEmergencyBuild: Will increase the third number in the version when the next build happens. (Example 1.0.0.0 would become 1.0.1.0)
            This is can (and should) be set when the build is queued by selecting the Parameters tab in the Queue Build dialog box.
        AssemblyInfoMask:This is used to set the mask to define what files will have the search and replace done for the new version number.
            What ever files match this mask are looked in for an expression matching {new Regex(attribute + @"\(""\d+\.\d+\.\d+\.\d+""\)")} (for example this would match AssemblyFileVersion("1.0.0.0"))
            Matching expressions have the version number in them replaced with the new build number
        Version File: The name of the file that will hold the version number. Defaults to Version.txt (that is what is used in the instructions above.
            This could be used to allow more than one version number per project.
        VersionFileLocation: This is where the version file is located.
            This could be used to allow more than one version number per project.
        IsPreRelease: If true then this build is a pre-release build (usually only matters when building NuGet packages).
        PreReleaseTag: Tag to add to the pre-release. (NuGut supports any value to indicate that a package is prerelease.

To build a NuGet Package using this template add this to your NuGet project's post build activity and replace the ""\\SomeComputer\developer\NuGet Repository"" with the location of your NuGetRepository (or where ever you want your packages to drop to)

NOTE: This script assumes you have "NuGet Package Restore" turned on. If you do not then you will have to update the nuget location to point to nuget.exe.

NOTE: Enter it exactly as shown. (Extra newlines will mess it up.)

