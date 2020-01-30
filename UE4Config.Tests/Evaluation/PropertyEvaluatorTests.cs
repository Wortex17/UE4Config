using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UE4Config.Evaluation;
using UE4Config.Parser;

namespace UE4Config.Tests.Evaluation
{
    [TestFixture]
    public class PropertyEvaluatorTests
    {
        [TestFixture]
        public class EvaluatePropertyValues
        {
            public static IEnumerable ConfigChains
            {
                get
                {
                    var configA = new ConfigIni();
                    configA.Read(new StringReader(String.Join("\n", new[] {
                        ";MyProperty=CommentedValueA",
                        "MyProperty=SectionlessValueA",
                        "+MyAddProperty=SectionlessValueA",
                        "MyPropertyOnlyInA=SectionlessValueB",
                        "[MySection]",
                        "MyProperty=ValueA",
                        "+MyAddProperty=ValueA",
                        "MyPropertyOnlyInA=ValueA"
                    })));
                    var configB = new ConfigIni();
                    configB.Read(new StringReader(String.Join("\n", new[] {
                        ";MyProperty=CommentedValueB",
                        "MyProperty=SectionlessValueB",
                        "+MyAddProperty=SectionlessValueB",
                        "MyPropertyOnlyInB=SectionlessValueB",
                        "[MySection]",
                        "MyProperty=ValueB",
                        "+MyAddProperty=ValueB",
                        "MyPropertyOnlyInB=ValueB"
                    })));
                    var configC = new ConfigIni();
                    configC.Read(new StringReader(String.Join("\n", new[] {
                        ";MyProperty=CommentedValueC",
                        "MyProperty=SectionlessValueC",
                        "+MyAddProperty=SectionlessValueC",
                        "MyPropertyOnlyInC=SectionlessValueC",
                        "[MySection]",
                        "MyProperty=ValueC",
                        "+MyAddProperty=ValueC",
                        "MyPropertyOnlyInC=ValueC"
                    })));

                    yield return new TestCaseData(new object[] { new[] { configA, configB } , new[] {"A", "B"}});
                    yield return new TestCaseData(new object[] { new[] { configA, configB, configC }, new[] { "A", "B", "C" } });
                }
            }

            [TestCaseSource(nameof(ConfigChains))]
            public void When_SetProperty(ConfigIni[] configs, string[] configIds)
            {
                var evaluator = new PropertyEvaluator();
                var latestConfigId = configIds[configIds.Length - 1];

                var propertyValues = new List<string>();
                evaluator.EvaluatePropertyValues(configs, "MySection", "MyProperty", new List<InstructionToken>(), propertyValues);
                Assert.That(propertyValues, Is.EquivalentTo(new[] {$"Value{latestConfigId}"}));

                propertyValues.Clear();
                evaluator.EvaluatePropertyValues(configs, null, "MyProperty", new List<InstructionToken>(), propertyValues);
                Assert.That(propertyValues, Is.EquivalentTo(new[] { $"SectionlessValue{latestConfigId}" }));
            }

            [TestCaseSource(nameof(ConfigChains))]
            public void When_AddProperty(ConfigIni[] configs, string[] configIds)
            {
                var evaluator = new PropertyEvaluator();
                var latestConfigId = configIds[configIds.Length - 1];

                var propertyValues = new List<string>();
                var expectedValues = new List<string>();
                evaluator.EvaluatePropertyValues(configs, "MySection", "MyAddProperty", new List<InstructionToken>(), propertyValues);
                foreach (var configId in configIds)
                {
                    expectedValues.Add($"Value{configId}");
                }
                Assert.That(propertyValues, Is.EquivalentTo(expectedValues));

                propertyValues.Clear();
                expectedValues.Clear();
                evaluator.EvaluatePropertyValues(configs, null, "MyAddProperty", new List<InstructionToken>(), propertyValues);
                foreach (var configId in configIds)
                {
                    expectedValues.Add($"SectionlessValue{configId}");
                }
                Assert.That(propertyValues, Is.EquivalentTo(expectedValues));
            }

            [TestCaseSource(nameof(ConfigChains))]
            public void When_UnknownProperty(ConfigIni[] configs, string[] configIds)
            {
                var evaluator = new PropertyEvaluator();
                var latestConfigId = configIds[configIds.Length - 1];

                var propertyValues = new List<string>();
                evaluator.EvaluatePropertyValues(configs, "MySection", "UnknownProperty", new List<InstructionToken>(), propertyValues);
                Assert.That(propertyValues, Is.EquivalentTo(new string[] { }));

                propertyValues.Clear();
                evaluator.EvaluatePropertyValues(configs, null, "MyProperty", new List<InstructionToken>(), propertyValues);
                Assert.That(propertyValues, Is.EquivalentTo(new[] { $"SectionlessValue{latestConfigId}" }));
            }
        }
    }
}