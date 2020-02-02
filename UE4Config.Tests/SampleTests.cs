using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
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
            //Evalkuate the values and put them into a list. Should the property only have a single value, the list will have a single element.
            //SHould the property not exist or should all its values have been deleted via config, the list will be empty.
            var values = new List<string>();
            config.EvaluatePropertyValues("/Script/EngineSettings.GeneralProjectSettings", "ProjectID", values);
            Assert.That(values, Is.EquivalentTo(new[]{"3F9D696D4363312194B0ECB2671E899F"}));
        }
    }
}
