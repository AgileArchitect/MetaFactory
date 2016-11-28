var target = Argument("target", "Default");

Task("Build")
    .Does(() => {
        Information("Hi!");
    });


Task("Default")
    .IsDependentOn("Build");

RunTarget(target);