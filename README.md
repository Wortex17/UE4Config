# UE4Config

*Project was not committed yet, this repo was set up just now, please wait a few days ;) *

A straightlaced C# libary to edit Unreal Engine 4 config files, for UE4 projects and built games.
[![GitHub release](https://img.shields.io/github/release/Wortex17/UE4Config)](https://github.com/Wortex17/UE4Config/releases/latest)
[![Build status](https://ci.appveyor.com/api/projects/status/f5tq5q3u4j87a0ux/branch/master?svg=true)](https://ci.appveyor.com/project/Wortex17/UE4Config/branch/master)
[![codecov](https://codecov.io/gh/Wortex17/UE4Config/branch/master/graph/badge.svg)](https://codecov.io/gh/Wortex17/UE4Config)  
[![License](https://img.shields.io/github/license/Wortex17/UE4Config)](https://raw.githubusercontent.com/Wortex17/UE4Config/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/Infrablack.UE4Config)](https://www.nuget.org/packages/Infrablack.UE4Config)


## Examples

### Evaluate a propertys values from a single config file

```
var config = new ConfigIni("DefaultGame");
//Load the configs contents from a file
config.Read(File.OpenText(TestUtils.GetTestDataPath("MockProject/Config/DefaultGame.ini")));
//Evaluate the values and put them into a list. Should the property only have a single value, the list will have a single element.
//Should the property not exist or should all its values have been deleted via config, the list will be empty.
var values = new List<string>();
config.EvaluatePropertyValues("/Script/EngineSettings.GeneralProjectSettings", "ProjectID", values);
Assert.That(values, Is.EquivalentTo(new[]{"3F9D696D4363312194B0ECB2671E899F"}));
```
