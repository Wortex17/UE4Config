using System;
using System.Collections.Generic;
using NUnit.Framework;
using UE4Config.Evaluation;
using UE4Config.Hierarchy;
using UE4Config.Parsing;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class ConfigHierarchyTests
    {
        private class MockConfigHierarchy : ConfigHierarchy
        {
            public delegate ConfigIni SpyGetConfig(string platform, string category, ConfigHierarchyLevel level);
            public delegate void SpySetConfig(string platform, string category, ConfigHierarchyLevel level, ConfigIni config);
            public delegate ConfigIni SpyCreateConfig(string platform, string category, ConfigHierarchyLevel level);
            
            public SpyGetConfig OnSpyGetConfig;
            public SpySetConfig OnSpySetConfig;
            public SpyCreateConfig OnSpyCreateConfig;

            public Action<string, string, string, string, ConfigHierarchyLevelRange, PropertyEvaluator, IList<string>> OnEvaluatePropertyValues;

            public override bool CheckEngineHasPlatformExtension(string platform)
            {
                throw new NotImplementedException();
            }

            public override bool CheckProjectHasPlatformExtension(string platform)
            {
                throw new NotImplementedException();
            }

            public override ConfigIni GetConfig(string platform, string category, ConfigHierarchyLevel level)
            {
                return OnSpyGetConfig?.Invoke(platform, category, level);
            }

            public override void PublishConfig(string platform, string category, ConfigHierarchyLevel level, ConfigIni config)
            {
                OnSpySetConfig?.Invoke(platform, category, level, config);
            }

            public override ConfigIni CreateConfig(string platform, string category, ConfigHierarchyLevel level)
            {
                return OnSpyCreateConfig?.Invoke(platform, category, level);
            }

            public override void EvaluatePropertyValues(string platform, string category, string sectionName, string propertyKey, ConfigHierarchyLevelRange range,
                PropertyEvaluator evaluator, IList<string> values)
            {
                OnEvaluatePropertyValues?.Invoke(platform, category, sectionName, propertyKey, range, evaluator, values);
                base.EvaluatePropertyValues(platform, category, sectionName, propertyKey, range, evaluator, values);
            }
        }

        [TestFixture]
        public class GetConfig
        {
            [Test]
            public void When_CalledWithoutPlatform_RelaysToDefaultPlatform()
            {
                int spyCount = 0;

                var expectedConfig = new ConfigIni("MyConfig");

                var hierarchy = new MockConfigHierarchy()
                {
                    OnSpyGetConfig = (platform, category, level) =>
                    {
                        spyCount++;
                        Assert.That(level, Is.EqualTo(ConfigHierarchyLevel.BaseCategory));
                        Assert.That(category, Is.EqualTo("MyCategory"));
                        Assert.That(platform, Is.EqualTo("Default"));
                        return expectedConfig;
                    }
                };

                var config = hierarchy.GetConfig("MyCategory", ConfigHierarchyLevel.BaseCategory);
                Assert.That(spyCount, Is.EqualTo(1));
                Assert.That(config, Is.SameAs(expectedConfig));
            }
        }

        [TestFixture]
        public class SetConfig
        {
            [Test]
            public void When_CalledWithoutPlatform_RelaysToDefaultPlatform()
            {
                int spyCount = 0;

                var expectedConfig = new ConfigIni("MyConfig");
                ConfigIni spiedConfig = null;

                var hierarchy = new MockConfigHierarchy()
                {
                    OnSpySetConfig = (platform, category, level, config) =>
                    {
                        spyCount++;
                        Assert.That(level, Is.EqualTo(ConfigHierarchyLevel.BaseCategory));
                        Assert.That(category, Is.EqualTo("MyCategory"));
                        Assert.That(platform, Is.EqualTo("Default"));
                        spiedConfig = config;
                    }
                };

                hierarchy.PublishConfig("MyCategory", ConfigHierarchyLevel.BaseCategory, expectedConfig);
                Assert.That(spyCount, Is.EqualTo(1));
                Assert.That(spiedConfig, Is.SameAs(expectedConfig));
            }
        }

        [TestFixture]
        public class GetConfigs
        {
            [Test]
            public void When_CalledWithoutPlatform_RelaysToDefaultPlatform()
            {
                int spyCount = 0;
                string latestRequestedPlatform = null;
                string latestRequestedCategory = null;
                ConfigHierarchyLevel latestRequestedLevel = (ConfigHierarchyLevel)(-1);

                Dictionary<ConfigHierarchyLevel, ConfigIni> expectedConfigs = new Dictionary<ConfigHierarchyLevel, ConfigIni>();
                expectedConfigs[ConfigHierarchyLevel.Base] = new ConfigIni("Base.ini");
                expectedConfigs[ConfigHierarchyLevel.BaseCategory] = new ConfigIni("BaseMyCategory.ini");
                expectedConfigs[ConfigHierarchyLevel.BasePlatformCategory] = new ConfigIni("DefaultMyCategory.ini");


                var hierarchy = new MockConfigHierarchy()
                {
                    OnSpyGetConfig = (platform, category, level) =>
                    {
                        spyCount++;

                        Assert.That(level, Is.GreaterThan(latestRequestedLevel));
                        Assert.That(category, Is.EqualTo("MyCategory"));
                        Assert.That(platform, Is.EqualTo("Default"));

                        latestRequestedPlatform = platform;
                        latestRequestedCategory = category;
                        latestRequestedLevel = level;
                        ConfigIni config;
                        if (expectedConfigs.TryGetValue(level, out config))
                        {
                            return config;
                        }
                        return null;
                    }
                };

                var configs = new List<ConfigIni>();
                hierarchy.GetConfigs("MyCategory", configs);
                Assert.That(spyCount, Is.GreaterThanOrEqualTo(1));
                Assert.That(configs, Is.EquivalentTo(expectedConfigs.Values));
                Assert.That(latestRequestedLevel, Is.EqualTo(ConfigHierarchyLevel.ProjectPlatformCategory));
                Assert.That(latestRequestedCategory, Is.EqualTo("MyCategory"));
                Assert.That(latestRequestedPlatform, Is.EqualTo("Default"));
            }


            [Test]
            public void When_CalledWithoutLevel_DefaultsToProjectPlatformCategory()
            {
                int spyCount = 0;
                string latestRequestedPlatform = null;
                string latestRequestedCategory = null;
                ConfigHierarchyLevel latestRequestedLevel = (ConfigHierarchyLevel)(-1);

                Dictionary<ConfigHierarchyLevel, ConfigIni> expectedConfigs = new Dictionary<ConfigHierarchyLevel, ConfigIni>();
                expectedConfigs[ConfigHierarchyLevel.Base] = new ConfigIni("Base.ini");
                expectedConfigs[ConfigHierarchyLevel.BaseCategory] = new ConfigIni("BaseMyCategory.ini");
                expectedConfigs[ConfigHierarchyLevel.BasePlatformCategory] = new ConfigIni("PlatformMyCategory.ini");
                expectedConfigs[ConfigHierarchyLevel.ProjectCategory] = new ConfigIni("DefaultMyCategory.ini");
                expectedConfigs[ConfigHierarchyLevel.ProjectPlatformCategory] = new ConfigIni("PlatformMyCategory.ini");


                var hierarchy = new MockConfigHierarchy()
                {
                    OnSpyGetConfig = (platform, category, level) =>
                    {
                        spyCount++;

                        Assert.That(level, Is.GreaterThan(latestRequestedLevel));
                        Assert.That(category, Is.EqualTo("MyCategory"));
                        Assert.That(platform, Is.EqualTo("MyPlatform"));

                        latestRequestedPlatform = platform;
                        latestRequestedCategory = category;
                        latestRequestedLevel = level;
                        ConfigIni config;
                        if (expectedConfigs.TryGetValue(level, out config))
                        {
                            return config;
                        }
                        return null;
                    }
                };

                var configs = new List<ConfigIni>();
                hierarchy.GetConfigs("MyPlatform", "MyCategory", configs);
                Assert.That(spyCount, Is.GreaterThanOrEqualTo(1));
                Assert.That(configs, Is.EquivalentTo(expectedConfigs.Values));
                Assert.That(latestRequestedLevel, Is.EqualTo(ConfigHierarchyLevel.ProjectPlatformCategory));
                Assert.That(latestRequestedCategory, Is.EqualTo("MyCategory"));
                Assert.That(latestRequestedPlatform, Is.EqualTo("MyPlatform"));
            }
        }
    
        [TestFixture]
        public class GetOrCreateConfig
        {
            [Test]
            public void When_CalledWithoutPlatform_RelaysToDefaultPlatform()
            {
                int spyCount = 0;

                var expectedConfig = new ConfigIni("MyConfig");

                var hierarchy = new MockConfigHierarchy()
                {
                    OnSpyGetConfig = (platform, category, level) =>
                    {
                        spyCount++;
                        Assert.That(level, Is.EqualTo(ConfigHierarchyLevel.BaseCategory));
                        Assert.That(category, Is.EqualTo("MyCategory"));
                        Assert.That(platform, Is.EqualTo("Default"));
                        return expectedConfig;
                    }
                };

                hierarchy.GetOrCreateConfig("MyCategory", ConfigHierarchyLevel.BaseCategory, out _);
                Assert.That(spyCount, Is.EqualTo(1));
            }
            
            [Test]
            public void When_CalledWithoutPlatform_ReturnsPositiveGetConfigResult()
            {
                var expectedConfig = new ConfigIni("MyConfig");

                var hierarchy = new MockConfigHierarchy()
                {
                    OnSpyGetConfig = (platform, category, level) => expectedConfig
                };

                var config = hierarchy.GetOrCreateConfig("MyCategory", ConfigHierarchyLevel.BaseCategory, out bool wasCreated);
                Assert.That(wasCreated, Is.False);
                Assert.That(config, Is.SameAs(expectedConfig));
            }
            
            [Test]
            public void When_CalledWithoutPlatform_ReturnsCreateConfigResult()
            {
                var expectedConfig = new ConfigIni("MyConfig");

                var hierarchy = new MockConfigHierarchy()
                {
                    OnSpyGetConfig = (platform, category, level) => null,
                    OnSpyCreateConfig = (platform, category, level) => expectedConfig
                };

                var config = hierarchy.GetOrCreateConfig("MyCategory", ConfigHierarchyLevel.BaseCategory, out bool wasCreated);
                Assert.That(wasCreated, Is.True);
                Assert.That(config, Is.SameAs(expectedConfig));
            }
        }
        
        [TestFixture]
        public class EvaluatePropertyValues
        {
            [Test]
            public void When_CalledWithoutEvaluator()
            {
                int spyCount = 0;
                string inPlatform = "MyPlatform";
                string inCategory = "MyCategory";
                string inSectionName = "MySection";
                string inPropertyKey = "MyProperty";
                List<string> inTargetValues = new List<string>();

                var hierarchy = new MockConfigHierarchy()
                {
                    OnEvaluatePropertyValues = (string platform, string category, string sectionName, string propertyKey, ConfigHierarchyLevelRange range,
                        PropertyEvaluator evaluator, IList<string> values) =>
                    {
                        spyCount++;

                        Assert.That(platform, Is.EqualTo(inPlatform));
                        Assert.That(category, Is.EqualTo(inCategory));
                        Assert.That(sectionName, Is.EqualTo(inSectionName));
                        Assert.That(propertyKey, Is.EqualTo(inPropertyKey));
                        Assert.That(evaluator, Is.SameAs(PropertyEvaluator.Default));
                        Assert.That(values, Is.SameAs(inTargetValues));
                    }
                };

                hierarchy.EvaluatePropertyValues(inPlatform, inCategory, inSectionName, inPropertyKey, inTargetValues);
                Assert.That(spyCount, Is.EqualTo(1));
            }

            [Test]
            public void When_CalledWithoutPlatformNorEvaluator()
            {
                int spyCount = 0;
                string inCategory = "MyCategory";
                string inSectionName = "MySection";
                string inPropertyKey = "MyProperty";
                List<string> inTargetValues = new List<string>();

                var hierarchy = new MockConfigHierarchy()
                {
                    OnEvaluatePropertyValues = (string platform, string category, string sectionName, string propertyKey, ConfigHierarchyLevelRange range,
                        PropertyEvaluator evaluator, IList<string> values) =>
                    {
                        spyCount++;

                        Assert.That(platform, Is.EqualTo("Default"));
                        Assert.That(category, Is.EqualTo(inCategory));
                        Assert.That(sectionName, Is.EqualTo(inSectionName));
                        Assert.That(propertyKey, Is.EqualTo(inPropertyKey));
                        Assert.That(evaluator, Is.SameAs(PropertyEvaluator.Default));
                        Assert.That(values, Is.SameAs(inTargetValues));
                    }
                };

                hierarchy.EvaluatePropertyValues(inCategory, inSectionName, inPropertyKey, inTargetValues);
                Assert.That(spyCount, Is.EqualTo(1));
            }

            [Test]
            public void When_CalledWithoutPlatform()
            {
                int spyCount = 0;
                string inCategory = "MyCategory";
                string inSectionName = "MySection";
                string inPropertyKey = "MyProperty";
                var inEvaluator = new PropertyEvaluator();
                List<string> inTargetValues = new List<string>();

                var hierarchy = new MockConfigHierarchy()
                {
                    OnEvaluatePropertyValues = (string platform, string category, string sectionName, string propertyKey, ConfigHierarchyLevelRange range,
                        PropertyEvaluator evaluator, IList<string> values) =>
                    {
                        spyCount++;

                        Assert.That(platform, Is.EqualTo("Default"));
                        Assert.That(category, Is.EqualTo(inCategory));
                        Assert.That(sectionName, Is.EqualTo(inSectionName));
                        Assert.That(propertyKey, Is.EqualTo(inPropertyKey));
                        Assert.That(evaluator, Is.SameAs(inEvaluator));
                        Assert.That(values, Is.SameAs(inTargetValues));
                    }
                };

                hierarchy.EvaluatePropertyValues(inCategory, inSectionName, inPropertyKey, inEvaluator, inTargetValues);
                Assert.That(spyCount, Is.EqualTo(1));
            }

            [Test]
            public void When_CalledWithoutRange()
            {
                int spyCount = 0;
                string inPlatform = "MyPlatform";
                string inCategory = "MyCategory";
                string inSectionName = "MySection";
                string inPropertyKey = "MyProperty";
                var inEvaluator = new PropertyEvaluator();
                List<string> inTargetValues = new List<string>();

                var hierarchy = new MockConfigHierarchy()
                {
                    OnEvaluatePropertyValues = (string platform, string category, string sectionName, string propertyKey, ConfigHierarchyLevelRange range,
                        PropertyEvaluator evaluator, IList<string> values) =>
                    {
                        spyCount++;

                        Assert.That(platform, Is.EqualTo(inPlatform));
                        Assert.That(category, Is.EqualTo(inCategory));
                        Assert.That(sectionName, Is.EqualTo(inSectionName));
                        Assert.That(propertyKey, Is.EqualTo(inPropertyKey));
                        Assert.That(range, Is.EqualTo(ConfigHierarchyLevelRange.All()));
                        Assert.That(evaluator, Is.SameAs(inEvaluator));
                        Assert.That(values, Is.SameAs(inTargetValues));
                    }
                };

                hierarchy.EvaluatePropertyValues(inPlatform, inCategory, inSectionName, inPropertyKey, inEvaluator, inTargetValues);
                Assert.That(spyCount, Is.EqualTo(1));
            }
        }
    }
}