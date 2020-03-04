using NUnit.Framework;
using UE4Config.Hierarchy;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class ConfigHierarchyLevelTests
    {
        [TestFixture]
        public class Extensions
        {
            [TestCase(ConfigHierarchyLevel.Base)]
            [TestCase(ConfigHierarchyLevel.ProjectCategory)]
            [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void AndLower(ConfigHierarchyLevel level)
            {
                Assert.That(level.AndLower(), Is.EqualTo(ConfigHierarchyLevelRange.AnyTo(level)));
            }

            [TestCase(ConfigHierarchyLevel.Base)]
            [TestCase(ConfigHierarchyLevel.ProjectCategory)]
            [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void AndHigher(ConfigHierarchyLevel level)
            {
                Assert.That(level.AndHigher(), Is.EqualTo(ConfigHierarchyLevelRange.AnyFrom(level)));
            }

            [TestCase(ConfigHierarchyLevel.Base)]
            [TestCase(ConfigHierarchyLevel.ProjectCategory)]
            [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void Exact(ConfigHierarchyLevel level)
            {
                Assert.That(level.Exact(), Is.EqualTo(ConfigHierarchyLevelRange.Exact(level)));
            }

            [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.BaseCategory)]
            [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.ProjectCategory)]
            [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.Base)]
            public void To(ConfigHierarchyLevel level, ConfigHierarchyLevel to)
            {
                Assert.That(level.To(to), Is.EqualTo(ConfigHierarchyLevelRange.FromTo(level, to)));
            }
        }
    }
}