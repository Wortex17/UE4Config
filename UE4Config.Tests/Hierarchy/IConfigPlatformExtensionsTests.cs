using System.Collections.Generic;
using NUnit.Framework;
using UE4Config.Hierarchy;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class IConfigPlatformExtensionsTests
    {
        [Test]
        public void ResolvePlatformInheritance_WithoutInheritance()
        {
            var platform = new ConfigPlatform("MyPlatform");
            var result = new List<IConfigPlatform>();
            var expectedResult = new List<IConfigPlatform>()
            {
                platform
            };
            
            IConfigPlatformExtensions.ResolvePlatformInheritance(platform, ref result);
            
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }
        
        [Test]
        public void ResolvePlatformInheritance_WithInheritance()
        {
            var grandParentPlatform = new ConfigPlatform("MyPlatformGrandParent");
            var parentPlatform = new ConfigPlatform("MyPlatformParent", grandParentPlatform);
            var platform = new ConfigPlatform("MyPlatform", parentPlatform);
            var result = new List<IConfigPlatform>();
            var expectedResult = new List<IConfigPlatform>()
            {
                grandParentPlatform,
                parentPlatform,
                platform
            };
            
            IConfigPlatformExtensions.ResolvePlatformInheritance(platform, ref result);
            
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }
    }
}