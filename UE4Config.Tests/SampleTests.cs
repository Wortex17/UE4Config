using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UE4Config.Evaluation;
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
        void ResetTestDirectories()
        {
            try
            {
                Directory.Delete(TestUtils.GetTestDataPath("MockProjectTmp"), true);
            } catch(DirectoryNotFoundException) {}
            try
            {
                Directory.Delete(TestUtils.GetTestDataPath("MockEngineTmp"), true);
            } catch(DirectoryNotFoundException) {}
            
            TestUtils.CopyDirectory(TestUtils.GetTestDataPath("MockProject"), TestUtils.GetTestDataPath("MockProjectTmp"), true);
            TestUtils.CopyDirectory(TestUtils.GetTestDataPath("MockEngine"), TestUtils.GetTestDataPath("MockEngineTmp"), true);
        }

        [TestCase]
        public void When_ReadConfig_DefaultGame()
        {
            //Create a new virtual config with a name for identification
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
        public void When_ReadConfigFromHierarchy_WindowsGame_ConfigTree()
        {
            ResetTestDirectories();
            var enginePath = TestUtils.GetTestDataPath("MockEngineTmp");
            var projectPath = TestUtils.GetTestDataPath("MockProjectTmp");
            
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
        }
        
        [TestCase]
        public void When_ModifyConfigFileByAppend_ConfigTree()
        {
            ResetTestDirectories();
            var enginePath = TestUtils.GetTestDataPath("MockEngineTmp");
            var projectPath = TestUtils.GetTestDataPath("MockProjectTmp");
            
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
        }

        [TestCase]
        public void When_CreatingVirtualConfigTreeManually()
        {
            ResetTestDirectories();
            var enginePath = TestUtils.GetTestDataPath("MockEngineTmp");
            var projectPath = TestUtils.GetTestDataPath("MockProjectTmp");
            
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
            
            Assert.That(autoTree, Is.InstanceOf(configTree.GetType()));
            Assert.That(autoTree.FileProvider, Is.InstanceOf(configTree.FileProvider.GetType()));
            Assert.That(autoTree.FileProvider.FileIOAdapter, Is.InstanceOf(configTree.FileProvider.FileIOAdapter.GetType()));
            Assert.That(autoTree.ReferenceTree, Is.InstanceOf(configTree.ReferenceTree.GetType()));

            var autoBranch = autoTree.FetchConfigBranch("Game", "Windows");
            var manualBranch = configTree.FetchConfigBranch("Game", "Windows");
            Assert.That(autoBranch, Has.Count.EqualTo(manualBranch.Count));
            for (int i = 0; i < autoBranch.Count; i++)
            {
                var autoConfig = autoBranch[i];
                var manualConfig = manualBranch[i];
                var autoConfigWriter = new ConfigIniWriter(new StringWriter());
                autoConfig.Write(autoConfigWriter);
                var manualConfigWriter = new ConfigIniWriter(new StringWriter());
                manualConfig.Write(manualConfigWriter);
                
                Assert.That(autoConfig.Name, Is.EqualTo(manualConfig.Name));
                Assert.That(autoConfig.Reference.ToString(), Is.EqualTo(manualConfig.Reference.ToString()));
                Assert.That(autoConfigWriter.ToString(), Is.EqualTo(manualConfigWriter.ToString()));
            }
        }
    }
}
