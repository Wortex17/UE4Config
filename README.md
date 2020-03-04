# UE4Config

A straightlaced C# libary to evaluate & edit Unreal Engine 4 config files, for UE4 projects and built games.
[![GitHub release](https://img.shields.io/github/release/Wortex17/UE4Config)](https://github.com/Wortex17/UE4Config/releases/latest)
[![Build status](https://ci.appveyor.com/api/projects/status/f5tq5q3u4j87a0ux/branch/master?svg=true)](https://ci.appveyor.com/project/Wortex17/UE4Config/branch/master)
[![codecov](https://codecov.io/gh/Wortex17/UE4Config/branch/master/graph/badge.svg)](https://codecov.io/gh/Wortex17/UE4Config)  
[![License](https://img.shields.io/github/license/Wortex17/UE4Config)](https://raw.githubusercontent.com/Wortex17/UE4Config/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/Infrablack.UE4Config)](https://www.nuget.org/packages/Infrablack.UE4Config)

## Features

* Read & parse single \*.ini config files
* Serialization pertains file structure and formatting of the original file by default
* Evaluate the value(s) of any property across one, or multiple config files
* ConfigFileHierarchy emulates UE4s hierarchical config layers and loads config files automatically

## Next to come

* Easier API to modify & add new properties and values
* Sanitization utilities to declutter config files

## Examples

### Evaluate a property from a single config file
You can directly load and read from a single specific config \*.ini file by reading and parsing that file, before evaluating any property values.
```
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
You can also initialize a config hierarchy by providing a engine and/or a project path, and then evaluate any property values for a specific target level & platform.
This emulates best what proeprty values would exist in which context in any Unreal Engine 4 project.

```
//Create a new config hierarchy, with paths to the engine as well as the project directory
var configHierarchy = new FileConfigHierarchy("Mock/Project/Directory", "Mock/Engine/Directory");

//Evaluate the values and put them into a list. Should the property only have a single value, the list will have a single element.
//Should the property not exist or should all its values have been deleted via config, the list will be empty.
var win64Values = new List<string>();

//Evaluate the property "LocalizationPaths" in the section "Internationalization" in the category "Game" for the topmost "Windows"-platform config in the hierarchy.
//This will take all lower hierarchy layers into account.
configHierarchy.EvaluatePropertyValues("Windows", "Game", "Internationalization", "LocalizationPaths", win64Values);

Assert.That(values, Is.EquivalentTo(new[]
  {
    "%GAMEDIR%Content/Localization/Game",
    "%GAMEDIR%Content/Localization/Game/Default",
    "%GAMEDIR%Content/Localization/Game/Win64"
  }));


//It is also possible to ask for a specific level range within the hierarchy
var engineDefaultWin64Values = new List<string>();
configHierarchy.EvaluatePropertyValues("Windows", "Game", "Internationalization", "LocalizationPaths",
    ConfigHierarchyLevel.BasePlatformCategory.AndLower(), engineDefaultWin64Values);
Assert.That(engineDefaultWin64Values, Is.EquivalentTo(new[]
{
    "%GAMEDIR%Content/Localization/Game"
}));

```
