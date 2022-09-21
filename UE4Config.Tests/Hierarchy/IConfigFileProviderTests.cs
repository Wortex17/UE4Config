using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UE4Config.Hierarchy;
using UE4Config.Parsing;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture(typeof(ConfigFileProvider))]
    public class IConfigFileProviderTests<T> where T : IConfigFileProvider, new()
    {
        protected static T NewProvider()
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
        [TestCase(ConfigDomain.None, "MyPlatform", null)]
        [TestCase(ConfigDomain.EngineBase, "MyPlatform", null)]
        [TestCase(ConfigDomain.Engine, "MyPlatform", null)]
        [TestCase(ConfigDomain.Project, "MyPlatform", null)]
        [TestCase(ConfigDomain.ProjectGenerated, "MyPlatform", null)]
        [TestCase(ConfigDomain.None, null, null)]
        [TestCase(ConfigDomain.None, null, "MyConfig")]
        [TestCase(ConfigDomain.None, "MyPlatform", null)]
        [TestCase(ConfigDomain.None, "MyPlatform", "MyConfig")]
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

        class MockConfigFileIOAdapter : IConfigFileIOAdapter
        {
            public delegate List<string> GetDirectoriesDelegate(string pivotPath);
            public GetDirectoriesDelegate OnGetDirectories;
            
            public delegate StreamReader OpenTextDelegate(string filePath);
            public OpenTextDelegate OnOpenText = path => StreamReader.Null;
            
            public delegate StreamWriter OpenWriteDelegate(string filePath);
            public OpenWriteDelegate OnOpenWrite = path => StreamWriter.Null;
                
            public List<string> GetDirectories(string pivotPath)
            {
                return OnGetDirectories(pivotPath);
            }

            public StreamReader OpenText(string filePath)
            {
                return OnOpenText(filePath);
            }

            public StreamWriter OpenWrite(string filePath)
            {
                return OnOpenWrite(filePath);
            }
        }
        
        [Test]
        public void LoadOrCreateConfig_ResolvesToFile()
        {
            const string configType = "MyConfig";
            var provider = NewProvider();
            var ioAdapter = new MockConfigFileIOAdapter();
            provider.Setup(ioAdapter, "%Engine%", "%Project%");
            var configFileRef = new ConfigFileReference(ConfigDomain.Engine, null, configType);

            int openText_CallCount = 0;
            ioAdapter.OnOpenText = path =>
            {
                openText_CallCount++;
                Assert.That(path, Is.EqualTo(provider.ResolveConfigFilePath(configFileRef)));
                return StreamReader.Null;
            };
            
            bool wasLoaded = provider.LoadOrCreateConfig(configFileRef, out var config);
            
            Assert.That(wasLoaded, Is.True);
            Assert.That(config, Is.Not.Null);
            Assert.That(config.Reference, Is.EqualTo(configFileRef));
            Assert.That(String.IsNullOrWhiteSpace(config.Name), Is.False);
            Assert.That(openText_CallCount, Is.EqualTo(1));
        }
        
        [Test]
        public void LoadOrCreateConfig_DirectoryNotFoundException()
        {
            const string configType = "MyConfig";
            var provider = NewProvider();
            var ioAdapter = new MockConfigFileIOAdapter();
            provider.Setup(ioAdapter, "%Engine%", "%Project%");
            var configFileRef = new ConfigFileReference(ConfigDomain.Engine, null, configType);

            int openText_CallCount = 0;
            ioAdapter.OnOpenText = path =>
            {
                openText_CallCount++;
                throw new DirectoryNotFoundException();
            };
            
            bool wasLoaded = provider.LoadOrCreateConfig(configFileRef, out var config);
            
            Assert.That(wasLoaded, Is.False);
            Assert.That(config, Is.Not.Null);
            Assert.That(config.Reference, Is.EqualTo(configFileRef));
            Assert.That(String.IsNullOrWhiteSpace(config.Name), Is.False);
            Assert.That(openText_CallCount, Is.EqualTo(1));
        }
        
        [Test]
        public void LoadOrCreateConfig_FileNotFoundException()
        {
            const string configType = "MyConfig";
            var provider = NewProvider();
            var ioAdapter = new MockConfigFileIOAdapter();
            provider.Setup(ioAdapter, "%Engine%", "%Project%");
            var configFileRef = new ConfigFileReference(ConfigDomain.Engine, null, configType);

            int openText_CallCount = 0;
            ioAdapter.OnOpenText = path =>
            {
                openText_CallCount++;
                throw new FileNotFoundException();
            };
            
            bool wasLoaded = provider.LoadOrCreateConfig(configFileRef, out var config);
            
            Assert.That(wasLoaded, Is.False);
            Assert.That(config, Is.Not.Null);
            Assert.That(config.Reference, Is.EqualTo(configFileRef));
            Assert.That(String.IsNullOrWhiteSpace(config.Name), Is.False);
            Assert.That(openText_CallCount, Is.EqualTo(1));
        }
        
        [Test]
        public void SaveConfig_When_TargetDoesNotExist()
        {
            const string configType = "MyConfig";
            var configFileRef = new ConfigFileReference(ConfigDomain.Engine, null, configType);
            var configIni = new ConfigIni(configType, configFileRef);
            var content = "[MySection]\nSection=Prop";
            configIni.Read(new StringReader(content));
            var provider = NewProvider();
            var ioAdapter = new MockConfigFileIOAdapter();
            provider.Setup(ioAdapter, "%Engine%", "%Project%");
            var outputFileRef = new ConfigFileReference(ConfigDomain.Engine, null, configType);

            int openWrite_CallCount = 0;
            int openRead_CallCount = 0;
            ioAdapter.OnOpenText = path =>
            {
                openRead_CallCount++;
                Assert.That(path, Is.EqualTo(provider.ResolveConfigFilePath(outputFileRef)));
                throw new FileNotFoundException();
            };
            ioAdapter.OnOpenWrite = path =>
            {
                openWrite_CallCount++;
                Assert.That(path, Is.EqualTo(provider.ResolveConfigFilePath(outputFileRef)));
                return StreamWriter.Null;
            };
        
            provider.SaveConfig(outputFileRef, configIni);
        
            Assert.That(openRead_CallCount, Is.EqualTo(1));
            Assert.That(openWrite_CallCount, Is.EqualTo(1));
        }

        [Test]
        public void SaveConfig_When_OriginalIsNewAndTargetDoesNotExist()
        {
            const string configType = "MyConfig";
            var configFileRef = new ConfigFileReference(ConfigDomain.Engine, null, configType);
            var configIni = new ConfigIni(configType, configFileRef);
            var provider = NewProvider();
            var ioAdapter = new MockConfigFileIOAdapter();
            provider.Setup(ioAdapter, "%Engine%", "%Project%");
            var outputFileRef = new ConfigFileReference(ConfigDomain.Engine, null, configType);

            int openWrite_CallCount = 0;
            int openRead_CallCount = 0;
            ioAdapter.OnOpenText = path =>
            {
                openRead_CallCount++;
                Assert.That(path, Is.EqualTo(provider.ResolveConfigFilePath(outputFileRef)));
                return null;
            };
            ioAdapter.OnOpenWrite = path =>
            {
                openWrite_CallCount++;
                Assert.That(path, Is.EqualTo(provider.ResolveConfigFilePath(outputFileRef)));
                return StreamWriter.Null;
            };
        
            provider.SaveConfig(outputFileRef, configIni);
        
            Assert.That(openRead_CallCount, Is.EqualTo(1));
            Assert.That(openWrite_CallCount, Is.EqualTo(0));
        }
        
        [Test]
        public void SaveConfig_When_TargetHasSameContent()
        {
            const string configType = "MyConfig";
            var configFileRef = new ConfigFileReference(ConfigDomain.Engine, null, configType);
            var configIni = new ConfigIni(configType, configFileRef);
            var content = "[MySection]\nSection=Prop";
            configIni.Read(new StringReader(content));
            var provider = NewProvider();
            var ioAdapter = new MockConfigFileIOAdapter();
            provider.Setup(ioAdapter, "%Engine%", "%Project%");
            var outputFileRef = new ConfigFileReference(ConfigDomain.Engine, null, configType);

            int openWrite_CallCount = 0;
            int openRead_CallCount = 0;
            ioAdapter.OnOpenText = path =>
            {
                openRead_CallCount++;
                Assert.That(path, Is.EqualTo(provider.ResolveConfigFilePath(outputFileRef)));
                return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            };
            ioAdapter.OnOpenWrite = path =>
            {
                openWrite_CallCount++;
                Assert.That(path, Is.EqualTo(provider.ResolveConfigFilePath(outputFileRef)));
                return StreamWriter.Null;
            };
        
            provider.SaveConfig(outputFileRef, configIni);
        
            Assert.That(openRead_CallCount, Is.EqualTo(1));
            Assert.That(openWrite_CallCount, Is.EqualTo(0));
        }
        
        [Test]
        public void SaveConfig_When_TargetHasDifferentContent()
        {
            const string configType = "MyConfig";
            var configFileRef = new ConfigFileReference(ConfigDomain.Engine, null, configType);
            var configIni = new ConfigIni(configType, configFileRef);
            var content = "[MySection]\nSection=Prop";
            configIni.Read(new StringReader(content));
            var provider = NewProvider();
            var ioAdapter = new MockConfigFileIOAdapter();
            provider.Setup(ioAdapter, "%Engine%", "%Project%");
            var outputFileRef = new ConfigFileReference(ConfigDomain.Engine, null, configType);

            int openWrite_CallCount = 0;
            int openRead_CallCount = 0;
            ioAdapter.OnOpenText = path =>
            {
                var differentContent = "[MySection]\nSection=Prop2";
                openRead_CallCount++;
                Assert.That(path, Is.EqualTo(provider.ResolveConfigFilePath(outputFileRef)));
                return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(differentContent)));
            };
            ioAdapter.OnOpenWrite = path =>
            {
                openWrite_CallCount++;
                Assert.That(path, Is.EqualTo(provider.ResolveConfigFilePath(outputFileRef)));
                return StreamWriter.Null;
            };
        
            provider.SaveConfig(outputFileRef, configIni);
        
            Assert.That(openRead_CallCount, Is.EqualTo(1));
            Assert.That(openWrite_CallCount, Is.EqualTo(1));
        }
    }
}