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
            var values = new List<string>();
            configHierarchy.EvaluatePropertyValues("Windows", "Game", "Internationalization", "LocalizationPaths", values);
            Assert.That(values, Is.EquivalentTo(new[]
            {
                "%GAMEDIR%Content/Localization/Game",
                "%GAMEDIR%Content/Localization/Game/Default",
                "%GAMEDIR%Content/Localization/Game/Win64"
            }));
        }
    }
}
