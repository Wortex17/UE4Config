using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
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
            public SpyGetConfig OnSpyGetConfig;

            public override ConfigIni GetConfig(string platform, string category, ConfigHierarchyLevel level)
            {
                return OnSpyGetConfig?.Invoke(platform, category, level);
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
        }
    }
}