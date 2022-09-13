using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UE4Config.Hierarchy;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture(typeof(ConfigTreeUE427))]
    public class IConfigTreeTests<T> where T : IConfigTree, new()
    {
        protected T NewTree()
        {
            return new T();
        }

        [Test]
        public void RegisterPlatform()
        {
            var tree = NewTree();
            var platformIdentifier = "GenericPlatform";
            
            var platform = tree.RegisterPlatform(platformIdentifier);
            
            Assert.That(platform, Is.Not.Null);
            Assert.That(platform.Identifier, Is.EqualTo(platformIdentifier));
            Assert.That(platform.ParentPlatform, Is.Null);
        }
        
        [Test]
        public void RegisterPlatformWithParent()
        {
            var tree = NewTree();
            var platformIdentifier = "GenericPlatform";
            var parentPlatform = tree.RegisterPlatform(platformIdentifier+"Parent");
            
            var platform = tree.RegisterPlatform(platformIdentifier, parentPlatform);
            
            Assert.That(platform, Is.Not.Null);
            Assert.That(platform.Identifier, Is.EqualTo(platformIdentifier));
            Assert.That(platform.ParentPlatform, Is.SameAs(parentPlatform));
        }
        
        [Test]
        public void GetPlatform()
        {
            var tree = NewTree();
            var platformIdentifier = "GenericPlatform";
            var platform = tree.RegisterPlatform(platformIdentifier);
            
            var gotPlatform = tree.GetPlatform(platformIdentifier);
            
            Assert.That(gotPlatform, Is.SameAs(platform));
        }
        
        [Test]
        public void GetPlatform_ReturnsSamePlatform()
        {
            var tree = NewTree();
            var platformIdentifier = "GenericPlatform";
            tree.RegisterPlatform(platformIdentifier);
            
            var gotPlatform = tree.GetPlatform(platformIdentifier);
            var gotPlatform2 = tree.GetPlatform(platformIdentifier);
            
            Assert.That(gotPlatform2, Is.SameAs(gotPlatform));
        }
        
        [Test]
        public void GetPlatform_WithUnregisteredPlatform()
        {
            var tree = NewTree();
            var platformIdentifier = "GenericPlatform";
            
            var gotPlatform = tree.GetPlatform(platformIdentifier);
            
            Assert.That(gotPlatform, Is.Null);
        }
        
        public static IEnumerable Cases_ConfigType
        {
            get
            {
                yield return new TestCaseData(new object[]{ConfigCategory.Compat});
                yield return new TestCaseData(new object[]{ConfigCategory.Editor});
                yield return new TestCaseData(new object[]{ConfigCategory.Engine});
                yield return new TestCaseData(new object[]{ConfigCategory.Game});
                yield return new TestCaseData(new object[]{ConfigCategory.Input});
                yield return new TestCaseData(new object[]{ConfigCategory.Lightmass});
                yield return new TestCaseData(new object[]{ConfigCategory.Scalability});
                yield return new TestCaseData(new object[]{ConfigCategory.DeviceProfiles});
                yield return new TestCaseData(new object[]{ConfigCategory.EditorGameAgnostic});
                yield return new TestCaseData(new object[]{ConfigCategory.EditorKeyBindings});
                yield return new TestCaseData(new object[]{ConfigCategory.EditorUserSettings});
            }
        }

        [Test]
        public void VisitConfigRoot()
        {
            var tree = NewTree();
            ConfigFileReference? result = null;

            tree.VisitConfigRoot((configFileReference) =>
            {
                result = configFileReference;
            });
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(new ConfigFileReference(ConfigDomain.EngineBase, null, null)));
        }
        
        [TestCaseSource(nameof(Cases_ConfigType))]
        public void VisitConfigBranch_WithoutPlatformIdentifier(string configType)
        {
            var tree = NewTree();
            
            var configBranch = new List<ConfigFileReference>();
            tree.VisitConfigBranch(configType, null, reference => configBranch.Add(reference));

            var expectedConfigBranch = new List<ConfigFileReference>()
            {
                // Engine/Base.ini
                tree.GetConfigRoot().GetValueOrDefault(),
                // Engine/Base*.ini
                new ConfigFileReference(ConfigDomain.EngineBase, null, configType),
                // Project/Default*.ini
                new ConfigFileReference(ConfigDomain.Project, null, configType),
                // Project/Generated*.ini
                new ConfigFileReference(ConfigDomain.ProjectGenerated, null, configType),
            };
            
            Assert.That(configBranch, Is.EquivalentTo(expectedConfigBranch));
        }

        void Assert_VisitConfigBranch_WithPlatformWithoutInheritance(T tree, string configType, IConfigPlatform platform, List<ConfigFileReference> configBranch)
        {
            var expectedConfigBranch = new List<ConfigFileReference>()
            {
                // Engine/Base.ini
                tree.GetConfigRoot().GetValueOrDefault(),
                // Engine/Base*.ini
                new ConfigFileReference(ConfigDomain.EngineBase, null, configType),
                // Engine/Platform/BasePlatform*.ini
                new ConfigFileReference(ConfigDomain.EngineBase, platform, configType),
                // Project/Default*.ini
                new ConfigFileReference(ConfigDomain.Project, null, configType),
                // Project/Generated*.ini
                new ConfigFileReference(ConfigDomain.ProjectGenerated, null, configType),
                // Engine/Platform/Platform*.ini
                new ConfigFileReference(ConfigDomain.Engine, platform, configType),
                // Project/Platform/Platform*.ini
                new ConfigFileReference(ConfigDomain.Project, platform, configType),
                // Project/Platform/GeneratedPlatform*.ini
                new ConfigFileReference(ConfigDomain.ProjectGenerated, platform, configType),
            };
            
            Assert.That(configBranch, Is.EquivalentTo(expectedConfigBranch));
        }
        
        [TestCaseSource(nameof(Cases_ConfigType))]
        public void VisitConfigBranch_WithUnregisteredPlatformWithoutInheritance(string configType)
        {
            var tree = NewTree();
            var platformIdentifier = "GenericPlatform";
            var configBranch = new List<ConfigFileReference>();
            tree.VisitConfigBranch(configType, platformIdentifier, reference => configBranch.Add(reference));
            var platform = tree.GetPlatform(platformIdentifier);

            Assert_VisitConfigBranch_WithPlatformWithoutInheritance(tree, configType, platform, configBranch);
        }
        
        [TestCaseSource(nameof(Cases_ConfigType))]
        public void VisitConfigBranch_WithRegisteredPlatformWithoutInheritance(string configType)
        {
            var tree = NewTree();
            var platformIdentifier = "GenericPlatform";
            var configBranch = new List<ConfigFileReference>();
            var platform = tree.RegisterPlatform(platformIdentifier);
            
            tree.VisitConfigBranch(configType, platformIdentifier, reference => configBranch.Add(reference));

            Assert_VisitConfigBranch_WithPlatformWithoutInheritance(tree, configType, platform, configBranch);
        }
        
        [TestCaseSource(nameof(Cases_ConfigType))]
        public void VisitConfigBranch_WithRegisteredPlatformWithInheritance(string configType)
        {
            var tree = NewTree();
            var platformIdentifier = "GenericPlatform";
            var configBranch = new List<ConfigFileReference>();
            var grandParentPlatform = tree.RegisterPlatform(platformIdentifier+"GrandParent");
            var parentPlatform = tree.RegisterPlatform(platformIdentifier+"Parent", grandParentPlatform);
            var platform = tree.RegisterPlatform(platformIdentifier, parentPlatform);
            
            tree.VisitConfigBranch(configType, platformIdentifier, reference => configBranch.Add(reference));

            var expectedConfigBranch = new List<ConfigFileReference>()
            {
                // Engine/Base.ini
                tree.GetConfigRoot().GetValueOrDefault(),
                // Engine/Base*.ini
                new ConfigFileReference(ConfigDomain.EngineBase, null, configType),
                // Engine/Platform/BasePlatform*.ini
                new ConfigFileReference(ConfigDomain.EngineBase, grandParentPlatform, configType),
                new ConfigFileReference(ConfigDomain.EngineBase, parentPlatform, configType),
                new ConfigFileReference(ConfigDomain.EngineBase, platform, configType),
                // Project/Default*.ini
                new ConfigFileReference(ConfigDomain.Project, null, configType),
                // Project/Generated*.ini
                new ConfigFileReference(ConfigDomain.ProjectGenerated, null, configType),
                // Engine/Platform/Platform*.ini
                new ConfigFileReference(ConfigDomain.Engine, grandParentPlatform, configType),
                // Project/Platform/Platform*.ini
                new ConfigFileReference(ConfigDomain.Project, grandParentPlatform, configType),
                // Project/Platform/GeneratedPlatform*.ini
                new ConfigFileReference(ConfigDomain.ProjectGenerated, grandParentPlatform, configType),
                // Engine/Platform/Platform*.ini
                new ConfigFileReference(ConfigDomain.Engine, parentPlatform, configType),
                // Project/Platform/Platform*.ini
                new ConfigFileReference(ConfigDomain.Project, parentPlatform, configType),
                // Project/Platform/GeneratedPlatform*.ini
                new ConfigFileReference(ConfigDomain.ProjectGenerated, parentPlatform, configType),
                // Engine/Platform/Platform*.ini
                new ConfigFileReference(ConfigDomain.Engine, platform, configType),
                // Project/Platform/Platform*.ini
                new ConfigFileReference(ConfigDomain.Project, platform, configType),
                // Project/Platform/GeneratedPlatform*.ini
                new ConfigFileReference(ConfigDomain.ProjectGenerated, platform, configType),
            };
            //TODO: Missing User Layers
            Assert.That(configBranch, Is.EquivalentTo(expectedConfigBranch));
        }
    }
}