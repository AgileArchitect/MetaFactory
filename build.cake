var target = Argument("target", "Default");

//#addin nuget:?package=Cake.NSwag
#addin nuget:?package=Cake.AutoRest
#addin nuget:?package=Cake.Git
//#tool nuget:?package=NSwag.MSBuild


 var currentBranch = GitBranchCurrent("./");
string suffix = "";
if( BuildSystem.IsLocalBuild ) {
    suffix = "-pre999";
}
else if( currentBranch.FriendlyName != "master" ) {
    if( BuildSystem.IsRunningOnAppVeyor ) {
        suffix = "-build" + BuildSystem.AppVeyor.Environment.Build.Number;
    }
    else {
        suffix = "-rc";
    }
}

Task("Clean")
    .Does(() => {
        CreateDirectory("./artifacts");
        CleanDirectory("./artifacts");
    });

Task("Restore")
    .Does(() => {
        DotNetCoreRestore("./src");
    });

Task("Build")
    .IsDependentOn("Restore")
    .IsDependentOn("Clean")
    .Does(() => {

        CreateDirectory("./artifacts");
        CleanDirectory("./artifacts");

        var settings = new DotNetCoreBuildSettings
        {
            Framework = "netcoreapp1.1",
            Configuration = "Release",
            OutputDirectory = "./artifacts/" + "MetaFactory"
        };

        DotNetCoreBuild("./src/MetaFactory", settings);
    });

Task("Test")
    .Does(() => {
        DotNetCoreBuild("./src/MetaFactory.Test");
        DotNetCoreTest("./src/MetaFactory.Test");
    });

Task("Generate-Client")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .Does(() => {


        var path = MakeAbsolute(File("./artifacts/swagger.json"));

        Information("Trying to generate " + path);

        StartProcess("dotnet", new ProcessSettings() {
            Arguments = "run generate http://localhost:5000/swagger/v1/swagger.json " + path,
            WorkingDirectory = "./src/MetaFactory"
        });

        var settings = new AutoRestSettings {
            Namespace = "MetaFactory.Client",
            Generator = CodeGenerator.CSharp,
            ClientName = "MetaFactoryClient",
            HeaderComment = "Generated by Cake.AutoRest",
            OutputDirectory = "./src/Metafactory.Client"
        };

        AutoRest.Generate(path, settings);

    });

Task("Build-Client")
    .IsDependentOn("Generate-Client")
    .Does(() => {

        var settings = new DotNetCoreBuildSettings
        {
            Framework = ".NETStandard,Version=v1.2",
            Configuration = "Release",
            OutputDirectory = "./artifacts/" + "MetaFactory.Client"
        };

        DotNetCoreBuild("./src/MetaFactory.Client", settings);
    });

Task("Build-Cake-Addin")
    .IsDependentOn("Build-Client")
    .Does(() => {

        var settings = new DotNetCoreBuildSettings
        {
            Framework = ".NETStandard,Version=v1.6",
            Configuration = "Release",
            OutputDirectory = "./artifacts/" + "Cake.MetaFactory"
        };

        DotNetCoreBuild("./src/Cake.MetaFactory", settings);
    });

Task("Package")
    .IsDependentOn("Build-Cake-Addin")
    .IsDependentOn("Package-Clean")
    .IsDependentOn("Package-Cake-Addin")
    .Does(() => {

        var settings = new DotNetCorePackSettings {
            Configuration = "Release",
            OutputDirectory = "./nuget",
            VersionSuffix = suffix
        };
        
        var toPack = new string[] { "MetaFactory", "MetaFactory.Client" };

        foreach( var p in toPack )
            DotNetCorePack("./src/" + p, settings);
    });

Task("Package-Cake-Addin")
    .Does(() => {
             var nuGetPackSettings   = new NuGetPackSettings {
                                     Id                      = "Cake.MetaFactory",
                                     Version                 = "1.0.0" + suffix,
                                     Title                   = "Cake Helper for the MetaFactory",
                                     Authors                 = new[] {"John Doe"},
                                     Owners                  = new[] {"Contoso"},
                                     Description             = "The description of the package",
                                     Summary                 = "Excellent summary of what the package does",
                                     ProjectUrl              = new Uri("https://github.com/AgileArchitect/MetaFactory"),
                                     Copyright               = "AgileArchitect 2017",
                                     Tags                    = new [] {"Cake", "MetaFactory"},
                                     RequireLicenseAcceptance= false,
                                     Symbols                 = false,
                                     NoPackageAnalysis       = true,
                                     Files                   = new [] {
                                                                          new NuSpecContent {Source = "*.dll", Target = "lib/net45"},
                                                                       },
                                     BasePath                = "./src/Cake.MetaFactory/bin/release/net45",
                                     OutputDirectory         = "./nuget"
                                 };

     NuGetPack(nuGetPackSettings);
    });


Task("Package-Clean")
    .Does(() => {
        CreateDirectory("./nuget");
        CleanDirectory("./nuget");
    });


Task("AppVeyor")
    .IsDependentOn("Package");

Task("Default")
    .IsDependentOn("Build-Client");

RunTarget(target);
