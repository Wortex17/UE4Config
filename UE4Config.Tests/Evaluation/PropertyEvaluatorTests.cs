using System;
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
            [Test]
            public void When_ValidConfigChain()
            {
                var evaluator = new PropertyEvaluator();
                var configA = new ConfigIni();
                configA.Read(new StringReader(String.Join("\n", new[] {
                    ";MyProperty=CommentedValueA",
                    "MyProperty=SectionlessValueA",
                    "[MySection]",
                    "MyProperty=ValueA"
                })));
                var configB = new ConfigIni();
                configB.Read(new StringReader(String.Join("\n", new[] {
                    ";MyProperty=CommentedValueB",
                    "MyProperty=SectionlessValueB",
                    "[MySection]",
                    "MyProperty=ValueB"
                })));

                var configs = new List<ConfigIni>() {configA, configB};
                var propertyValues = new List<string>();
                evaluator.EvaluatePropertyValues(configs, "MySection", "MyProperty", new List<InstructionToken>(), propertyValues);

                Assert.That(propertyValues, Is.EquivalentTo(new[] {"ValueB"}));
            }
        }
    }
}