using NUnit.Framework;
using UE4Config.Hierarchy;
using UE4Config.Parsing;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class VirtualConfigCacheTests
    {
        [Test]
        public void Constructor()
        {
            var configFileReference = new ConfigFileReference(ConfigDomain.EngineBase, null, "MyConfig");
            
            var virtualConfigCache = new VirtualConfigCache(configFileReference);
                
            Assert.That(virtualConfigCache.FileReference, Is.EqualTo(configFileReference));
            Assert.That(virtualConfigCache.ConfigIni, Is.Null);
            Assert.That(virtualConfigCache.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Unknown));
        }

        [TestFixture]
        public class SetConfigIni
        {
            [Test]
            public void When_WasNotLoaded()
            {
                var configFileReference = new ConfigFileReference(ConfigDomain.EngineBase, null, "MyConfig");
                var virtualConfigCache = new VirtualConfigCache(configFileReference);
                var configIni = new ConfigIni();
                
                virtualConfigCache.SetConfigIni(configIni, false);
                
                Assert.That(virtualConfigCache.FileReference, Is.EqualTo(configFileReference));
                Assert.That(virtualConfigCache.ConfigIni, Is.SameAs(configIni));
                Assert.That(virtualConfigCache.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Created));
            }
            
            [Test]
            public void When_WasLoaded()
            {
                var configFileReference = new ConfigFileReference(ConfigDomain.EngineBase, null, "MyConfig");
                var virtualConfigCache = new VirtualConfigCache(configFileReference);
                var configIni = new ConfigIni();
                
                virtualConfigCache.SetConfigIni(configIni, true);
                
                Assert.That(virtualConfigCache.FileReference, Is.EqualTo(configFileReference));
                Assert.That(virtualConfigCache.ConfigIni, Is.SameAs(configIni));
                Assert.That(virtualConfigCache.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Loaded));
            }
            
            [Test]
            public void When_NullWasNotLoaded()
            {
                var configFileReference = new ConfigFileReference(ConfigDomain.EngineBase, null, "MyConfig");
                var virtualConfigCache = new VirtualConfigCache(configFileReference);
                
                virtualConfigCache.SetConfigIni(null, false);
                
                Assert.That(virtualConfigCache.ConfigIni, Is.Null);
                Assert.That(virtualConfigCache.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Created));
            }
            
            [Test]
            public void When_NullWasLoaded()
            {
                var configFileReference = new ConfigFileReference(ConfigDomain.EngineBase, null, "MyConfig");
                var virtualConfigCache = new VirtualConfigCache(configFileReference);
                
                virtualConfigCache.SetConfigIni(null, true);
                
                Assert.That(virtualConfigCache.ConfigIni, Is.Null);
                Assert.That(virtualConfigCache.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Loaded));
            }
            
            [Test]
            public void When_FileReferenceDiffers()
            {
                var configFileReference = new ConfigFileReference(ConfigDomain.EngineBase, null, "MyConfig");
                var otherConfigFileReference = new ConfigFileReference(ConfigDomain.Engine, null, "MyConfig2");
                var virtualConfigCache = new VirtualConfigCache(configFileReference);
                var configIni = new ConfigIni("MyConfig2", otherConfigFileReference);
                
                virtualConfigCache.SetConfigIni(configIni, false);
                
                Assert.That(virtualConfigCache.FileReference, Is.EqualTo(configFileReference));
                Assert.That(configIni.Reference, Is.EqualTo(otherConfigFileReference));
                Assert.That(virtualConfigCache.ConfigIni, Is.SameAs(configIni));
                Assert.That(virtualConfigCache.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Created));
            }
        }
        
        [Test]
        public void InvalidateCache()
        {
            var configFileReference = new ConfigFileReference(ConfigDomain.EngineBase, null, "MyConfig");
            var virtualConfigCache = new VirtualConfigCache(configFileReference);
            var configIni = new ConfigIni();
            virtualConfigCache.SetConfigIni(configIni, false);
            
            virtualConfigCache.InvalidateCache();
                
            Assert.That(virtualConfigCache.FileReference, Is.EqualTo(configFileReference));
            Assert.That(virtualConfigCache.ConfigIni, Is.Null);
            Assert.That(virtualConfigCache.FileState, Is.EqualTo(VirtualConfigCache.ConfigFileState.Unknown));
        }
    }
}