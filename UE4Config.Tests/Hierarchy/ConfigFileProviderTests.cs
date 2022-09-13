using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UE4Config.Hierarchy;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class ConfigFileProviderTests
    {
        protected static ConfigFileProvider NewProvider()
        {
            return new ConfigFileProvider();
        }

        private const string TestEnginePath = "%Engine%";
        private const string TestProjectPath = "%Project%";

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
                var provider = NewProvider();
                provider.Setup(TestEnginePath, TestProjectPath);

                var result =
                    provider.ResolveConfigFilePath(new ConfigFileReference(domain, new ConfigPlatform(platform), type));

                Assert.That(result, Is.EqualTo(expectedPath));
            }

            [TestCaseSource(nameof(Cases_ResolveConfigFilePath_WithValidReference))]
            [TestCaseSource(nameof(Cases_ResolveConfigFilePath_WithValidReference_LegacyPlatformConfig))]
            public void WithValidReference_LegacyPlatformConfigs(ConfigDomain domain,
                string platform, string type, string expectedPath)
            {
                var provider = NewProvider();
                provider.Setup(TestEnginePath, TestProjectPath);
                provider.SetPlatformLegacyConfig(platform, true);

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
                var provider = NewProvider();
                const string platform = "MyPlatform";

                Assert.That(provider.IsPlatformUsingLegacyConfig(platform), Is.False);
            }

            [Test]
            public void SetPlatformLegacyConfig_Enable()
            {
                var provider = NewProvider();
                const string platform = "MyPlatform";

                provider.SetPlatformLegacyConfig(platform, true);

                Assert.That(provider.IsPlatformUsingLegacyConfig(platform), Is.True);
            }

            [Test]
            public void SetPlatformLegacyConfig_EnableEnabled()
            {
                var provider = NewProvider();
                const string platform = "MyPlatform";

                provider.SetPlatformLegacyConfig(platform, true);
                Assert.That(() => { provider.SetPlatformLegacyConfig(platform, true); }, Throws.Nothing);

                Assert.That(provider.IsPlatformUsingLegacyConfig(platform), Is.True);
            }

            [Test]
            public void SetPlatformLegacyConfig_DisableEnabled()
            {
                var provider = NewProvider();
                const string platform = "MyPlatform";

                provider.SetPlatformLegacyConfig(platform, true);
                provider.SetPlatformLegacyConfig(platform, false);

                Assert.That(provider.IsPlatformUsingLegacyConfig(platform), Is.False);
            }

            [Test]
            public void SetPlatformLegacyConfig_DisableDisabled()
            {
                var provider = NewProvider();
                const string platform = "MyPlatform";

                provider.SetPlatformLegacyConfig(platform, true);
                provider.SetPlatformLegacyConfig(platform, false);
                Assert.That(() => { provider.SetPlatformLegacyConfig(platform, false); }, Throws.Nothing);

                Assert.That(provider.IsPlatformUsingLegacyConfig(platform), Is.False);
            }

            [Test]
            public void GetPlatformsUsingLegacyConfig()
            {
                var provider = NewProvider();
                var platformsEnablingLegacyConfig = new List<string>()
                {
                    "MyPlatform",
                    "MyPlatform2",
                    "foobar"
                };

                foreach (var platformId in platformsEnablingLegacyConfig)
                {
                    provider.SetPlatformLegacyConfig(platformId, true);
                }

                var platformsUsingLegacyConfig = provider.GetPlatformsUsingLegacyConfig().ToList();

                Assert.That(platformsUsingLegacyConfig, Is.EquivalentTo(platformsEnablingLegacyConfig));
            }

            [Test]
            public void GetPlatformsUsingLegacyConfig_WithRemoval()
            {
                var provider = NewProvider();
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
                    provider.SetPlatformLegacyConfig(platformId, true);
                }

                foreach (var platformId in platformsDisablingLegacyConfig)
                {
                    provider.SetPlatformLegacyConfig(platformId, false);
                    platformsEnablingLegacyConfig.Remove(platformId);
                }

                var platformsUsingLegacyConfig = provider.GetPlatformsUsingLegacyConfig().ToList();

                Assert.That(platformsUsingLegacyConfig, Is.EquivalentTo(platformsEnablingLegacyConfig));
            }

            [Test]
            public void ClearPlatformUsingLegacyConfig()
            {
                var provider = NewProvider();
                var platformsEnablingLegacyConfig = new List<string>()
                {
                    "MyPlatform",
                    "MyPlatform2",
                    "foobar"
                };

                foreach (var platformId in platformsEnablingLegacyConfig)
                {
                    provider.SetPlatformLegacyConfig(platformId, true);
                }

                provider.ClearPlatformUsingLegacyConfig();

                foreach (var platformId in platformsEnablingLegacyConfig)
                {
                    Assert.That(provider.IsPlatformUsingLegacyConfig(platformId), Is.False);
                }
                Assert.That(provider.GetPlatformsUsingLegacyConfig().ToList(), Is.Empty);
            }
        }
    }
}