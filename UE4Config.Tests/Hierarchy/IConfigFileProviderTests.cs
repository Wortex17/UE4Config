using System;
using NUnit.Framework;
using UE4Config.Hierarchy;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture(typeof(ConfigFileProvider))]
    public class IConfigFileProviderTests<T> where T : IConfigFileProvider, new()
    {
        protected T NewProvider()
        {
            return new T();
        }

        [TestCase("%Engine%", "%Project%")]
        [TestCase(null, "%Project%")]
        [TestCase("%Engine%", null)]
        public void Setup(string enginePath, string projectPath)
        {
            var provider = NewProvider();
            var fileIOAdapter = new ConfigFileIOAdapter();
            var expectedEnginePath = enginePath;
            var expectedProjectPath = projectPath;
            
            Assert.That(provider.IsSetup, Is.False);
            provider.Setup(fileIOAdapter, enginePath, projectPath);
            
            Assert.That(provider.FileIOAdapter, Is.SameAs(fileIOAdapter));
            Assert.That(provider.EnginePath, Is.EqualTo(expectedEnginePath));
            Assert.That(provider.ProjectPath, Is.EqualTo(expectedProjectPath));
            Assert.That(provider.IsSetup, Is.True);
        }
        
        [Test]
        public void Setup_WithoutPaths()
        {
            var provider = NewProvider();
            var fileIOAdapter = new ConfigFileIOAdapter();
            provider.Setup(fileIOAdapter, null, null);
            
            Assert.That(provider.FileIOAdapter, Is.SameAs(fileIOAdapter));
            Assert.That(provider.EnginePath, Is.Null);
            Assert.That(provider.ProjectPath, Is.Null);
            Assert.That(provider.IsSetup, Is.False);
        }
        
        [Test]
        public void Setup_WithoutIOAdapter()
        {
            var provider = NewProvider();
            var expectedEnginePath = "%Engine%";
            var expectedProjectPath = "%Project%";
            provider.Setup(null, expectedEnginePath, expectedProjectPath);
            
            Assert.That(provider.FileIOAdapter, Is.Null);
            Assert.That(provider.EnginePath, Is.EqualTo(expectedEnginePath));
            Assert.That(provider.ProjectPath, Is.EqualTo(expectedProjectPath));
            Assert.That(provider.IsSetup, Is.False);
        }
        
        [TestCase(ConfigDomain.Engine, "MyPlatform", "MyConfig")]
        [TestCase(ConfigDomain.Engine, "PS4", "Game")]
        public void ResolveConfigFilePath_WithoutSetup(ConfigDomain domain, string platform, string type)
        {
            var provider = NewProvider();

            var result = provider.ResolveConfigFilePath(new ConfigFileReference(domain, new ConfigPlatform(platform), type));
            
            Assert.That(provider.FileIOAdapter, Is.Null);
            Assert.That(provider.EnginePath, Is.Null);
            Assert.That(provider.ProjectPath, Is.Null);
            Assert.That(provider.IsSetup, Is.False);
            Assert.That(result, Is.Null);
        }
        
        
        [TestCase(ConfigDomain.Engine, null, null)]
        [TestCase(ConfigDomain.Project, null, null)]
        [TestCase(ConfigDomain.ProjectGenerated, null, null)]
        [TestCase(ConfigDomain.Custom, "MyPlatform", null)]
        [TestCase(ConfigDomain.EngineBase, "MyPlatform", null)]
        [TestCase(ConfigDomain.Engine, "MyPlatform", null)]
        [TestCase(ConfigDomain.Project, "MyPlatform", null)]
        [TestCase(ConfigDomain.ProjectGenerated, "MyPlatform", null)]
        [TestCase(ConfigDomain.Custom, null, null)]
        [TestCase(ConfigDomain.Custom, null, "MyConfig")]
        [TestCase(ConfigDomain.Custom, "MyPlatform", null)]
        [TestCase(ConfigDomain.Custom, "MyPlatform", "MyConfig")]
        public void ResolveConfigFilePath_WithInvalidReference(ConfigDomain domain, string platform, string type)
        {
            var provider = NewProvider();
            var ioAdapter = new ConfigFileIOAdapter();
            provider.Setup(ioAdapter, "%Engine%", "%Project%");

            var result = provider.ResolveConfigFilePath(new ConfigFileReference(domain, new ConfigPlatform(platform), type));
            
            Assert.That(result, Is.Null);
        }
        
        [TestCase(ConfigDomain.EngineBase, null, null)]
        [TestCase(ConfigDomain.Engine, null, "MyConfig")]
        [TestCase(ConfigDomain.Project, null, "MyConfig")]
        [TestCase(ConfigDomain.ProjectGenerated, null, "MyConfig")]
        [TestCase(ConfigDomain.EngineBase, null, "MyConfig")]
        [TestCase(ConfigDomain.Engine, null, "MyConfig")]
        [TestCase(ConfigDomain.Project, null, "MyConfig")]
        [TestCase(ConfigDomain.ProjectGenerated, null, "MyConfig")]
        [TestCase(ConfigDomain.EngineBase, "MyPlatform", "MyConfig")]
        [TestCase(ConfigDomain.Engine, "MyPlatform", "MyConfig")]
        [TestCase(ConfigDomain.Project, "MyPlatform", "MyConfig")]
        [TestCase(ConfigDomain.ProjectGenerated, "MyPlatform", "MyConfig")]
        public void ResolveConfigFilePath_WithValidReference(ConfigDomain domain, string platform, string type)
        {
            var provider = NewProvider();
            var ioAdapter = new ConfigFileIOAdapter();
            provider.Setup(ioAdapter, "%Engine%", "%Project%");

            var result = provider.ResolveConfigFilePath(new ConfigFileReference(domain, new ConfigPlatform(platform), type));
            
            Assert.That(result, Is.Not.Null);
            Assert.That(String.IsNullOrWhiteSpace(result), Is.False);
        }
    }
}