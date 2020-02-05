using System.ComponentModel;
using NUnit.Framework;
using UE4Config.Hierarchy;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class FileConfigHierarchyTest
    {
        private class MockFileConfigHierarchy : FileConfigHierarchy
        {
            public MockFileConfigHierarchy(string projectPath, string enginePath) : base(projectPath, enginePath)
            {
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
    }
}