using NUnit.Framework;
using UE4Config.Hierarchy;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class ConfigHierarchyLevelRangeTests
    {
        [TestFixture]
        public class Constructors
        {
            [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.BaseCategory)]
            [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_OrderIsCorrect(ConfigHierarchyLevel from, ConfigHierarchyLevel to)
            {
                var range = new ConfigHierarchyLevelRange(from, to);

                Assert.That(range.From, Is.EqualTo(from));
                Assert.That(range.To, Is.EqualTo(to));
                Assert.That(range.IncludesAnything, Is.True);
                Assert.That(range.HasTo, Is.True);
                Assert.That(range.HasFrom, Is.True);
            }

            [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.Base)]
            [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.Base)]
            public void When_OrderIsFlipped(ConfigHierarchyLevel from, ConfigHierarchyLevel to)
            {
                var range = new ConfigHierarchyLevelRange(from, to);

                Assert.That(range.From, Is.EqualTo(to));
                Assert.That(range.To, Is.EqualTo(from));
                Assert.That(range.IncludesAnything, Is.True);
                Assert.That(range.HasTo, Is.True);
                Assert.That(range.HasFrom, Is.True);
            }

            [TestCase(ConfigHierarchyLevel.BaseCategory)]
            [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void When_IsSingleLevel(ConfigHierarchyLevel level)
            {
                var range = new ConfigHierarchyLevelRange(level, level);

                Assert.That(range.From, Is.EqualTo(level));
                Assert.That(range.To, Is.EqualTo(range.From));
                Assert.That(range.IncludesAnything, Is.True);
                Assert.That(range.HasTo, Is.True);
                Assert.That(range.HasFrom, Is.True);
            }

            [Test]
            public void When_IsEmpty_DoesntIncludeAnything()
            {
                var range = new ConfigHierarchyLevelRange();
                
                Assert.That(range.IncludesAnything, Is.False);
                Assert.That(range.HasTo, Is.False);
                Assert.That(range.HasFrom, Is.False);
            }

            [TestFixture]
            public class UtilityConstructors
            {
                [Test]
                public void All()
                {
                    var range = ConfigHierarchyLevelRange.All();

                    Assert.That(range.IncludesAnything, Is.True);
                    Assert.That(range.HasTo, Is.False);
                    Assert.That(range.HasFrom, Is.False);
                }

                [Test]
                public void None()
                {
                    var range = ConfigHierarchyLevelRange.None();

                    Assert.That(range.IncludesAnything, Is.False);
                    Assert.That(range.HasTo, Is.False);
                    Assert.That(range.HasFrom, Is.False);
                }

                [TestCase(ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory)]
                public void AnyFrom(ConfigHierarchyLevel level)
                {
                    var range = ConfigHierarchyLevelRange.AnyFrom(level);

                    Assert.That(range.IncludesAnything, Is.True);
                    Assert.That(range.HasTo, Is.False);
                    Assert.That(range.HasFrom, Is.True);
                    Assert.That(range.From, Is.EqualTo(level));
                }

                [TestCase(ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory)]
                public void AnyTo(ConfigHierarchyLevel level)
                {
                    var range = ConfigHierarchyLevelRange.AnyTo(level);

                    Assert.That(range.IncludesAnything, Is.True);
                    Assert.That(range.HasTo, Is.True);
                    Assert.That(range.To, Is.EqualTo(level));
                    Assert.That(range.HasFrom, Is.False);
                }
            }
        }

        [TestFixture]
        public class Includes
        {
            [TestCase(ConfigHierarchyLevel.Base)]
            [TestCase(ConfigHierarchyLevel.BaseCategory)]
            [TestCase(ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase(ConfigHierarchyLevel.ProjectCategory)]
            [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void All(ConfigHierarchyLevel test)
            {
                Assert.That(ConfigHierarchyLevelRange.All().Includes(test), Is.True);
            }

            [TestCase(ConfigHierarchyLevel.Base)]
            [TestCase(ConfigHierarchyLevel.BaseCategory)]
            [TestCase(ConfigHierarchyLevel.BasePlatformCategory)]
            [TestCase(ConfigHierarchyLevel.ProjectCategory)]
            [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory)]
            public void None(ConfigHierarchyLevel test)
            {
                Assert.That(ConfigHierarchyLevelRange.None().Includes(test), Is.False);
            }

            [TestFixture]
            public class AnyFrom
            {
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.ProjectPlatformCategory)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                public void When_Positive(ConfigHierarchyLevel from, ConfigHierarchyLevel test)
                {
                    Assert.That(ConfigHierarchyLevelRange.AnyFrom(from).Includes(test), Is.True);
                }

                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.ProjectCategory)]
                public void When_Negative(ConfigHierarchyLevel from, ConfigHierarchyLevel test)
                {
                    Assert.That(ConfigHierarchyLevelRange.AnyFrom(from).Includes(test), Is.False);
                }
            }

            [TestFixture]
            public class AnyTo
            {
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                public void When_Positive(ConfigHierarchyLevel to, ConfigHierarchyLevel test)
                {
                    Assert.That(ConfigHierarchyLevelRange.AnyTo(to).Includes(test), Is.True);
                }

                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.ProjectPlatformCategory)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                public void When_Negative(ConfigHierarchyLevel to, ConfigHierarchyLevel test)
                {
                    Assert.That(ConfigHierarchyLevelRange.AnyTo(to).Includes(test), Is.False);
                }
            }

            [TestFixture]
            public class Exact
            {
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                public void When_Positive(ConfigHierarchyLevel level, ConfigHierarchyLevel test)
                {
                    Assert.That(ConfigHierarchyLevelRange.Exact(level).Includes(test), Is.True);
                }

                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.Base, ConfigHierarchyLevel.ProjectPlatformCategory)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.ProjectCategory)]
                [TestCase(ConfigHierarchyLevel.BasePlatformCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.ProjectPlatformCategory, ConfigHierarchyLevel.ProjectCategory)]
                public void When_Negative(ConfigHierarchyLevel level, ConfigHierarchyLevel test)
                {
                    Assert.That(ConfigHierarchyLevelRange.Exact(level).Includes(test), Is.False);
                }
            }

            [TestFixture]
            public class FromTo
            {
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.BaseCategory)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.BasePlatformCategory)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.ProjectCategory)]
                public void When_Positive(ConfigHierarchyLevel from, ConfigHierarchyLevel to, ConfigHierarchyLevel test)
                {
                    Assert.That(ConfigHierarchyLevelRange.FromTo(from, to).Includes(test), Is.True);
                }

                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.Base)]
                [TestCase(ConfigHierarchyLevel.BaseCategory, ConfigHierarchyLevel.ProjectCategory, ConfigHierarchyLevel.ProjectPlatformCategory)]
                public void When_Negative(ConfigHierarchyLevel from, ConfigHierarchyLevel to, ConfigHierarchyLevel test)
                {
                    Assert.That(ConfigHierarchyLevelRange.FromTo(from, to).Includes(test), Is.False);
                }
            }
        }
    }
}