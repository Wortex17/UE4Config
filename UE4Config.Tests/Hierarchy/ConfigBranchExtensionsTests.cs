using System;
using System.Collections.Generic;
using NUnit.Framework;
using UE4Config.Hierarchy;
using UE4Config.Parsing;

namespace UE4Config.Tests.Hierarchy
{
    [TestFixture]
    public class ConfigBranchExtensionsTests
    {
        [TestFixture]
        public class SelectHeadConfigReference
        {
            [Test]
            public void When_EmptyBranch()
            {
                var branch = new List<ConfigFileReference>()
                {
                    
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Engine);
            
                Assert.That(result, Is.EqualTo(ConfigFileReference.None));
            }
            
            [Test]
            public void When_EmptyDomain()
            {
                var type = "MyConfig";
                var branch = new List<ConfigFileReference>()
                {
                    new ConfigFileReference(ConfigDomain.Project, null, type),
                    new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type)
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Engine);
            
                Assert.That(result, Is.EqualTo(ConfigFileReference.None));
            }
            
            [Test]
            public void When_HasDomain()
            {
                var type = "MyConfig";
                var branch = new List<ConfigFileReference>()
                {
                    new ConfigFileReference(ConfigDomain.Project, null, type),
                    new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type)
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project);
            
                Assert.That(result, Is.EqualTo(branch[0]));
            }
            
            [Test]
            public void When_HasDomainAndPlatform()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var branch = new List<ConfigFileReference>()
                {
                    new ConfigFileReference(ConfigDomain.Project, null, type),
                    new ConfigFileReference(ConfigDomain.Project, platform, type),
                    new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type)
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project);
            
                Assert.That(result, Is.EqualTo(branch[1]));
            }
            
            [Test]
            public void When_HasDomainAndPlatform_NoneOrAnyPlatform()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var branch = new List<ConfigFileReference>()
                {
                    new ConfigFileReference(ConfigDomain.Project, null, type),
                    new ConfigFileReference(ConfigDomain.Project, platform, type),
                    new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type)
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.NoneOrAny);
            
                Assert.That(result, Is.EqualTo(branch[1]));
            }
            
            [Test]
            public void When_HasDomainAndPlatform_NonePlatform()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var branch = new List<ConfigFileReference>()
                {
                    new ConfigFileReference(ConfigDomain.Project, null, type),
                    new ConfigFileReference(ConfigDomain.Project, platform, type),
                    new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type)
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.None);
            
                Assert.That(result, Is.EqualTo(branch[0]));
            }
            
            [Test]
            public void When_HasDomainAndPlatform_AnyPlatform()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var branch = new List<ConfigFileReference>()
                {
                    new ConfigFileReference(ConfigDomain.Project, null, type),
                    new ConfigFileReference(ConfigDomain.Project, platform, type),
                    new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type)
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.Any);
            
                Assert.That(result, Is.EqualTo(branch[1]));
            }
            
            [Test]
            public void When_HasDomainAndMultiPlatform_AnyPlatform()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var platform2 = new ConfigPlatform("MyPlatform2");
                var branch = new List<ConfigFileReference>()
                {
                    new ConfigFileReference(ConfigDomain.Project, null, type),
                    new ConfigFileReference(ConfigDomain.Project, platform, type),
                    new ConfigFileReference(ConfigDomain.Project, platform2, type),
                    new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type)
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.Any);
            
                Assert.That(result, Is.EqualTo(branch[2]));
            }
            
            [Test]
            public void When_HasDomainAndMultiPlatform_SpecificPlatform_Unspecified()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var platform2 = new ConfigPlatform("MyPlatform2");
                var branch = new List<ConfigFileReference>()
                {
                    new ConfigFileReference(ConfigDomain.Project, null, type),
                    new ConfigFileReference(ConfigDomain.Project, platform, type),
                    new ConfigFileReference(ConfigDomain.Project, platform2, type),
                    new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type)
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.Specific);
            
                Assert.That(result, Is.EqualTo(ConfigFileReference.None));
            }
            
            [Test]
            public void When_HasDomainAndMultiPlatform_SpecificPlatform_Unmatching()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var platform2 = new ConfigPlatform("MyPlatform2");
                var branch = new List<ConfigFileReference>()
                {
                    new ConfigFileReference(ConfigDomain.Project, null, type),
                    new ConfigFileReference(ConfigDomain.Project, platform, type),
                    new ConfigFileReference(ConfigDomain.Project, platform2, type),
                    new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type)
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.Specific, "OtherPlatform");
            
                Assert.That(result, Is.EqualTo(ConfigFileReference.None));
            }
            
            [Test]
            public void When_HasDomainAndMultiPlatform_SpecificPlatform_Matching()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var platform2 = new ConfigPlatform("MyPlatform2");
                var branch = new List<ConfigFileReference>()
                {
                    new ConfigFileReference(ConfigDomain.Project, null, type),
                    new ConfigFileReference(ConfigDomain.Project, platform, type),
                    new ConfigFileReference(ConfigDomain.Project, platform2, type),
                    new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type)
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.Specific, "MyPlatform");
            
                Assert.That(result, Is.EqualTo(branch[1]));
            }
        }
        
        [TestFixture]
        public class SelectHeadConfigIni
        {
            [Test]
            public void When_EmptyBranch()
            {
                var branch = new List<ConfigIni>()
                {
                    
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Engine);
            
                Assert.That(result, Is.Null);
            }
            
            [Test]
            public void When_EmptyDomain()
            {
                var type = "MyConfig";
                var branch = new List<ConfigIni>()
                {
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, null, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type))
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Engine);
            
                Assert.That(result, Is.Null);
            }
            
            [Test]
            public void When_HasDomain()
            {
                var type = "MyConfig";
                var branch = new List<ConfigIni>()
                {
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, null, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type))
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project);
            
                Assert.That(result, Is.EqualTo(branch[0]));
            }
            
            [Test]
            public void When_HasDomainAndPlatform()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var branch = new List<ConfigIni>()
                {
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, null, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, platform, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type))
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project);
            
                Assert.That(result, Is.EqualTo(branch[1]));
            }
            
            [Test]
            public void When_HasDomainAndPlatform_NoneOrAnyPlatform()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var branch = new List<ConfigIni>()
                {
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, null, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, platform, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type))
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.NoneOrAny);
            
                Assert.That(result, Is.EqualTo(branch[1]));
            }
            
            [Test]
            public void When_HasDomainAndPlatform_NonePlatform()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var branch = new List<ConfigIni>()
                {
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, null, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, platform, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type))
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.None);
            
                Assert.That(result, Is.EqualTo(branch[0]));
            }
            
            [Test]
            public void When_HasDomainAndPlatform_AnyPlatform()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var branch = new List<ConfigIni>()
                {
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, null, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, platform, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type))
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.Any);
            
                Assert.That(result, Is.EqualTo(branch[1]));
            }
            
            [Test]
            public void When_HasDomainAndMultiPlatform_AnyPlatform()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var platform2 = new ConfigPlatform("MyPlatform2");
                var branch = new List<ConfigIni>()
                {
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, null, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, platform, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, platform2, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type))
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.Any);
            
                Assert.That(result, Is.EqualTo(branch[2]));
            }
            
            [Test]
            public void When_HasDomainAndMultiPlatform_SpecificPlatform_Unspecified()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var platform2 = new ConfigPlatform("MyPlatform2");
                var branch = new List<ConfigIni>()
                {
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, null, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, platform, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, platform2, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type))
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.Specific);
            
                Assert.That(result, Is.Null);
            }
            
            [Test]
            public void When_HasDomainAndMultiPlatform_SpecificPlatform_Unmatching()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var platform2 = new ConfigPlatform("MyPlatform2");
                var branch = new List<ConfigIni>()
                {
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, null, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, platform, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, platform2, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type))
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.Specific, "OtherPlatform");
            
                Assert.That(result, Is.Null);
            }
            
            [Test]
            public void When_HasDomainAndMultiPlatform_SpecificPlatform_Matching()
            {
                var type = "MyConfig";
                var platform = new ConfigPlatform("MyPlatform");
                var platform2 = new ConfigPlatform("MyPlatform2");
                var branch = new List<ConfigIni>()
                {
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, null, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, platform, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.Project, platform2, type)),
                    new ConfigIni(type, new ConfigFileReference(ConfigDomain.ProjectGenerated, null, type))
                };

                var result = ConfigBranchExtensions.SelectHeadConfig(branch, ConfigDomain.Project, ConfigBranchPlatformSelector.Specific, "MyPlatform");
            
                Assert.That(result, Is.EqualTo(branch[1]));
            }
        }
    }
}