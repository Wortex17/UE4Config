using System;
using System.Collections.Generic;
using NUnit.Framework;
using UE4Config.Hierarchy;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class IConfigTreeExtensionsTests
    {
        class TestConfigReferenceTree : IConfigReferenceTree
        {
            public Action<Action<ConfigFileReference>> OnVisitConfigRoot;
            public IConfigFileProvider FileProvider { get; private set; }

            public void Setup(IConfigFileProvider configFileProvider)
            {
                FileProvider = configFileProvider;
            }

            public void VisitConfigRoot(Action<ConfigFileReference> onConfig)
            {
                OnVisitConfigRoot(onConfig);
            }

            public Action<string, string, Action<ConfigFileReference>> OnVisitConfigBranch;
            public void VisitConfigBranch(string configType, string platformIdentifier, Action<ConfigFileReference> onConfig)
            {
                OnVisitConfigBranch(configType, platformIdentifier, onConfig);
            }

            public IConfigPlatform GetPlatform(string platformIdentifier)
            {
                throw new NotImplementedException();
            }

            public IConfigPlatform RegisterPlatform(string platformIdentifier, IConfigPlatform parentPlatform = null)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void GetConfigRoot()
        {
            var tree = new TestConfigReferenceTree();
            var expectedResult = new ConfigFileReference(ConfigDomain.None, new ConfigPlatform("MyPlatform"), "MyConfig");
            int onVisitConfigRootCount = 0;
            tree.OnVisitConfigRoot = action =>
            {
                onVisitConfigRootCount++;
                action(expectedResult);
            };
            
            ConfigFileReference? result = IConfigReferenceTreeExtensions.GetConfigRoot(tree);
            
            Assert.That(onVisitConfigRootCount, Is.EqualTo(1));
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetConfigRoot_WithoutCallback()
        {
            var tree = new TestConfigReferenceTree();
            int onVisitConfigRootCount = 0;
            tree.OnVisitConfigRoot = action =>
            {
                onVisitConfigRootCount++;
            };
            
            ConfigFileReference? result = IConfigReferenceTreeExtensions.GetConfigRoot(tree);
            
            Assert.That(onVisitConfigRootCount, Is.EqualTo(1));
            Assert.That(result, Is.Null);
        }
        
        [Test]
        public void GetConfigBranch()
        {
            var tree = new TestConfigReferenceTree();
            var expectedConfigType = "MyConfig";
            var expectedPlatformIdentifier = "MyPlatform";
            var expectedResult = new List<ConfigFileReference>();
            tree.OnVisitConfigBranch = (configType, platformIdentifier, action) =>
            {
                var configFileReference = new ConfigFileReference(ConfigDomain.None, new ConfigPlatform(platformIdentifier),
                    configType);
                expectedResult.Add(configFileReference);
                action(configFileReference);
            };
            
            var result = IConfigReferenceTreeExtensions.GetConfigBranch(tree, expectedConfigType, expectedPlatformIdentifier);
            
            Assert.That(result, Is.EquivalentTo(expectedResult));
            Assert.That(
                result.FindIndex(reference => reference.Platform != null),
                Is.GreaterThanOrEqualTo(0));
            Assert.That(
                result.FindIndex(reference => reference.Type != null),
                Is.GreaterThanOrEqualTo(0));
        }
        
        [Test]
        public void GetConfigBranch_WithoutCallback()
        {
            var tree = new TestConfigReferenceTree();
            tree.OnVisitConfigBranch = (configType, platformIdentifier, action) =>
            {
            };
            
            var result = IConfigReferenceTreeExtensions.GetConfigBranch(tree, "MyConfig", "MyPlatform");
            
            Assert.That(result, Is.Empty);
        }
    }
}