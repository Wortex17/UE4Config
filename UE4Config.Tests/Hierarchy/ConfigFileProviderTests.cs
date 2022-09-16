using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UE4Config.Hierarchy;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class ConfigFileProviderTests
    {
        private const string TestEnginePath = "%Engine%";
        private const string TestProjectPath = "%Project%";
        
        protected static ConfigFileProvider NewProvider()
        {
            return new ConfigFileProvider();
        }
        
        protected static ConfigFileProvider NewProviderWithDefaultSetup()
        {
            var provider = new ConfigFileProvider();
            provider.Setup(new ConfigFileIOAdapter(), TestEnginePath, TestProjectPath);
            return provider;
        }

        [TestFixture]
        public class ResolveConfigFilePath
        {
            public static IEnumerable Cases_ResolveConfigFilePath_WithValidReference
            {
                get
                {
                    const string Type = "MyConfig";
                    yield return new TestCaseData(new object[]
                    {
                        ConfigDomain.EngineBase, null, null,
                        Path.Combine(TestEnginePath, "Config", "Base.ini")
                    });
                    yield return new TestCaseData(new object[]
                    {
                        ConfigDomain.EngineBase, null, Type,
                        Path.Combine(TestEnginePath, "Config", $"Base{Type}.ini")
                    });
                    yield return new TestCaseData(new object[]
                    {
                        ConfigDomain.Project, null, Type,
                        Path.Combine(TestProjectPath, "Config", $"Default{Type}.ini")
                    });
                    yield return new TestCaseData(new object[]
                    {
                        ConfigDomain.ProjectGenerated, null, Type,
                        Path.Combine(TestProjectPath, "Config", $"Generated{Type}.ini")
                    });
                }
            }

            public static IEnumerable Cases_ResolveConfigFilePath_WithValidReference_PlatformSwitch(bool isLegacy)
            {
                const string Platform = "MyPlatform";
                const string Type = "MyConfig";
                string PlatformSubDir = isLegacy
                    ? Path.Combine("Config", Platform)
                    : Path.Combine("Platforms", Platform, "Config");

                yield return new TestCaseData(new object[]
                {
                    ConfigDomain.EngineBase, Platform, Type,
                    Path.Combine(TestEnginePath, PlatformSubDir, $"Base{Platform}{Type}.ini")
                });
                yield return new TestCaseData(new object[]
                {
                    ConfigDomain.Engine, Platform, Type,
                    Path.Combine(TestEnginePath, PlatformSubDir, $"{Platform}{Type}.ini")
                });
                yield return new TestCaseData(new object[]
                {
                    ConfigDomain.Project, Platform, Type,
                    Path.Combine(TestProjectPath, PlatformSubDir, $"{Platform}{Type}.ini")
                });
                yield return new TestCaseData(new object[]
                {
                    ConfigDomain.ProjectGenerated, Platform, Type,
                    Path.Combine(TestProjectPath, PlatformSubDir, $"Generated{Platform}{Type}.ini")
                });
            }

            public static IEnumerable Cases_ResolveConfigFilePath_WithValidReference_ModernPlatformConfig =>
                Cases_ResolveConfigFilePath_WithValidReference_PlatformSwitch(false);

            public static IEnumerable Cases_ResolveConfigFilePath_WithValidReference_LegacyPlatformConfig =>
                Cases_ResolveConfigFilePath_WithValidReference_PlatformSwitch(true);

            [TestCaseSource(nameof(Cases_ResolveConfigFilePath_WithValidReference))]
            [TestCaseSource(nameof(Cases_ResolveConfigFilePath_WithValidReference_ModernPlatformConfig))]
            public void WithValidReference(ConfigDomain domain, string platform, string type,
                string expectedPath)
            {
                var provider = NewProviderWithDefaultSetup();

                var result =
                    provider.ResolveConfigFilePath(new ConfigFileReference(domain, new ConfigPlatform(platform), type));

                Assert.That(result, Is.EqualTo(expectedPath));
            }

            [TestCaseSource(nameof(Cases_ResolveConfigFilePath_WithValidReference))]
            [TestCaseSource(nameof(Cases_ResolveConfigFilePath_WithValidReference_LegacyPlatformConfig))]
            public void WithValidReference_LegacyPlatformConfigs(ConfigDomain domain,
                string platform, string type, string expectedPath)
            {
                var provider = NewProviderWithDefaultSetup();
                provider.EnginePlatformLegacyConfig.SetPlatformLegacyConfig(platform, true);
                provider.ProjectPlatformLegacyConfig.SetPlatformLegacyConfig(platform, true);

                var result =
                    provider.ResolveConfigFilePath(new ConfigFileReference(domain, new ConfigPlatform(platform), type));

                Assert.That(result, Is.EqualTo(expectedPath));
            }
        }

        [TestFixture]
        public class PlatformLegacyConfig
        {
            [Test]
            public void IsPlatformUsingLegacyConfig_Default()
            {
                var legacyConfig = new ConfigFileProvider.PlatformLegacyConfig();
                const string platform = "MyPlatform";

                Assert.That(legacyConfig.IsPlatformUsingLegacyConfig(platform), Is.False);
            }

            [Test]
            public void SetPlatformLegacyConfig_Enable()
            {
                var legacyConfig = new ConfigFileProvider.PlatformLegacyConfig();
                const string platform = "MyPlatform";

                legacyConfig.SetPlatformLegacyConfig(platform, true);

                Assert.That(legacyConfig.IsPlatformUsingLegacyConfig(platform), Is.True);
            }

            [Test]
            public void SetPlatformLegacyConfig_EnableEnabled()
            {
                var legacyConfig = new ConfigFileProvider.PlatformLegacyConfig();
                const string platform = "MyPlatform";

                legacyConfig.SetPlatformLegacyConfig(platform, true);
                Assert.That(() => { legacyConfig.SetPlatformLegacyConfig(platform, true); }, Throws.Nothing);

                Assert.That(legacyConfig.IsPlatformUsingLegacyConfig(platform), Is.True);
            }

            [Test]
            public void SetPlatformLegacyConfig_DisableEnabled()
            {
                var legacyConfig = new ConfigFileProvider.PlatformLegacyConfig();
                const string platform = "MyPlatform";

                legacyConfig.SetPlatformLegacyConfig(platform, true);
                legacyConfig.SetPlatformLegacyConfig(platform, false);

                Assert.That(legacyConfig.IsPlatformUsingLegacyConfig(platform), Is.False);
            }

            [Test]
            public void SetPlatformLegacyConfig_DisableDisabled()
            {
                var legacyConfig = new ConfigFileProvider.PlatformLegacyConfig();
                const string platform = "MyPlatform";

                legacyConfig.SetPlatformLegacyConfig(platform, true);
                legacyConfig.SetPlatformLegacyConfig(platform, false);
                Assert.That(() => { legacyConfig.SetPlatformLegacyConfig(platform, false); }, Throws.Nothing);

                Assert.That(legacyConfig.IsPlatformUsingLegacyConfig(platform), Is.False);
            }

            [Test]
            public void SetPlatformLegacyConfig_Null()
            {
                var legacyConfig = new ConfigFileProvider.PlatformLegacyConfig();

                legacyConfig.SetPlatformLegacyConfig(null, true);

                Assert.That(legacyConfig.IsPlatformUsingLegacyConfig(null), Is.False);
            }

            [Test]
            public void GetPlatformsUsingLegacyConfig()
            {
                var legacyConfig = new ConfigFileProvider.PlatformLegacyConfig();
                var platformsEnablingLegacyConfig = new List<string>()
                {
                    "MyPlatform",
                    "MyPlatform2",
                    "foobar"
                };

                foreach (var platformId in platformsEnablingLegacyConfig)
                {
                    legacyConfig.SetPlatformLegacyConfig(platformId, true);
                }

                var platformsUsingLegacyConfig = legacyConfig.GetPlatformsUsingLegacyConfig().ToList();

                Assert.That(platformsUsingLegacyConfig, Is.EquivalentTo(platformsEnablingLegacyConfig));
            }

            [Test]
            public void GetPlatformsUsingLegacyConfig_WithRemoval()
            {
                var legacyConfig = new ConfigFileProvider.PlatformLegacyConfig();
                var platformsEnablingLegacyConfig = new List<string>()
                {
                    "MyPlatform",
                    "MyPlatform2",
                    "foobar",
                    "moo"
                };
                var platformsDisablingLegacyConfig = new List<string>()
                {
                    "MyPlatform",
                    "foobar"
                };

                foreach (var platformId in platformsEnablingLegacyConfig)
                {
                    legacyConfig.SetPlatformLegacyConfig(platformId, true);
                }

                foreach (var platformId in platformsDisablingLegacyConfig)
                {
                    legacyConfig.SetPlatformLegacyConfig(platformId, false);
                    platformsEnablingLegacyConfig.Remove(platformId);
                }

                var platformsUsingLegacyConfig = legacyConfig.GetPlatformsUsingLegacyConfig().ToList();

                Assert.That(platformsUsingLegacyConfig, Is.EquivalentTo(platformsEnablingLegacyConfig));
            }

            [Test]
            public void ClearPlatformUsingLegacyConfig()
            {
                var legacyConfig = new ConfigFileProvider.PlatformLegacyConfig();
                var platformsEnablingLegacyConfig = new List<string>()
                {
                    "MyPlatform",
                    "MyPlatform2",
                    "foobar"
                };

                foreach (var platformId in platformsEnablingLegacyConfig)
                {
                    legacyConfig.SetPlatformLegacyConfig(platformId, true);
                }

                legacyConfig.ClearPlatformUsingLegacyConfig();

                foreach (var platformId in platformsEnablingLegacyConfig)
                {
                    Assert.That(legacyConfig.IsPlatformUsingLegacyConfig(platformId), Is.False);
                }
                Assert.That(legacyConfig.GetPlatformsUsingLegacyConfig().ToList(), Is.Empty);
            }
        }

        [TestFixture]
        public class AutoDetectPlatformsUsingLegacyConfig
        {
            class MockConfigFileIOAdapter : IConfigFileIOAdapter
            {
                public delegate List<string> GetDirectoriesDelegate(string pivotPath);

                public GetDirectoriesDelegate OnGetDirectories;
                
                public List<string> GetDirectories(string pivotPath)
                {
                    return OnGetDirectories(pivotPath);
                }

                public StreamReader OpenText(string filePath)
                {
                    return StreamReader.Null;
                }

                public StreamWriter OpenWrite(string filePath)
                {
                    return StreamWriter.Null;
                }
            }

            [Test]
            public void When_NoIOAdapter()
            {
                var provider = NewProvider();
                provider.Setup(null, TestEnginePath, TestProjectPath);
                
                Assert.That(() =>
                {
                    provider.AutoDetectPlatformsUsingLegacyConfig();
                }, Throws.Nothing);
            }

            [Test]
            public void When_NoSetup()
            {
                var provider = NewProvider();
                
                Assert.That(() =>
                {
                    provider.AutoDetectPlatformsUsingLegacyConfig();
                }, Throws.Nothing);
            }

            [Test]
            public void When_SetupComplete()
            {
                var provider = NewProvider();
                var fileIOAdapter = new MockConfigFileIOAdapter();
                provider.Setup(fileIOAdapter, TestEnginePath, TestProjectPath);
                var onGetDirectoriesCalls = new List<string>();
                var engineLegacyConfigDirs = new List<string>()
                {
                    Path.Combine(TestEnginePath, "Config", "LegacyPlatformA"),
                    Path.Combine(TestEnginePath, "Config", "LegacyPlatformB")
                };
                var engineModernConfigDirs = new List<string>()
                {
                    Path.Combine(TestEnginePath, "Platforms", "PlatformD"),
                    Path.Combine(TestEnginePath, "Platforms", "PlatformE")
                };
                var projectLegacyConfigDirs = new List<string>()
                {
                    Path.Combine(TestProjectPath, "Config", "LegacyPlatformA"),
                    Path.Combine(TestProjectPath, "Config", "LegacyPlatformU")
                };
                var projectModernConfigDirs = new List<string>()
                {
                    Path.Combine(TestProjectPath, "Platforms", "PlatformD"),
                    Path.Combine(TestProjectPath, "Platforms", "PlatformW")
                };
                fileIOAdapter.OnGetDirectories = path =>
                {
                    onGetDirectoriesCalls.Add(path);
                    Assert.That(path, Does.StartWith(TestEnginePath).Or.StartsWith(TestProjectPath));
                    
                    bool isEngine = path.StartsWith(TestEnginePath);
                    bool isLegacy = path.EndsWith("Config");
                    if (isEngine)
                    {
                        if (isLegacy)
                        {
                            return engineLegacyConfigDirs;
                        }
                        else
                        {
                            return engineModernConfigDirs;
                        }
                    }
                    else
                    {
                        if (isLegacy)
                        {
                            return projectLegacyConfigDirs;
                        }
                        else
                        {
                            return projectModernConfigDirs;
                        }
                    }
                };
                var expectedGetDirCalls = new List<string>()
                {
                    Path.Combine(TestEnginePath, "Config"),
                    Path.Combine(TestEnginePath, "Platforms"),
                    Path.Combine(TestProjectPath, "Config"),
                    Path.Combine(TestProjectPath, "Platforms"),
                };
                
                provider.AutoDetectPlatformsUsingLegacyConfig();
                
                Assert.That(onGetDirectoriesCalls, Is.EquivalentTo(expectedGetDirCalls));
                foreach (var engineLegacyConfigDir in engineLegacyConfigDirs)
                {
                    var platformIdentifier = Path.GetFileName(engineLegacyConfigDir);
                    Assert.That(provider.EnginePlatformLegacyConfig.IsPlatformUsingLegacyConfig(
                            platformIdentifier), Is.True,
                        $"{platformIdentifier} IsPlatformUsingLegacyConfig=true ");
                }
                foreach (var engineModernConfigDir in engineModernConfigDirs)
                {
                    var platformIdentifier = Path.GetFileName(engineModernConfigDir);
                    Assert.That(provider.EnginePlatformLegacyConfig.IsPlatformUsingLegacyConfig(
                            platformIdentifier), Is.False,
                        $"{platformIdentifier} IsPlatformUsingLegacyConfig=false ");
                }
                foreach (var projectLegacyConfigDir in projectLegacyConfigDirs)
                {
                    var platformIdentifier = Path.GetFileName(projectLegacyConfigDir);
                    Assert.That(provider.ProjectPlatformLegacyConfig.IsPlatformUsingLegacyConfig(
                            platformIdentifier), Is.True,
                        $"{platformIdentifier} IsPlatformUsingLegacyConfig=true ");
                }
                foreach (var projectModernConfigDir in projectModernConfigDirs)
                {
                    var platformIdentifier = Path.GetFileName(projectModernConfigDir);
                    Assert.That(provider.ProjectPlatformLegacyConfig.IsPlatformUsingLegacyConfig(
                            platformIdentifier), Is.False,
                        $"{platformIdentifier} IsPlatformUsingLegacyConfig=false ");
                }
            }
            
            [Test]
            public void When_OverlappingResults()
            {
                var provider = NewProvider();
                const string platformName = "LegacyAndModernPlatform";
                var fileIOAdapter = new MockConfigFileIOAdapter();
                provider.Setup(fileIOAdapter, TestEnginePath, TestProjectPath);
                var onGetDirectoriesCalls = new List<string>();
                fileIOAdapter.OnGetDirectories = path =>
                {
                    onGetDirectoriesCalls.Add(path);
                    Assert.That(path, Does.StartWith(TestEnginePath).Or.StartsWith(TestProjectPath));

                    return new List<string>()
                    {
                        Path.Combine(path,platformName)
                    };
                };
                var expectedGetDirCalls = new List<string>()
                {
                    Path.Combine(TestEnginePath, "Config"),
                    Path.Combine(TestEnginePath, "Platforms"),
                    Path.Combine(TestProjectPath, "Config"),
                    Path.Combine(TestProjectPath, "Platforms"),
                };
                
                provider.AutoDetectPlatformsUsingLegacyConfig();
                
                Assert.That(onGetDirectoriesCalls, Is.EquivalentTo(expectedGetDirCalls));
                Assert.That(provider.EnginePlatformLegacyConfig.IsPlatformUsingLegacyConfig(
                        platformName), Is.False,
                    $"{platformName} IsPlatformUsingLegacyConfig=false ");
                Assert.That(provider.ProjectPlatformLegacyConfig.IsPlatformUsingLegacyConfig(
                        platformName), Is.False,
                    $"{platformName} IsPlatformUsingLegacyConfig=false ");
            }
            
            [Test]
            public void When_MissingEnginePath()
            {
                var provider = NewProvider();
                const string platformName = "LegacyAndModernPlatform";
                var fileIOAdapter = new MockConfigFileIOAdapter();
                provider.Setup(fileIOAdapter, null, TestProjectPath);
                var onGetDirectoriesCalls = new List<string>();
                fileIOAdapter.OnGetDirectories = path =>
                {
                    onGetDirectoriesCalls.Add(path);
                    return new List<string>()
                    {
                        Path.Combine(path,platformName)
                    };
                };
                var expectedGetDirCalls = new List<string>()
                {
                    Path.Combine(TestProjectPath, "Config"),
                    Path.Combine(TestProjectPath, "Platforms"),
                };
                
                provider.AutoDetectPlatformsUsingLegacyConfig();
                
                Assert.That(onGetDirectoriesCalls, Is.EquivalentTo(expectedGetDirCalls));
            }
            
            [Test]
            public void When_MissingProjectPath()
            {
                var provider = NewProvider();
                const string platformName = "LegacyAndModernPlatform";
                var fileIOAdapter = new MockConfigFileIOAdapter();
                provider.Setup(fileIOAdapter, TestEnginePath, null);
                var onGetDirectoriesCalls = new List<string>();
                fileIOAdapter.OnGetDirectories = path =>
                {
                    onGetDirectoriesCalls.Add(path);
                    return new List<string>()
                    {
                        Path.Combine(path,platformName)
                    };
                };
                var expectedGetDirCalls = new List<string>()
                {
                    Path.Combine(TestEnginePath, "Config"),
                    Path.Combine(TestEnginePath, "Platforms"),
                };
                
                provider.AutoDetectPlatformsUsingLegacyConfig();
                
                Assert.That(onGetDirectoriesCalls, Is.EquivalentTo(expectedGetDirCalls));
            }
        }
    }
}