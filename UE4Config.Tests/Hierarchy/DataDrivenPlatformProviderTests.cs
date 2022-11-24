using System.Collections.Generic;
using NUnit.Framework;
using UE4Config.Hierarchy;
using UE4Config.Parsing;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class DataDrivenPlatformProviderTests
    {
        class MockConfigFileProvider : IConfigFileProviderAutoPlatformModel
        {
            public delegate bool OnLoadOrCreateDataDrivenPlatformConfigDelegate(string platformIdentifier,
                out ConfigIni configIni);
            public OnLoadOrCreateDataDrivenPlatformConfigDelegate OnLoadOrCreateDataDrivenPlatformConfig;
            
            public delegate List<string> OnFindAllPlatformsDelegate();
            public OnFindAllPlatformsDelegate OnFindAllPlatforms;
            
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
                throw new System.NotImplementedException();
            }

            public bool LoadOrCreateDataDrivenPlatformConfig(string platformIdentifier, out ConfigIni configIni)
            {
                return OnLoadOrCreateDataDrivenPlatformConfig.Invoke(platformIdentifier, out configIni);
            }

            public void SaveConfig(ConfigFileReference configFileReference, ConfigIni configIni)
            {
                throw new System.NotImplementedException();
            }

            public List<string> FindAllPlatforms()
            {
                return OnFindAllPlatforms();
            }

            public void AutoDetectPlatformsUsingLegacyConfig()
            {
                throw new System.NotImplementedException();
            }
        }
        
        [TestFixture]
        public class Setup
        {
            [Test]
            void When_CalledWithValidFileProvider()
            {
                var fileProvider = new MockConfigFileProvider();
                var dataDrivenPlatformProvider = new DataDrivenPlatformProvider();
                
                dataDrivenPlatformProvider.Setup(fileProvider);
                
                Assert.That(dataDrivenPlatformProvider.FileProvider, Is.SameAs(fileProvider));
            }
        }
        
        [TestFixture]
        public class CollectDataDrivenPlatforms
        {
            [Test]
            void When_CalledAfterSetupWithFileProvider()
            {
                Dictionary<string, ConfigIni> platformConfigs = new Dictionary<string, ConfigIni>()
                {
                    {"PlatformMissingDataDrivenConfig", null},
                    {"PlatformWithEmptyDrivenConfig", new ConfigIni()},
                    {"PlatformWithInvalidDrivenConfig", new ConfigIni(){Sections = new List<ConfigIniSection>(){
                        new ConfigIniSection("PlatformInfo "),
                        new ConfigIniSection("PlatformInfo"),
                        new ConfigIniSection("PlatformInf"),
                        new ConfigIniSection("foobar"),
                    }}},
                    {"PlatformA", new ConfigIni(){Sections = new List<ConfigIniSection>(){
                        new ConfigIniSection("PlatformInfo PlatformA")
                    }}},
                    {"PlatformB", new ConfigIni(){Sections = new List<ConfigIniSection>(){
                        new ConfigIniSection("PlatformInfo PlatformB", new []{
                            new InstructionToken(InstructionType.Set, "TargetPlatformName", "PlatB"),
                        }),
                        new ConfigIniSection("PlatformInfo PlatformB1", new []{
                            new InstructionToken(InstructionType.Set, "TargetPlatformName", "PlatB1"),
                            new InstructionToken(InstructionType.Set, "IniPlatformName", "PlatB"),
                        }),
                        new ConfigIniSection("PlatformInfo PlatformB2", new []{
                            new InstructionToken(InstructionType.Set, "IniPlatformName", "PlatB"),
                        })
                    }}},
                };
                int callCount_FindAllPlatformsCount = 0;
                var calls_LoadOrCreateDataDrivenPlatformConfig = new List<string>();
                var fileProvider = new MockConfigFileProvider
                {
                    OnFindAllPlatforms = () =>
                    {
                        callCount_FindAllPlatformsCount++;
                        return new List<string>(platformConfigs.Keys);
                    },
                    
                    OnLoadOrCreateDataDrivenPlatformConfig = (string identifier, out ConfigIni ini) =>
                    {
                        calls_LoadOrCreateDataDrivenPlatformConfig.Add(identifier);
                        return platformConfigs.TryGetValue(identifier, out ini);
                    }
                };
                var dataDrivenPlatformProvider = new DataDrivenPlatformProvider();
                dataDrivenPlatformProvider.Setup(fileProvider);
                
                dataDrivenPlatformProvider.CollectDataDrivenPlatforms();
                
                Assert.That(callCount_FindAllPlatformsCount, Is.EqualTo(1));
                Assert.That(calls_LoadOrCreateDataDrivenPlatformConfig, Is.EquivalentTo(platformConfigs.Keys));
            }
        }
    }
}