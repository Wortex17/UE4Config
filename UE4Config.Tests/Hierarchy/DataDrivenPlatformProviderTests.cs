using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using UE4Config.Hierarchy;
using UE4Config.Parsing;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class DataDrivenPlatformProviderTests
    {
        class TestDataDrivenPlatformProvider : DataDrivenPlatformProvider
        {
            public Dictionary<string, DataDrivenPlatformInfo> Exposed_DataDrivenPlatformInfos =>
                m_DataDrivenPlatformInfos;
        }
        
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

        class TestConfigReferenceTree : IConfigReferenceTree
        {
            public IConfigFileProvider FileProvider { get; }
            public void Setup(IConfigFileProvider configFileProvider)
            {
                throw new NotImplementedException();
            }

            public void VisitConfigRoot(Action<ConfigFileReference> onConfig)
            {
                throw new NotImplementedException();
            }

            public void VisitConfigBranch(string configType, string platformIdentifier, Action<ConfigFileReference> onConfig)
            {
                throw new NotImplementedException();
            }

            public IConfigPlatform GetPlatform(string platformIdentifier)
            {
                if (RegisteredPlatforms.ContainsKey(platformIdentifier))
                {
                    return RegisteredPlatforms[platformIdentifier];
                }

                return null;
            }

            public IConfigPlatform RegisterPlatform(string platformIdentifier, IConfigPlatform parentPlatform = null)
            {
                var platform = new ConfigPlatform(platformIdentifier, parentPlatform);
                RegisteredPlatforms.Add(platformIdentifier, platform);
                return platform;
            }

            public Dictionary<string, IConfigPlatform> RegisteredPlatforms = new Dictionary<string, IConfigPlatform>();
        }
        
        [TestFixture]
        public class Setup
        {
            [Test]
            public void When_CalledWithValidFileProvider()
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
            public void When_CalledWithoutFileProvider()
            {
                var dataDrivenPlatformProvider = new DataDrivenPlatformProvider();
                
                Assert.That(() =>
                {
                    dataDrivenPlatformProvider.CollectDataDrivenPlatforms();
                }, Throws.Exception);
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos, Is.Empty);
            }
            
            [Test]
            public void When_ProvidedWithoutPlatform()
            {
                var platforms = new List<string>() { };
                int callCount_FindAllPlatformsCount = 0;
                var calls_LoadOrCreateDataDrivenPlatformConfig = new List<string>();
                var fileProvider = new MockConfigFileProvider
                {
                    OnFindAllPlatforms = () =>
                    {
                        callCount_FindAllPlatformsCount++;
                        return platforms;
                    },
                    OnLoadOrCreateDataDrivenPlatformConfig = (string identifier, out ConfigIni ini) =>
                    {
                        calls_LoadOrCreateDataDrivenPlatformConfig.Add(identifier);
                        ini = null;
                        return false;
                    }
                };
                var dataDrivenPlatformProvider = new DataDrivenPlatformProvider();
                dataDrivenPlatformProvider.Setup(fileProvider);
                
                Assert.That(() =>
                {
                    dataDrivenPlatformProvider.CollectDataDrivenPlatforms();
                }, Throws.Nothing);
                Assert.That(callCount_FindAllPlatformsCount, Is.EqualTo(1));
                Assert.That(calls_LoadOrCreateDataDrivenPlatformConfig, Is.EquivalentTo(platforms));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos, Is.Empty);
            }
            
            [Test]
            public void When_ProvidedWithPlatformWithoutConfig()
            {
                var platforms = new List<string>() { "PlatformA", "PlatformB"  };
                int callCount_FindAllPlatformsCount = 0;
                var calls_LoadOrCreateDataDrivenPlatformConfig = new List<string>();
                var fileProvider = new MockConfigFileProvider
                {
                    OnFindAllPlatforms = () =>
                    {
                        callCount_FindAllPlatformsCount++;
                        return platforms;
                    },
                    OnLoadOrCreateDataDrivenPlatformConfig = (string identifier, out ConfigIni ini) =>
                    {
                        calls_LoadOrCreateDataDrivenPlatformConfig.Add(identifier);
                        ini = null;
                        return false;
                    }
                };
                var dataDrivenPlatformProvider = new DataDrivenPlatformProvider();
                dataDrivenPlatformProvider.Setup(fileProvider);
                
                Assert.That(() =>
                {
                    dataDrivenPlatformProvider.CollectDataDrivenPlatforms();
                }, Throws.Nothing);
                Assert.That(callCount_FindAllPlatformsCount, Is.EqualTo(1));
                Assert.That(calls_LoadOrCreateDataDrivenPlatformConfig, Is.EquivalentTo(platforms));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos, Is.Empty);
            }
            
            [Test]
            public void When_ProvidedWithPlatformWithNullConfig()
            {
                Dictionary<string, ConfigIni> platformConfigs = new Dictionary<string, ConfigIni>()
                {
                    {"PlatformWithNullConfig", null}
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
                
                Assert.That(() =>
                {
                    dataDrivenPlatformProvider.CollectDataDrivenPlatforms();
                }, Throws.Nothing);
                Assert.That(callCount_FindAllPlatformsCount, Is.EqualTo(1));
                Assert.That(calls_LoadOrCreateDataDrivenPlatformConfig, Is.EquivalentTo(platformConfigs.Keys));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos, Is.Empty);
            }
            
            [Test]
            public void When_ProvidedWithPlatformWithEmptyConfig()
            {
                Dictionary<string, ConfigIni> platformConfigs = new Dictionary<string, ConfigIni>()
                {
                    {"PlatformWithEmptyConfig", new ConfigIni()},
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
                
                Assert.That(() =>
                {
                    dataDrivenPlatformProvider.CollectDataDrivenPlatforms();
                }, Throws.Nothing);
                Assert.That(callCount_FindAllPlatformsCount, Is.EqualTo(1));
                Assert.That(calls_LoadOrCreateDataDrivenPlatformConfig, Is.EquivalentTo(platformConfigs.Keys));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos, Is.Empty);
            }
            
            [Test]
            public void When_ProvidedWithPlatformWithConfigWithoutInfos()
            {
                Dictionary<string, ConfigIni> platformConfigs = new Dictionary<string, ConfigIni>()
                {
                    {"PlatformWithoutInfos", new ConfigIni(){Sections = new List<ConfigIniSection>(){
                        new ConfigIniSection("PlatformInfo "),
                        new ConfigIniSection("PlatformInfo"),
                        new ConfigIniSection("PlatformInf"),
                        new ConfigIniSection("foobar"),
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
                
                Assert.That(() =>
                {
                    dataDrivenPlatformProvider.CollectDataDrivenPlatforms();
                }, Throws.Nothing);
                Assert.That(callCount_FindAllPlatformsCount, Is.EqualTo(1));
                Assert.That(calls_LoadOrCreateDataDrivenPlatformConfig, Is.EquivalentTo(platformConfigs.Keys));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos, Is.Empty);
            }
            
            [Test]
            public void When_ProvidedWithPlatformWithConfigWithInfo()
            {
                Dictionary<string, ConfigIni> platformConfigs = new Dictionary<string, ConfigIni>()
                {
                    {"PlatformA", new ConfigIni(){Sections = new List<ConfigIniSection>(){
                        new ConfigIniSection("PlatformInfo PlatformA")
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
                
                Assert.That(() =>
                {
                    dataDrivenPlatformProvider.CollectDataDrivenPlatforms();
                }, Throws.Nothing);
                Assert.That(callCount_FindAllPlatformsCount, Is.EqualTo(1));
                Assert.That(calls_LoadOrCreateDataDrivenPlatformConfig, Is.EquivalentTo(platformConfigs.Keys));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos, Has.Count.EqualTo(1));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos, Contains.Key("PlatformA"));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos["PlatformA"].PlatformIdentifier, Is.EqualTo("PlatformA"));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos["PlatformA"].ParentPlatformIdentifier, Is.Null);
            }
            
            [Test]
            public void When_ProvidedWithPlatformWithConfigWithInfos()
            {
                Dictionary<string, ConfigIni> platformConfigs = new Dictionary<string, ConfigIni>()
                {
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
                
                Assert.That(() =>
                {
                    dataDrivenPlatformProvider.CollectDataDrivenPlatforms();
                }, Throws.Nothing);
                Assert.That(callCount_FindAllPlatformsCount, Is.EqualTo(1));
                Assert.That(calls_LoadOrCreateDataDrivenPlatformConfig, Is.EquivalentTo(platformConfigs.Keys));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos, Has.Count.EqualTo(3));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos, Contains.Key("PlatB"));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos["PlatB"].PlatformIdentifier, Is.EqualTo("PlatB"));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos["PlatB"].ParentPlatformIdentifier, Is.Null);
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos, Contains.Key("PlatB1"));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos["PlatB1"].PlatformIdentifier, Is.EqualTo("PlatB1"));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos["PlatB1"].ParentPlatformIdentifier, Is.EqualTo("PlatB"));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos, Contains.Key("PlatformB2"));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos["PlatformB2"].PlatformIdentifier, Is.EqualTo("PlatformB2"));
                Assert.That(dataDrivenPlatformProvider.DataDrivenPlatformInfos["PlatformB2"].ParentPlatformIdentifier, Is.EqualTo("PlatB"));
            }
        }

        [TestFixture]
        public class RegisterDataDrivenPlatforms
        {
            [Test]
            public void When_NoPlatform()
            {
                var dataDrivenPlatformProvider = new TestDataDrivenPlatformProvider();
                var configReferenceTree = new TestConfigReferenceTree();
                
                dataDrivenPlatformProvider.RegisterDataDrivenPlatforms(configReferenceTree);
                
                Assert.That(configReferenceTree.RegisteredPlatforms, Has.Count.EqualTo(0));
            }
            
            [Test]
            public void When_SinglePlatform()
            {
                var dataDrivenPlatformProvider = new TestDataDrivenPlatformProvider();
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA"
                });
                var configReferenceTree = new TestConfigReferenceTree();
                
                dataDrivenPlatformProvider.RegisterDataDrivenPlatforms(configReferenceTree);
                
                Assert.That(configReferenceTree.RegisteredPlatforms, Has.Count.EqualTo(1));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].Identifier, Is.EqualTo("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].ParentPlatform, Is.Null);
            }
            
            [Test]
            public void When_MultiplePlatforms()
            {
                var dataDrivenPlatformProvider = new TestDataDrivenPlatformProvider();
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA"
                });
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformB", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformB"
                });
                var configReferenceTree = new TestConfigReferenceTree();
                
                dataDrivenPlatformProvider.RegisterDataDrivenPlatforms(configReferenceTree);
                
                Assert.That(configReferenceTree.RegisteredPlatforms, Has.Count.EqualTo(2));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].Identifier, Is.EqualTo("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].ParentPlatform, Is.Null);
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformB"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformB"].Identifier, Is.EqualTo("PlatformB"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformB"].ParentPlatform, Is.Null);
            }
            
            [Test]
            public void When_PlatformsWithInheritance()
            {
                var dataDrivenPlatformProvider = new TestDataDrivenPlatformProvider();
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA"
                });
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA2", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA2",
                    ParentPlatformIdentifier = "PlatformA"
                });
                var configReferenceTree = new TestConfigReferenceTree();
                
                dataDrivenPlatformProvider.RegisterDataDrivenPlatforms(configReferenceTree);
                
                Assert.That(configReferenceTree.RegisteredPlatforms, Has.Count.EqualTo(2));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].Identifier, Is.EqualTo("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].ParentPlatform, Is.Null);
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA2"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA2"].Identifier, Is.EqualTo("PlatformA2"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA2"].ParentPlatform, Is.SameAs(configReferenceTree.RegisteredPlatforms["PlatformA"]));
            }
            
            [Test]
            public void When_PlatformsWithInheritanceInReverseOrder()
            {
                var dataDrivenPlatformProvider = new TestDataDrivenPlatformProvider();
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA2", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA2"
                });
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA",
                    ParentPlatformIdentifier = "PlatformA2"
                });
                var configReferenceTree = new TestConfigReferenceTree();
                
                dataDrivenPlatformProvider.RegisterDataDrivenPlatforms(configReferenceTree);
                
                Assert.That(configReferenceTree.RegisteredPlatforms, Has.Count.EqualTo(2));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].Identifier, Is.EqualTo("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].ParentPlatform, Is.SameAs(configReferenceTree.RegisteredPlatforms["PlatformA2"]));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA2"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA2"].Identifier, Is.EqualTo("PlatformA2"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA2"].ParentPlatform, Is.Null);
            }
            
            [Test]
            public void When_PlatformsWithLongInheritance()
            {
                var dataDrivenPlatformProvider = new TestDataDrivenPlatformProvider();
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA"
                });
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA2", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA2",
                    ParentPlatformIdentifier = "PlatformA"
                });
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA3", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA3",
                    ParentPlatformIdentifier = "PlatformA2"
                });
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA4", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA4",
                    ParentPlatformIdentifier = "PlatformA3"
                });
                var configReferenceTree = new TestConfigReferenceTree();
                
                dataDrivenPlatformProvider.RegisterDataDrivenPlatforms(configReferenceTree);
                
                Assert.That(configReferenceTree.RegisteredPlatforms, Has.Count.EqualTo(4));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].Identifier, Is.EqualTo("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].ParentPlatform, Is.Null);
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA2"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA2"].Identifier, Is.EqualTo("PlatformA2"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA2"].ParentPlatform, Is.SameAs(configReferenceTree.RegisteredPlatforms["PlatformA"]));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA3"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA3"].Identifier, Is.EqualTo("PlatformA3"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA3"].ParentPlatform, Is.SameAs(configReferenceTree.RegisteredPlatforms["PlatformA2"]));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA4"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA4"].Identifier, Is.EqualTo("PlatformA4"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA4"].ParentPlatform, Is.SameAs(configReferenceTree.RegisteredPlatforms["PlatformA3"]));
            }
            
            [Test]
            public void When_PlatformsWithLongInheritanceInReverseOrder()
            {
                var dataDrivenPlatformProvider = new TestDataDrivenPlatformProvider();
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA4", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA4"
                });
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA3", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA3",
                    ParentPlatformIdentifier = "PlatformA4"
                });
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA2", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA2",
                    ParentPlatformIdentifier = "PlatformA3"
                });
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA",
                    ParentPlatformIdentifier = "PlatformA2"
                });
                var configReferenceTree = new TestConfigReferenceTree();
                
                dataDrivenPlatformProvider.RegisterDataDrivenPlatforms(configReferenceTree);
                
                Assert.That(configReferenceTree.RegisteredPlatforms, Has.Count.EqualTo(4));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].Identifier, Is.EqualTo("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].ParentPlatform, Is.SameAs(configReferenceTree.RegisteredPlatforms["PlatformA2"]));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA2"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA2"].Identifier, Is.EqualTo("PlatformA2"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA2"].ParentPlatform, Is.SameAs(configReferenceTree.RegisteredPlatforms["PlatformA3"]));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA3"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA3"].Identifier, Is.EqualTo("PlatformA3"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA3"].ParentPlatform, Is.SameAs(configReferenceTree.RegisteredPlatforms["PlatformA4"]));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA4"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA4"].Identifier, Is.EqualTo("PlatformA4"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA4"].ParentPlatform, Is.Null);
            }
            
            [Test]
            public void When_PlatformWithUnresolvedInheritance()
            {
                var dataDrivenPlatformProvider = new TestDataDrivenPlatformProvider();
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA2", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA2",
                    ParentPlatformIdentifier = "PlatformA"
                });
                var configReferenceTree = new TestConfigReferenceTree();
                
                dataDrivenPlatformProvider.RegisterDataDrivenPlatforms(configReferenceTree);
                
                Assert.That(configReferenceTree.RegisteredPlatforms, Has.Count.EqualTo(2));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].Identifier, Is.EqualTo("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].ParentPlatform, Is.Null);
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA2"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA2"].Identifier, Is.EqualTo("PlatformA2"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA2"].ParentPlatform, Is.SameAs(configReferenceTree.RegisteredPlatforms["PlatformA"]));
            }
            
            [Test]
            public void When_PlatformWithSelfInheritance()
            {
                var dataDrivenPlatformProvider = new TestDataDrivenPlatformProvider();
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA",
                    ParentPlatformIdentifier = "PlatformA"
                });
                var configReferenceTree = new TestConfigReferenceTree();
                
                dataDrivenPlatformProvider.RegisterDataDrivenPlatforms(configReferenceTree);
                
                Assert.That(configReferenceTree.RegisteredPlatforms, Has.Count.EqualTo(1));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].Identifier, Is.EqualTo("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].ParentPlatform, Is.Null);
            }
            
            [Test]
            public void When_PlatformWithCircularInheritance()
            {
                var dataDrivenPlatformProvider = new TestDataDrivenPlatformProvider();
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA",
                    ParentPlatformIdentifier = "PlatformA3"
                });
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA2", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA2",
                    ParentPlatformIdentifier = "PlatformA"
                });
                dataDrivenPlatformProvider.Exposed_DataDrivenPlatformInfos.Add("PlatformA3", new DataDrivenPlatformInfo()
                {
                    PlatformIdentifier = "PlatformA3",
                    ParentPlatformIdentifier = "PlatformA2"
                });
                var configReferenceTree = new TestConfigReferenceTree();
                
                dataDrivenPlatformProvider.RegisterDataDrivenPlatforms(configReferenceTree);
                
                Assert.That(configReferenceTree.RegisteredPlatforms, Has.Count.EqualTo(3));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA"].Identifier, Is.EqualTo("PlatformA"));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA2"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA2"].Identifier, Is.EqualTo("PlatformA2"));
                Assert.That(configReferenceTree.RegisteredPlatforms, Contains.Key("PlatformA3"));
                Assert.That(configReferenceTree.RegisteredPlatforms["PlatformA3"].Identifier, Is.EqualTo("PlatformA3"));

                var parentA = configReferenceTree.RegisteredPlatforms["PlatformA"].ParentPlatform;
                var parentA2 = configReferenceTree.RegisteredPlatforms["PlatformA2"].ParentPlatform;
                var parentA3 = configReferenceTree.RegisteredPlatforms["PlatformA3"].ParentPlatform;
                Assert.That(parentA, Is.Not.SameAs(parentA2));
                Assert.That(parentA2, Is.Not.SameAs(parentA3));
                Assert.That(parentA3, Is.Not.SameAs(parentA));
                Assert.That(parentA == null || parentA2 == null || parentA3 == null);
            }
        }
    }
}