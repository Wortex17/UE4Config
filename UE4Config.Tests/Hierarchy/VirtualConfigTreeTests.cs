using System.Collections.Generic;
using NUnit.Framework;
using UE4Config.Hierarchy;
using UE4Config.Parsing;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class VirtualConfigTreeTests
    {
        class MockConfigFileProvider : IConfigFileProvider
        {
            public delegate bool OnLoadOrCreateConfigDelegate(ConfigFileReference configFileReference,
                out ConfigIni configIni);
            public OnLoadOrCreateConfigDelegate OnLoadOrCreateConfig;
            
            public delegate void OnSaveConfigDelegate(ConfigFileReference configFileReference,
                ConfigIni configIni);
            public OnSaveConfigDelegate OnSaveConfig;
                
            public IConfigFileIOAdapter FileIOAdapter { get; }
            public string EnginePath { get; }
            public string ProjectPath { get; }
            public bool IsSetup { get; }
            public void Setup(IConfigFileIOAdapter fileIOAdapter, string enginePath, string projectPath)
            {
                throw new System.NotImplementedException();
            }

            public string ResolveConfigFilePath(ConfigFileReference reference)
            {
                throw new System.NotImplementedException();
            }

            public bool LoadOrCreateConfig(ConfigFileReference configFileReference, out ConfigIni configIni)
            {
                return OnLoadOrCreateConfig(configFileReference, out configIni);
            }

            public bool LoadOrCreateDataDrivenPlatformConfig(string platformIdentifier, out ConfigIni configIni)
            {
                throw new System.NotImplementedException();
            }

            public void SaveConfig(ConfigFileReference configFileReference, ConfigIni configIni)
            {
                OnSaveConfig(configFileReference, configIni);
            }
        }

        [Test]
        public void Constructor()
        {
            var configFileProvider = new MockConfigFileProvider();
            var configRefTree = new ConfigReferenceTree427();
            configRefTree.Setup(configFileProvider);
            
            var virtualConfigTree = new VirtualConfigTree(configRefTree);

            Assert.That(virtualConfigTree.ReferenceTree, Is.SameAs(configRefTree));
            Assert.That(virtualConfigTree.FileProvider, Is.SameAs(configRefTree.FileProvider));
            Assert.That(virtualConfigTree.ConfigsCache, Is.Not.Null);
        }
        
        [Test]
        public void FetchConfigRoot()
        {
            var configFileProvider = new MockConfigFileProvider();
            var configRefTree = new ConfigReferenceTree427();
            configRefTree.Setup(configFileProvider);
            var virtualConfigTree = new VirtualConfigTree(configRefTree);
            var rootConfig = new ConfigIni();
            virtualConfigTree.ConfigsCache.Peek((ConfigFileReference)virtualConfigTree.ReferenceTree.GetConfigRoot()).SetConfigIni(rootConfig, true);
            
            var result = virtualConfigTree.FetchConfigRoot();

            Assert.That(result, Is.SameAs(rootConfig));
        }
        
        [Test]
        public void FetchConfigBranch()
        {
            var configFileProvider = new MockConfigFileProvider();
            var configRefTree = new ConfigReferenceTree427();
            configRefTree.Setup(configFileProvider);
            var virtualConfigTree = new VirtualConfigTree(configRefTree);
            var refBranch = configRefTree.GetConfigBranch("MyConfig", null);
            var expectedBranch = new List<ConfigIni>();
            foreach (var refBranchItem in refBranch)
            {
                var configForItem = new ConfigIni();
                expectedBranch.Add(configForItem);
                virtualConfigTree.ConfigsCache.Peek(refBranchItem).SetConfigIni(configForItem, true);
            }
            
            var result = virtualConfigTree.FetchConfigBranch("MyConfig", null);

            Assert.That(result, Is.EquivalentTo(expectedBranch));
        }
        
        [Test]
        public void PublishConfig()
        {
            var configFileProvider = new MockConfigFileProvider();
            var configRefTree = new ConfigReferenceTree427();
            configRefTree.Setup(configFileProvider);
            var virtualConfigTree = new VirtualConfigTree(configRefTree);
            var configFileReference = new ConfigFileReference(ConfigDomain.Project, null, "MyConfig");
            var editConfig = new ConfigIni("MyConfig", configFileReference);
            editConfig.AppendRawText("foobar"); //Make the config contain anything
            
            ConfigIni configToSave = null;
            int callCountOnSaveConfig = 0;
            configFileProvider.OnSaveConfig = (ConfigFileReference reference, ConfigIni ini) =>
            {
                callCountOnSaveConfig++;
                configToSave = ini;
            };
                
            virtualConfigTree.PublishConfig(editConfig);
            var cached = virtualConfigTree.ConfigsCache.Peek(editConfig.Reference);

            Assert.That(callCountOnSaveConfig, Is.EqualTo(1));
            Assert.That(configToSave, Is.SameAs(editConfig));
            Assert.That(cached.ConfigIni, Is.SameAs(editConfig));
            Assert.That(cached.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Loaded));
        }
    }
}