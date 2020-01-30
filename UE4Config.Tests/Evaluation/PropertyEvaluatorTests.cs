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
                        "MyPropertyOnlyInA=SectionlessValueB",
                        "[MySection]",
                        "MyProperty=ValueA",
                        "MyPropertyOnlyInA=ValueA"
                    })));
                    var configB = new ConfigIni();
                    configB.Read(new StringReader(String.Join("\n", new[] {
                        ";MyProperty=CommentedValueB",
                        "MyProperty=SectionlessValueB",
                        "MyPropertyOnlyInB=SectionlessValueB",
                        "[MySection]",
                        "MyProperty=ValueB",
                        "MyPropertyOnlyInB=ValueB"
                    })));
                    var configC = new ConfigIni();
                    configC.Read(new StringReader(String.Join("\n", new[] {
                        ";MyProperty=CommentedValueC",
                        "MyProperty=SectionlessValueC",
                        "MyPropertyOnlyInC=SectionlessValueC",
                        "[MySection]",
                        "MyProperty=ValueC",
                        "MyPropertyOnlyInC=ValueC"
                    })));

                    yield return new TestCaseData(new object[] { new ConfigIni[] { configA, configB } });
                }
            }

            [TestCaseSource(nameof(ConfigChains))]
            public void When_SetPropertyInNamedSection(ConfigIni[] configs)
            {
                var evaluator = new PropertyEvaluator();

                var propertyValues = new List<string>();
                evaluator.EvaluatePropertyValues(configs, "MySection", "MyProperty", new List<InstructionToken>(), propertyValues);
                Assert.That(propertyValues, Is.EquivalentTo(new[] {"ValueB"}));

                propertyValues.Clear();
                evaluator.EvaluatePropertyValues(configs, null, "MyProperty", new List<InstructionToken>(), propertyValues);
                Assert.That(propertyValues, Is.EquivalentTo(new[] { "SectionlessValueB" }));
            }

            [TestCaseSource(nameof(ConfigChains))]
            public void When_SetProperty(ConfigIni[] configs)
            {
                var evaluator = new PropertyEvaluator();

                var propertyValues = new List<string>();
                evaluator.EvaluatePropertyValues(configs, null, "MyProperty", new List<InstructionToken>(), propertyValues);
                Assert.That(propertyValues, Is.EquivalentTo(new[] { "SectionlessValueB" }));
            }
        }
    }
}