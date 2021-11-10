using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UE4Config.Hierarchy;
using UE4Config.Parsing;

namespace UE4Config.Tests
{
    /// <summary>
    /// Tests for all samples shown in readmes or docs.
    /// </summary>
    [TestFixture]
    class SampleTests
    {
        [TestCase]
        public void When_ReadConfig_DefaultGame()
        {
            //Create a new config with a name for identification
            var config = new ConfigIni("DefaultGame");

            //Load the configs contents from a file
            config.Read(File.OpenText(TestUtils.GetTestDataPath("MockProject/Config/DefaultGame.ini")));

            //Evaluate the values and put them into a list. Should the property only have a single value, the list will have a single element.
            //Should the property not exist or should all its values have been deleted via config, the list will be empty.
            var values = new List<string>();
            config.EvaluatePropertyValues("/Script/EngineSettings.GeneralProjectSettings", "ProjectID", values);

            Assert.That(values, Is.EquivalentTo(new[]{"3F9D696D4363312194B0ECB2671E899F"}));
        }

        [TestCase]
        public void When_ReadConfigFromHierarchy_WindowsGame()
        {
            //Create a new config hierarchy, with paths to the engine as well as the project directory
            var configHierarchy = new FileConfigHierarchy(TestUtils.GetTestDataPath("MockProject"), TestUtils.GetTestDataPath("MockEngine"));

            //Evaluate the values and put them into a list. Should the property only have a single value, the list will have a single element.
            //Should the property not exist or should all its values have been deleted via config, the list will be empty.
            var win64Values = new List<string>();

            //Evaluate the property "LocalizationPaths" in the section "Internationalization" int the category "Game" for the topmost "Windows"-platform config in the hierarchy.
            //This will take all lower hierarchy layers into account.
            configHierarchy.EvaluatePropertyValues("Windows", "Game", "Internationalization", "LocalizationPaths", win64Values);

            Assert.That(win64Values, Is.EquivalentTo(new[]
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
        }
        
        [TestCase]
        public void When_ModifyConfigFileByAppend()
        {
            //Create a new config hierarchy, with paths to the engine as well as the project directory
            var configHierarchy = new FileConfigHierarchy(TestUtils.GetTestDataPath("MockProject"), TestUtils.GetTestDataPath("MockEngine"));
            
            //Acquire the target config we want to modify
            var config = configHierarchy.GetOrCreateConfig("Windows", "Game", ConfigHierarchyLevel.ProjectPlatformCategory, out _);

            //We modify the config by just appending further configuration which will redefine properties
            config.AppendRawText("[Global]\n" +
                                 "+GUIDs=modifieda44d");
            //Here we use the config ini syntax to add another value to the list

            //Cleanup the config before publishing it
            config.NormalizeLineEndings();
            config.MergeDuplicateSections();
            config.GroupPropertyInstructions();
            config.CondenseWhitespace();

            //Publish the config and write it back
            configHierarchy.PublishConfig("Windows", "Game", ConfigHierarchyLevel.ProjectPlatformCategory, config);

            //Make sure the modified value made it in
            var win64Values = new List<string>();
            configHierarchy.EvaluatePropertyValues("Windows", "Game", "Global", "GUIDs", win64Values);
            Assert.That(win64Values, Is.EquivalentTo(new[]
            {
                "a44b",
                "modifieda44d"
            }));
        }
    }
}
