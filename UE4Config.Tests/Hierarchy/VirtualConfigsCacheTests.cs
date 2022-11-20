using NUnit.Framework;
using UE4Config.Hierarchy;
using UE4Config.Parsing;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class VirtualConfigsCacheTests
    {
        class MockConfigFileProvider : IConfigFileProvider
        {
            public delegate bool OnLoadOrCreateConfigDelegate(ConfigFileReference configFileReference,
                out ConfigIni configIni);

            public OnLoadOrCreateConfigDelegate OnLoadOrCreateConfig;
                
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

            public void SaveConfig(ConfigFileReference configFileReference, ConfigIni configIni)
            {
                throw new System.NotImplementedException();
            }
        }
        
        [Test]
        public void Peek()
        {
            var virtualConfigsCache = new VirtualConfigsCache();
            var configFileReference = new ConfigFileReference(ConfigDomain.Engine, null, "MyConfig");
            
            var virtualConfigCache = virtualConfigsCache.Peek(configFileReference);

            Assert.That(virtualConfigCache.FileReference, Is.EqualTo(configFileReference));
            Assert.That(virtualConfigCache.ConfigIni, Is.Null);
            Assert.That(virtualConfigCache.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Unknown));
        }
        
        [Test]
        public void Peek_Twice()
        {
            var virtualConfigsCache = new VirtualConfigsCache();
            var configFileReference = new ConfigFileReference(ConfigDomain.Engine, null, "MyConfig");
            
            var virtualConfigCache1 = virtualConfigsCache.Peek(configFileReference);
            var virtualConfigCache2 = virtualConfigsCache.Peek(configFileReference);

            Assert.That(virtualConfigCache1, Is.SameAs(virtualConfigCache2));
        }
        
        [Test]
        public void Peek_DifferentTwice()
        {
            var virtualConfigsCache = new VirtualConfigsCache();
            var configFileReference1 = new ConfigFileReference(ConfigDomain.Engine, null, "MyConfig1");
            var configFileReference2 = new ConfigFileReference(ConfigDomain.Engine, null, "MyConfig2");
            
            var virtualConfigCache1 = virtualConfigsCache.Peek(configFileReference1);
            var virtualConfigCache2 = virtualConfigsCache.Peek(configFileReference2);

            Assert.That(virtualConfigCache1, Is.Not.SameAs(virtualConfigCache2));
        }

        [TestFixture]
        public class GetOrLoadConfig
        {
            
            [Test]
            public void When_CannotLoadExisting()
            {
                var virtualConfigsCache = new VirtualConfigsCache();
                var configFileReference = new ConfigFileReference(ConfigDomain.Engine, null, "MyConfig");
                var configFileProvider = new MockConfigFileProvider();
                var expectedConfig = new ConfigIni();
                bool expectedConfigWasLoaded = false;
                int callCount_OnLoadOrCreateConfig = 0;
                configFileProvider.OnLoadOrCreateConfig = (ConfigFileReference reference, out ConfigIni ini) =>
                {
                    callCount_OnLoadOrCreateConfig++;
                    ini = expectedConfig;
                    return expectedConfigWasLoaded;
                };
                
                var result = virtualConfigsCache.GetOrLoadConfig(configFileReference, configFileProvider);
                var virtualConfigCache = virtualConfigsCache.Peek(configFileReference);

                Assert.That(callCount_OnLoadOrCreateConfig, Is.EqualTo(1));
                Assert.That(result, Is.SameAs(expectedConfig));
                Assert.That(result, Is.SameAs(virtualConfigCache.ConfigIni));
                Assert.That(virtualConfigCache.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Created));
            }
            
            [Test]
            public void When_LoadsExisting()
            {
                var virtualConfigsCache = new VirtualConfigsCache();
                var configFileReference = new ConfigFileReference(ConfigDomain.Engine, null, "MyConfig");
                var configFileProvider = new MockConfigFileProvider();
                var expectedConfig = new ConfigIni();
                bool expectedConfigWasLoaded = true;
                int callCount_OnLoadOrCreateConfig = 0;
                configFileProvider.OnLoadOrCreateConfig = (ConfigFileReference reference, out ConfigIni ini) =>
                {
                    callCount_OnLoadOrCreateConfig++;
                    ini = expectedConfig;
                    return expectedConfigWasLoaded;
                };
                
                var result = virtualConfigsCache.GetOrLoadConfig(configFileReference, configFileProvider);
                var virtualConfigCache = virtualConfigsCache.Peek(configFileReference);

                Assert.That(callCount_OnLoadOrCreateConfig, Is.EqualTo(1));
                Assert.That(result, Is.SameAs(expectedConfig));
                Assert.That(result, Is.SameAs(virtualConfigCache.ConfigIni));
                Assert.That(virtualConfigCache.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Loaded));
            }
            
            [Test]
            public void When_CalledTwice()
            {
                var virtualConfigsCache = new VirtualConfigsCache();
                var configFileReference = new ConfigFileReference(ConfigDomain.Engine, null, "MyConfig");
                var configFileProvider = new MockConfigFileProvider();
                var expectedConfig = new ConfigIni();
                bool expectedConfigWasLoaded = true;
                int callCount_OnLoadOrCreateConfig = 0;
                configFileProvider.OnLoadOrCreateConfig = (ConfigFileReference reference, out ConfigIni ini) =>
                {
                    callCount_OnLoadOrCreateConfig++;
                    ini = expectedConfig;
                    return expectedConfigWasLoaded;
                };
                
                var result1 = virtualConfigsCache.GetOrLoadConfig(configFileReference, configFileProvider);
                var result2 = virtualConfigsCache.GetOrLoadConfig(configFileReference, configFileProvider);

                Assert.That(callCount_OnLoadOrCreateConfig, Is.EqualTo(1));
                Assert.That(result1, Is.SameAs(expectedConfig));
                Assert.That(result2, Is.SameAs(expectedConfig));
            }
        }
        
        [Test]
        public void InvalidateCache()
        {
            var virtualConfigsCache = new VirtualConfigsCache();
            var configFileReference1 = new ConfigFileReference(ConfigDomain.Engine, null, "MyConfig1");
            var configFileReference2 = new ConfigFileReference(ConfigDomain.Engine, null, "MyConfig2");
            
            var configFileProvider = new MockConfigFileProvider();
            configFileProvider.OnLoadOrCreateConfig = (ConfigFileReference reference, out ConfigIni ini) =>
            {
                ini = new ConfigIni();
                return true;
            };
            
            virtualConfigsCache.GetOrLoadConfig(configFileReference1, configFileProvider);
            virtualConfigsCache.GetOrLoadConfig(configFileReference2, configFileProvider);
            
            virtualConfigsCache.InvalidateCache();
            
            var virtualConfigCache1 = virtualConfigsCache.Peek(configFileReference1);
            var virtualConfigCache2 = virtualConfigsCache.Peek(configFileReference2);

            Assert.That(virtualConfigCache1.ConfigIni, Is.Null);
            Assert.That(virtualConfigCache1.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Unknown));
            Assert.That(virtualConfigCache2.ConfigIni, Is.Null);
            Assert.That(virtualConfigCache2.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Unknown));
        }
    }
}