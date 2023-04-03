# UE4Config

A straightlaced C# libary to evaluate & edit Unreal Engine 4 config files, for UE4 projects and built games.  
[![GitHub release](https://img.shields.io/github/release/Wortex17/UE4Config)](https://github.com/Wortex17/UE4Config/releases/latest)
[![Build status](https://ci.appveyor.com/api/projects/status/f5tq5q3u4j87a0ux/branch/master?svg=true)](https://ci.appveyor.com/project/Wortex17/UE4Config/branch/master)
[![Nuget](https://img.shields.io/nuget/v/Infrablack.UE4Config)](https://www.nuget.org/packages/Infrablack.UE4Config)  
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/f679eceb343c47d581494ad6b6b9f809)](https://app.codacy.com/manual/Wortex17/UE4Config?utm_source=github.com&utm_medium=referral&utm_content=Wortex17/UE4Config&utm_campaign=Badge_Grade_Dashboard)
[![codecov](https://codecov.io/gh/Wortex17/UE4Config/branch/master/graph/badge.svg)](https://codecov.io/gh/Wortex17/UE4Config)
[![License](https://img.shields.io/github/license/Wortex17/UE4Config)](https://raw.githubusercontent.com/Wortex17/UE4Config/master/LICENSE)


## Features

* Read & parse single \*.ini config files
* Serialization retains file structure and formatting of the original file by default
* Evaluate the value(s) of any property across one, or multiple config files
* ConfigTree classes emulate UE4s hierarchical config layers and load config files automatically
* Supports new "PlatformExtension" folder structure & configs of UE4.24+
* Supports DataDrivenPlatform definitions of UE 4.27+

## Next to come

* Easier API to modify & add new properties and values

## Examples

### Evaluate a property from a single config file
You can directly load and read from a single specific config \*.ini file by reading and parsing that file, before evaluating any property values.
```C#
var config = new ConfigIni("DefaultGame");

//Load the configs contents from a file, via a read stream
config.Read(File.OpenText("MockProject/Config/DefaultGame.ini");

//Evaluate the values and put them into a list. Should the property only have a single value, the list will have a single element.
//Should the property not exist or should all its values have been deleted via config, the list will be empty.
var values = new List<string>();
config.EvaluatePropertyValues("/Script/EngineSettings.GeneralProjectSettings", "ProjectID", values);

Assert.That(values, Is.EquivalentTo(new[]{"3F9D696D4363312194B0ECB2671E899F"}));
```

### Evaluate a property from a config hierarchy
You can use the virtual config tree to work with config files in memory by providing a engine and/or a project path.
It can provide you branches based on target category (like Game, Editor, Input etc.) and platform you want to use.
This emulates best what property values would be in which context of any Unreal Engine 4 project.
```C#
//Create a new virtual config tree to allow us working with an in-memory virtual hierarchy
var configTree = VirtualConfigTreeUtility.CreateVirtualConfigTree(enginePath, projectPath);

//Evaluate the values and put them into a list. Should the property only have a single value, the list will have a single element.
//Should the property not exist or should all its values have been deleted via config, the list will be empty.
var win64Values = new List<string>();

//Evaluate the property "LocalizationPaths" in the section "Internationalization" in the category "Game" for the topmost "Windows"-platform config in the hierarchy.
//This will take all lower hierarchy layers into account.
var configBranch = configTree.FetchConfigBranch("Game", "Windows");
PropertyEvaluator.Default.EvaluatePropertyValues(configBranch, "Internationalization", "LocalizationPaths", win64Values);

Assert.That(win64Values, Is.EquivalentTo(new[]
{
    "%GAMEDIR%Content/Localization/Game",
    "%GAMEDIR%Content/Localization/Game/Default",
    "%GAMEDIR%Content/Localization/Game/Win64"
}));

//It is also possible to ask for a specific level range within the hierarchy
var filteredBranch = configBranch.FindAll((ini => ini.Reference.Domain <= ConfigDomain.Engine));
var engineDefaultWin64Values = new List<string>();
PropertyEvaluator.Default.EvaluatePropertyValues(filteredBranch, "Internationalization", "LocalizationPaths", engineDefaultWin64Values);
Assert.That(engineDefaultWin64Values, Is.EquivalentTo(new[]
{
    "%GAMEDIR%Content/Localization/Game"
}));
```

### Modify configs in a config hierarchy
You can use the virtual config tree to work with config files in memory
and save them to disk after making modifications.
```C#
//Create a new virtual config tree to allow us working with an in-memory virtual hierarchy
var configTree = VirtualConfigTreeUtility.CreateVirtualConfigTree(enginePath, projectPath);

//Acquire the target config ("Game" on Platform "Windows") we want to modify
//Select the trees branch first, by providing a config category and the platform we're branching on
var configBranch = configTree.FetchConfigBranch("Game", "Windows");
//Select the head config on that branch that is still in "Engine" domain
var config = configBranch.SelectHeadConfig(ConfigDomain.Project);
//This will be the {Project}/Config/Windows/WindowsGame.ini

//We modify the config by just appending further configuration which will redefine properties
config.AppendRawText("[Global]\n" +
                     "+GUIDs=modifieda44d");
//Here we use the config ini syntax to add another value to the list

//Cleanup the config before publishing it
config.Sanitize();

//Publish the config and write it back
configTree.PublishConfig(config);

//Make sure the modified value made it into the file:
//Invalidate our cache to reload the branch from disk
configTree.ConfigsCache.InvalidateCache();
configBranch = configTree.FetchConfigBranch("Game", "Windows");
//Resolve the value on the branch
var win64Values = new List<string>();
PropertyEvaluator.Default.EvaluatePropertyValues(configBranch, "Global", "GUIDs", win64Values);
Assert.That(win64Values, Is.EquivalentTo(new[]
{
    "a44b",
    "modifieda44d"
}));
```

### Setting up a VirtualConfigTree
While there is the utility method to create a virtual config tree,
you may set it up manually to customize behavior.
```C#
var autoTree = VirtualConfigTreeUtility.CreateVirtualConfigTree(enginePath, projectPath);
            
//This will provide paths and a virtual hierarchy for a project+engine base path combination
var configProvider = new ConfigFileProvider();
configProvider.Setup(new ConfigFileIOAdapter(), TestUtils.GetTestDataPath("MockEngineTmp"), TestUtils.GetTestDataPath("MockProjectTmp"));
//Auto-detect if a project still uses the legacy Config/{Platform}/*.ini setup.
configProvider.AutoDetectPlatformsUsingLegacyConfig();
//Create a DataDrivenPlatform provider that can pre-fill our platform hierarchy like UE4.27 does (based on DataDrivenPlatform configs)
var dataDrivenPlatformProvider = new DataDrivenPlatformProvider();
dataDrivenPlatformProvider.Setup(configProvider);
dataDrivenPlatformProvider.CollectDataDrivenPlatforms();
//Create the base tree model, based on the config hierarchy used by UE since 4.27+
var configRefTree = new ConfigReferenceTree427();
configRefTree.Setup(configProvider);
//Apply any detected DataDrivenPlatforms
dataDrivenPlatformProvider.RegisterDataDrivenPlatforms(configRefTree);
//Create a virtual config tree to allow us working with an in-memory virtual hierarchy
var configTree = new VirtualConfigTree(configRefTree);

//autoTree and configTree have the exact same setup (though different instances)
```