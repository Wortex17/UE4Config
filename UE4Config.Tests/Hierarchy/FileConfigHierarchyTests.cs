using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using NUnit.Framework;
using UE4Config.Hierarchy;
using UE4Config.Parsing;

namespace UE4Config.Tests.Hierarchy
{
    static class Constants
    {
        public static readonly string MockProjectDir = TestUtils.GetTestDataPath("MockProject");
        public static readonly string MockEngineDir = TestUtils.GetTestDataPath("MockEngine");

        public static readonly string NonExistingCategory = "OtherCategory";
        public static readonly string NonExistingPlatform = "OtherPlatform";
    }

    [TestFixture]
    public class FileConfigHierarchyTest
    {
        private class MockFileConfigHierarchy : FileConfigHierarchy
        {
            public delegate ConfigIni HandleLoadConfig(string platform, string category, ConfigHierarchyLevel level);
            public delegate void HandleCacheConfig(string platform, string category, ConfigHierarchyLevel level, ConfigIni config);

            public HandleLoadConfig OnLoadConfig;
            public HandleCacheConfig OnCacheConfig;

            public MockFileConfigHierarchy(string projectPath, string enginePath) : base(projectPath, enginePath)
            {}

            protected override ConfigIni LoadConfig(string platform, string category, ConfigHierarchyLevel level)
            {
                if (OnLoadConfig != null)
                {
                    return OnLoadConfig(platform, category, level);
                }
                else
                {
                    return base.LoadConfig(platform, category, level);
                }
            }

            protected override void CacheConfig(string platform, string category, ConfigHierarchyLevel level, ConfigIni config)
            {
                if (OnCacheConfig != null)
                {
                    OnCacheConfig(platform, category, level, config);
                }
                else
                {
                    base.CacheConfig(platform, category, level, config);
                }
            }

            public void Exposed_CacheConfig(string platform, string category, ConfigHierarchyLevel level, ConfigIni config)
            {
                CacheConfig(platform, category, level, config);
            }

            public ConfigIni Exposed_GetCachedConfig(string platform, string category, ConfigHierarchyLevel level)
            {
                return GetCachedConfig(platform, category, level);
            }

            public ConfigIni Exposed_LoadConfig(string platform, string category, ConfigHierarchyLevel level)
            {
                return LoadConfig(platform, category, level);
            }
        }


        [Test]
        public void Constructor()
        {
            var hierarchy = new FileConfigHierarchy(@".\MyProject\", @".\Engine\");
            Assert.That(hierarchy.ProjectPath, Is.EqualTo(@".\MyProject\"));
            Assert.That(hierarchy.EnginePath, Is.EqualTo(@".\Engine\"));
        }

        [Test]
        public void ConstructorWithNullEnginePath()
        {
            var hierarchy = new FileConfigHierarchy(@".\MyProject\", null);
            Assert.That(hierarchy.ProjectPath, Is.EqualTo(@".\MyProject\"));
            Assert.That(hierarchy.EnginePath, Is.Null);
        }

        [Test]
        public void ConstructorWithNullProjectPath()
        {
            var hierarchy = new FileConfigHierarchy(null, @".\Engine\");
            Assert.That(hierarchy.ProjectPath, Is.Null);
            Assert.That(hierarchy.EnginePath, Is.EqualTo(@".\Engine\"));
        }

        [TestFixture]
        public class GetConfigFilePath
        {
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.Base)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BaseCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_CalledWithValidLevel(string platform, string category, ConfigHierarchyLevel level)
            {
                var hierarchy = new MockFileConfigHierarchy(@".\MyProject\", @".\Engine\")
                { };

                var path = hierarchy.GetConfigFilePath(platform, category, level);
                Assert.That(path, Is.Not.Null);
                Assert.That(path, Is.Not.Empty);
            }

            [TestCase("MyCategory", ConfigHierarchyLevel.Base)]
            [TestCase("MyCategory", ConfigHierarchyLevel.BaseCategory)]
            [TestCase("MyCategory", ConfigHierarchyLevel.ProjectCategory)]
            public void When_CalledWithDefaultPlatformAndValidLevel(string category, ConfigHierarchyLevel level)
            {
                var hierarchy = new MockFileConfigHierarchy(@".\MyProject\", @".\Engine\")
                { };

                var path = hierarchy.GetConfigFilePath("Default", category, level);
                Assert.That(path, Is.Not.Null);
                Assert.That(path, Is.Not.Empty);
            }

            [TestCase("MyCategory", ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase("MyCategory", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_CalledWithDefaultPlatformAndPlatformLevel(string category, ConfigHierarchyLevel level)
            {
                var hierarchy = new MockFileConfigHierarchy(@".\MyProject\", @".\Engine\")
                { };

                var path = hierarchy.GetConfigFilePath("Default", category, level);
                Assert.That(path, Is.Null);
            }

            [Test]
            public void When_CalledWithInvalidLevel()
            {
                var hierarchy = new MockFileConfigHierarchy(@".\MyProject\", @".\Engine\")
                { };

                Assert.That(() => {
                    hierarchy.GetConfigFilePath("MyPlatform", "MyCategory", (ConfigHierarchyLevel) (-1));
                }, Throws.TypeOf<InvalidEnumArgumentException>());

            }
        }

        [TestFixture]
        public class CacheConfig
        {
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.Base)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BaseCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_CachingConfig(string platform, string category, ConfigHierarchyLevel level)
            {
                var hierarchy = new MockFileConfigHierarchy(@".\MyProject\", @".\Engine\");
                var dummyConfig = new ConfigIni();

                hierarchy.Exposed_CacheConfig(platform, category, level, dummyConfig);
                var gotConfig = hierarchy.Exposed_GetCachedConfig(platform, category, level);

                Assert.That(gotConfig, Is.SameAs(dummyConfig));
            }

            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.Base)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BaseCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_CachingConfigAgain(string platform, string category, ConfigHierarchyLevel level)
            {
                var hierarchy = new MockFileConfigHierarchy(@".\MyProject\", @".\Engine\");
                var dummyConfig1 = new ConfigIni();
                var dummyConfig2 = new ConfigIni();

                hierarchy.Exposed_CacheConfig(platform, category, level, dummyConfig1);
                hierarchy.Exposed_CacheConfig(platform, category, level, dummyConfig2);
                var gotConfig = hierarchy.Exposed_GetCachedConfig(platform, category, level);

                Assert.That(gotConfig, Is.SameAs(dummyConfig2));
            }

            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.Base)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BaseCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_FetchingUncachedConfigFromEmptyCache(string platform, string category, ConfigHierarchyLevel level)
            {
                var hierarchy = new MockFileConfigHierarchy(@".\MyProject\", @".\Engine\");

                var gotConfig = hierarchy.Exposed_GetCachedConfig(platform, category, level);

                Assert.That(gotConfig, Is.Null);
            }

            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.Base)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BaseCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_FetchingUncachedConfigFromFilledCache(string platform, string category, ConfigHierarchyLevel level)
            {
                var hierarchy = new MockFileConfigHierarchy(@".\MyProject\", @".\Engine\");
                var dummyConfig = new ConfigIni();

                hierarchy.Exposed_CacheConfig("OtherPlatform", category, level, dummyConfig);
                hierarchy.Exposed_CacheConfig(platform, "OtherCategory", level, dummyConfig);
                var gotConfig = hierarchy.Exposed_GetCachedConfig(platform, category, level);

                Assert.That(gotConfig, Is.Null);
            }

            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.Base)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BaseCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_FetchingCachedConfigFromFilledCache(string platform, string category, ConfigHierarchyLevel level)
            {
                var hierarchy = new MockFileConfigHierarchy(@".\MyProject\", @".\Engine\");
                var dummyConfig1 = new ConfigIni();
                var dummyConfig2 = new ConfigIni();

                hierarchy.Exposed_CacheConfig(platform, category, level, dummyConfig1);
                hierarchy.Exposed_CacheConfig("OtherPlatform", category, level, dummyConfig2);
                hierarchy.Exposed_CacheConfig(platform, "OtherCategory", level, dummyConfig2);
                var gotConfig = hierarchy.Exposed_GetCachedConfig(platform, category, level);

                Assert.That(gotConfig, Is.SameAs(dummyConfig1));
            }
        }

        [TestFixture]
        public class GetConfig
        {
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.Base)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BaseCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_CalledOnce_CallsLoadConfig(string platform, string category, ConfigHierarchyLevel level)
            {
                var hierarchy = new MockFileConfigHierarchy(@".\MyProject\", @".\Engine\");
                var fakeLoadedConfig = new ConfigIni();

                bool didCallLoadConfig = false;
                hierarchy.OnLoadConfig += (loadPlatform, loadCategory, loadHierarchyLevel) =>
                    {
                        Assert.That(loadPlatform, Is.EqualTo(platform));
                        Assert.That(loadCategory, Is.EqualTo(category));
                        Assert.That(loadHierarchyLevel, Is.EqualTo(level));
                        didCallLoadConfig = true;
                        return fakeLoadedConfig;
                    };

                var gotConfig = hierarchy.GetConfig(platform, category, level);
                Assert.That(didCallLoadConfig, Is.True);
                Assert.That(gotConfig, Is.SameAs(fakeLoadedConfig));
            }


            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.Base)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BaseCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_CalledTwice_SkipsLoadConfig(string platform, string category, ConfigHierarchyLevel level)
            {
                var hierarchy = new MockFileConfigHierarchy(@".\MyProject\", @".\Engine\");
                var fakeLoadedConfig = new ConfigIni();

                int countCallLoadConfig = 0;
                hierarchy.OnLoadConfig += (loadPlatform, loadCategory, loadHierarchyLevel) =>
                {
                    countCallLoadConfig++;
                    return fakeLoadedConfig;
                };

                var gotConfig1 = hierarchy.GetConfig(platform, category, level);
                var gotConfig2 = hierarchy.GetConfig(platform, category, level);
                Assert.That(gotConfig2, Is.SameAs(gotConfig1));
                Assert.That(gotConfig2, Is.SameAs(fakeLoadedConfig));
                Assert.That(countCallLoadConfig, Is.EqualTo(1));
            }
        }
        
        [TestFixture]
        public class CreateConfig
        {
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.Base)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BaseCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectCategory)]
            [TestCase("MyPlatform", "MyCategory", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_Called_CallsCacheConfig(string platform, string category, ConfigHierarchyLevel level)
            {
                var hierarchy = new MockFileConfigHierarchy(@".\MyProject\", @".\Engine\");
                var spiedConfig = new ConfigIni();

                bool didCallCacheConfig = false;
                hierarchy.OnCacheConfig += (loadPlatform, loadCategory, loadHierarchyLevel, config) =>
                    {
                        Assert.That(loadPlatform, Is.EqualTo(platform));
                        Assert.That(loadCategory, Is.EqualTo(category));
                        Assert.That(loadHierarchyLevel, Is.EqualTo(level));
                        spiedConfig = config;
                        didCallCacheConfig = true;
                    };

                var createdConfig = hierarchy.CreateConfig(platform, category, level);
                Assert.That(didCallCacheConfig, Is.True);
                Assert.That(createdConfig, Is.SameAs(spiedConfig));
            }
        }

        [TestFixture]
        public class LoadConfig
        {
            [TestCase("Windows", "Game", ConfigHierarchyLevel.Base)]
            [TestCase("Windows", "Game", ConfigHierarchyLevel.BaseCategory)]
            [TestCase("Windows", "Game", ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase("Windows", "Game", ConfigHierarchyLevel.ProjectCategory)]
            [TestCase("Windows", "Game", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_CalledOnValidConfigSpecifierWithExistingFile(string platform, string category, ConfigHierarchyLevel level)
            {
                var projectPath = TestUtils.GetTestDataPath(@".\MockProject");
                var enginePath = TestUtils.GetTestDataPath(@".\MockEngine");
                var hierarchy = new MockFileConfigHierarchy(projectPath, enginePath);

                var config = hierarchy.Exposed_LoadConfig(platform, category, level);

                Assert.That(config, Is.Not.Null);
                Assert.That(config.Name, Is.Not.Null);
                Assert.That(config.Name, Is.Not.EqualTo(""));
            }

            [TestCase("Windows", "Lightmass", ConfigHierarchyLevel.ProjectCategory)]
            [TestCase("OtherPlatform", "Game", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_CalledOnValidConfigSpecifierWithMissingFile(string platform, string category, ConfigHierarchyLevel level)
            {
                var projectPath = TestUtils.GetTestDataPath(@".\MockProject");
                var enginePath = TestUtils.GetTestDataPath(@".\MockEngine");
                var hierarchy = new MockFileConfigHierarchy(projectPath, enginePath);

                var config = hierarchy.Exposed_LoadConfig(platform, category, level);

                Assert.That(config, Is.Null);
            }

            [TestCase("Default", "Game", ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase("Default", "Game", ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_CalledOnInvalidConfigSpecifier(string platform, string category, ConfigHierarchyLevel level)
            {
                var projectPath = TestUtils.GetTestDataPath(@".\MockProject");
                var enginePath = TestUtils.GetTestDataPath(@".\MockEngine");
                var hierarchy = new MockFileConfigHierarchy(projectPath, enginePath);

                var config = hierarchy.Exposed_LoadConfig(platform, category, level);

                Assert.That(config, Is.Null);
            }
        }

        [TestFixture]
        public class CheckEngineHasPlatformExtension
        {
            [TestCase("Switch")]
            [TestCase("XBoxOne")]
            [TestCase("Linux")]
            public void When_PlatformExtensionFolderExists(string platform)
            {
                var hierarchy = new MockFileConfigHierarchy(Constants.MockProjectDir, Constants.MockEngineDir) { };
                Assert.That(hierarchy.CheckEngineHasPlatformExtension(platform), Is.True);
            }

            [TestCase("Windows")]
            [TestCase("Mac")]
            public void When_PlatformExtensionFolderIsMissing(string platform)
            {
                var hierarchy = new MockFileConfigHierarchy(Constants.MockProjectDir, Constants.MockEngineDir) { };
                Assert.That(hierarchy.CheckEngineHasPlatformExtension(platform), Is.False);
            }
        }

        [TestFixture]
        public class CheckProjectHasPlatformExtension
        {
            [TestCase("Switch")]
            [TestCase("XBoxOne")]
            [TestCase("Mac")]
            public void When_PlatformExtensionFolderExists(string platform)
            {
                var hierarchy = new MockFileConfigHierarchy(Constants.MockProjectDir, Constants.MockEngineDir) { };
                Assert.That(hierarchy.CheckProjectHasPlatformExtension(platform), Is.True);
            }

            [TestCase("Windows")]
            [TestCase("Linux")]
            public void When_PlatformExtensionFolderIsMissing(string platform)
            {
                var hierarchy = new MockFileConfigHierarchy(Constants.MockProjectDir, Constants.MockEngineDir) { };
                Assert.That(hierarchy.CheckProjectHasPlatformExtension(platform), Is.False);
            }
        }

        [TestFixture]
        public class IntegrationTests
        {
            [TestFixture]
            public class GetConfigFilePath
            {
                public static IEnumerable<TestCaseData> ValidExistingConfigFilePaths
                {
                    get
                    {
                        string batchPlatform = "";
                        string batchCategory = "";

                        batchPlatform = "Windows";
                        batchCategory = "Game";
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectPlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Config", batchPlatform, $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Config", $"Default{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BasePlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Config", batchPlatform, $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BaseCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Config", $"Base{batchCategory}.ini"))
                        };
                    }
                }

                public static IEnumerable<TestCaseData> ValidNonexistingConfigFilePaths
                {
                    get
                    {
                        string batchPlatform = "";
                        string batchCategory = "";

                        batchPlatform = "Windows";
                        batchCategory = Constants.NonExistingCategory;
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectPlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Config", batchPlatform, $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Config", $"Default{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BasePlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Config", batchPlatform, $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BaseCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Config", $"Base{batchCategory}.ini"))
                        };

                        batchPlatform = Constants.NonExistingPlatform;
                        batchCategory = "Game";
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectPlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Config", batchPlatform, $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Config", $"Default{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BasePlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Config", batchPlatform, $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BaseCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Config", $"Base{batchCategory}.ini"))
                        };
                    }
                }

                [TestCaseSource(nameof(ValidExistingConfigFilePaths))]
                [TestCaseSource(nameof(ValidNonexistingConfigFilePaths))]
                public string When_FileCouldExist(string projectPath, string enginePath, string platform, string category, ConfigHierarchyLevel level)
                {
                    var hierarchy = new MockFileConfigHierarchy(projectPath, enginePath) { };
                    string path = hierarchy.GetConfigFilePath(platform, category, level);
                    return path;
                }

                public static IEnumerable<TestCaseData> Cases_OnlyPlatformExtensionConfigFilesExist
                {
                    get
                    {
                        string batchPlatform = "";
                        string batchCategory = "";

                        batchPlatform = "XBoxOne";
                        batchCategory = "Game";
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectPlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Platforms", batchPlatform, "Config", $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Config", $"Default{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BasePlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Platforms", batchPlatform, "Config", $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BaseCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Config", $"Base{batchCategory}.ini"))
                        };

                        batchPlatform = "Mac";
                        batchCategory = "Game";
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectPlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Platforms", batchPlatform, "Config", $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Config", $"Default{batchCategory}.ini"))
                        };
                        //Only has Project PlatformExtension, so legacy paths in engine
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BasePlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Config", batchPlatform, $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BaseCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Config", $"Base{batchCategory}.ini"))
                        };

                        batchPlatform = "Linux";
                        batchCategory = "Game";
                        //Only has Project PlatformExtension, so legacy paths in project
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectPlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Config", batchPlatform, $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Config", $"Default{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BasePlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Platforms", batchPlatform, "Config", $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BaseCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Config", $"Base{batchCategory}.ini"))
                        };
                    }
                }

                [TestCaseSource(nameof(Cases_OnlyPlatformExtensionConfigFilesExist))]
                public string When_OnlyPlatformExtensionConfigFilesExist(string projectPath, string enginePath, string platform, string category, ConfigHierarchyLevel level)
                {
                    var hierarchy = new MockFileConfigHierarchy(projectPath, enginePath) { };
                    string path = hierarchy.GetConfigFilePath(platform, category, level);
                    return path;
                }

                public static IEnumerable<TestCaseData> Cases_LegacyAndPlatformExtensionConfigFilesExist
                {
                    get
                    {
                        string batchPlatform = "";
                        string batchCategory = "";

                        batchPlatform = "Switch";
                        batchCategory = "Game";
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectPlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Platforms", batchPlatform, "Config", $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.ProjectCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockProjectDir, "Config", $"Default{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BasePlatformCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Platforms", batchPlatform, "Config", $"{batchPlatform}{batchCategory}.ini"))
                        };
                        yield return new TestCaseData(Constants.MockProjectDir, Constants.MockEngineDir, batchPlatform, batchCategory, ConfigHierarchyLevel.BaseCategory)
                        {
                            ExpectedResult = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(Constants.MockEngineDir, "Config", $"Base{batchCategory}.ini"))
                        };
                    }
                }

                [TestCaseSource(nameof(Cases_LegacyAndPlatformExtensionConfigFilesExist))]
                public string When_LegacyAndPlatformExtensionConfigFilesExist(string projectPath, string enginePath, string platform, string category, ConfigHierarchyLevel level)
                {
                    var hierarchy = new MockFileConfigHierarchy(projectPath, enginePath) { };
                    string path = hierarchy.GetConfigFilePath(platform, category, level);
                    return path;
                }
            }
        }
    }
}