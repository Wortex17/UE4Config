using NUnit.Framework;
using UE4Config.Hierarchy;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class ConfigFileReferenceTests
    {
        [TestFixture]
        public class Constructor
        {
            [Test]
            public void When_Default()
            {
                var configFileReference = new ConfigFileReference();
                
                Assert.That(configFileReference.Domain, Is.EqualTo(ConfigDomain.None));
                Assert.That(configFileReference.Platform, Is.Null);
                Assert.That(configFileReference.Type, Is.Null);
            }
            
            [TestCase("")]
            [TestCase(" ")]
            [TestCase("\t")]
            public void When_WhitespaceInType(string type)
            {
                Assert.That(() =>
                {
                    var configFileReference = new ConfigFileReference(ConfigDomain.None, null, "  ");
                }, Throws.ArgumentException);
            }
            
            [TestCase("base")]
            [TestCase("Base")]
            [TestCase("Default")]
            [TestCase("default")]
            public void When_KeywordInType(string type)
            {
                Assert.That(() =>
                {
                    var configFileReference = new ConfigFileReference(ConfigDomain.None, null, type);
                }, Throws.ArgumentException);
            }
        }
        
        [Test]
        public void IsPlatformConfig_WhenNoPlatform()
        {
            var configFileReference = new ConfigFileReference(ConfigDomain.Engine, null, "MyConfig");
                
            Assert.That(configFileReference.IsPlatformConfig, Is.False);
        }
        
        [Test]
        public void IsPlatformConfig_WhenPlatform()
        {
            var configFileReference = new ConfigFileReference(ConfigDomain.Engine, new ConfigPlatform("MyPlatform"), "MyConfig");
                
            Assert.That(configFileReference.IsPlatformConfig, Is.True);
        }
    }
}