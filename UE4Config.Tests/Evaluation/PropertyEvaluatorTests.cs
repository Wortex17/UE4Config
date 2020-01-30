using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        public class EvaluateInstructions
        {
            static InstructionToken NewInstructionValue(InstructionType type, string value)
            {
                return new InstructionToken(type, "TESTKEY", value);
            }

            static InstructionToken NewInstructionKey(InstructionType type, string key)
            {
                return new InstructionToken(type, key);
            }

            static InstructionToken NewInstruction(InstructionType type)
            {
                return new InstructionToken(type, "TESTKEY");
            }

            public static IEnumerable Cases_When_ValidInstructionChain
            {
                get
                {
                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Set, "A")
                            },
                            new string[] {"A"}
                        })
                        { TestName = "SetInitial" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Add, "A")
                            },
                            new string[] {"A"}
                        })
                        { TestName = "AddInitial" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.AddOverride, "A")
                            },
                            new string[] {"A"}
                        })
                        { TestName = "AddOverrideInitial" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Remove, "A")
                            },
                            new string[] {}
                        })
                        { TestName = "RemoveInitial" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstruction(InstructionType.RemoveAll)
                            },
                            new string[] {}
                        })
                        { TestName = "RemoveAllInitial" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Set, "")
                            },
                            new string[] {""}
                        })
                        { TestName = "SetInitialWhitespace" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Add, "")
                            },
                            new string[] {""}
                        })
                        { TestName = "AddInitialWhitespace" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Remove, "")
                            },
                            new string[] {}
                        })
                        { TestName = "RemoveInitialWhitespace" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Add, "A"),
                                NewInstructionValue(InstructionType.Add, "B")
                            },
                            new string[] {"A", "B"}
                        })
                        { TestName = "AddTwo" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Set, "A"),
                                NewInstructionValue(InstructionType.Add, "B")
                            },
                            new string[] {"A", "B"}
                        })
                        { TestName = "SetOne, AddOne" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Set, "A"),
                                NewInstructionValue(InstructionType.AddOverride, "B")
                            },
                            new string[] {"A", "B"}
                        })
                        { TestName = "SetOne, AddOverrideOne" };

                    yield return new TestCaseData(new object[]
                        {
                            new[]
                            {
                                NewInstructionValue(InstructionType.Set, "A"),
                                NewInstructionValue(InstructionType.Add, "B"),
                                NewInstructionValue(InstructionType.Add, "C")
                            },
                            new[] {"A", "B", "C"}
                        })
                        { TestName = "SetOne, AddTwo" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Set, "A"),
                                NewInstructionValue(InstructionType.Add, "B"),
                                NewInstructionValue(InstructionType.AddOverride, "C")
                            },
                            new string[] {"A", "B", "C"}
                        })
                        { TestName = "SetOne, AddOne, AddOverrideOne" };

                    yield return new TestCaseData(new object[]
                        {
                            new[]
                            {
                                NewInstructionValue(InstructionType.Set, "A"),
                                NewInstructionValue(InstructionType.Set, "B")
                            },
                            new[] {"B"}
                        })
                        { TestName = "SetTwo" };

                    yield return new TestCaseData(new object[]
                        {
                            new[]
                            {
                                NewInstructionValue(InstructionType.Set, "A"),
                                NewInstructionValue(InstructionType.Add, "B"),
                                NewInstructionValue(InstructionType.Set, "C")
                            },
                            new[] {"C"}
                        })
                        { TestName = "SetOne, AddOne, SetOne" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Add, "A"),
                                NewInstructionValue(InstructionType.Add, "B"),
                                NewInstructionValue(InstructionType.Remove, "B")
                            },
                            new string[] {"A"}
                        })
                        { TestName = "AddTwo, RemoveOne" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Add, "A"),
                                NewInstructionValue(InstructionType.Add, "B"),
                                NewInstructionValue(InstructionType.Remove, "A")
                            },
                            new string[] {"B"}
                        })
                        { TestName = "AddTwo, RemoveFirstOne" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Add, "A"),
                                NewInstructionValue(InstructionType.Add, "B"),
                                NewInstructionValue(InstructionType.Remove, "A"),
                                NewInstructionValue(InstructionType.Remove, "B")
                            },
                            new string[] {}
                        })
                        { TestName = "AddTwo, RemoveTwo" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Add, "A"),
                                NewInstructionValue(InstructionType.Add, "B"),
                                NewInstructionValue(InstructionType.Remove, "B"),
                                NewInstructionValue(InstructionType.Remove, "A")
                            },
                            new string[] {}
                        })
                        { TestName = "AddTwo, RemoveTwoInverseOrder" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Add, "A"),
                                NewInstructionValue(InstructionType.Add, "B"),
                                NewInstruction(InstructionType.RemoveAll)
                            },
                            new string[] {}
                        })
                        { TestName = "AddTwo, RemoveAll" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Add, "A"),
                                NewInstructionValue(InstructionType.Add, "B"),
                                NewInstruction(InstructionType.RemoveAll),
                                NewInstructionValue(InstructionType.Add, "C")
                            },
                            new string[] {"C"}
                        })
                        { TestName = "AddTwo, RemoveAll, AddOne" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Add, "A"),
                                NewInstructionValue(InstructionType.Add, "B"),
                                NewInstruction(InstructionType.RemoveAll),
                                NewInstructionValue(InstructionType.Add, "C"),
                                NewInstructionValue(InstructionType.Add, "D")
                            },
                            new string[] {"C", "D"}
                        })
                        { TestName = "AddTwo, RemoveAll, AddTwo" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Add, "A"),
                                NewInstructionValue(InstructionType.Add, "B"),
                                NewInstruction(InstructionType.RemoveAll),
                                NewInstructionValue(InstructionType.Add, "C"),
                                NewInstructionValue(InstructionType.Add, "A")
                            },
                            new string[] {"C", "A"}
                        })
                        { TestName = "AddTwo, RemoveAll, AddTwoIncludingOldOne" };

                    yield return new TestCaseData(new object[]
                        {
                            new InstructionToken[]
                            {
                                NewInstructionValue(InstructionType.Add, "A"),
                                NewInstructionValue(InstructionType.Add, "A"),
                                NewInstructionValue(InstructionType.Remove, "A")
                            },
                            new string[] {}
                        })
                        { TestName = "AddTwoDuplicates, Remove" };
                }
            }

            [TestCaseSource(nameof(Cases_When_ValidInstructionChain))]
            public void When_ValidInstructionChain(InstructionToken[] instructions, string[] expectedValues)
            {
                var evaluator = new PropertyEvaluator();
                var values = new List<string>();
                Assert.That(() => { evaluator.EvaluateInstructions(instructions, values); }, Throws.Nothing);
                Assert.That(values, Is.EquivalentTo(expectedValues));
            }

            [Test]
            public void When_InvalidInstructionInChain_Throws()
            {
                var evaluator = new PropertyEvaluator();
                var instructions = new List<InstructionToken>();
                instructions.Add(NewInstruction((InstructionType)999));
                var values = new List<string>();
                Assert.That(() => { evaluator.EvaluateInstructions(instructions, values); }, Throws.TypeOf<InvalidEnumArgumentException>());
            }
        }

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
                        "MyPropertyOnlyInA=SectionlessValueA",
                        "ListProperty=ValueA0",
                        "+ListProperty=ValueA1",
                        "+ListProperty=ValueA2",
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
                        "+ListProperty=ValueB1",
                        "+ListProperty=ValueB2",
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
                        "+ListProperty=ValueC1",
                        "+ListProperty=ValueC2",
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
                evaluator.EvaluatePropertyValues(configs, "MySection", "MyProperty", propertyValues);
                Assert.That(propertyValues, Is.EquivalentTo(new[] {$"Value{latestConfigId}"}));

                propertyValues.Clear();
                evaluator.EvaluatePropertyValues(configs, null, "MyProperty", propertyValues);
                Assert.That(propertyValues, Is.EquivalentTo(new[] { $"SectionlessValue{latestConfigId}" }));
            }

            [TestCaseSource(nameof(ConfigChains))]
            public void When_AddProperty(ConfigIni[] configs, string[] configIds)
            {
                var evaluator = new PropertyEvaluator();

                var propertyValues = new List<string>();
                var expectedValues = new List<string>();
                evaluator.EvaluatePropertyValues(configs, "MySection", "MyAddProperty", propertyValues);
                foreach (var configId in configIds)
                {
                    expectedValues.Add($"Value{configId}");
                }
                Assert.That(propertyValues, Is.EquivalentTo(expectedValues));

                propertyValues.Clear();
                expectedValues.Clear();
                evaluator.EvaluatePropertyValues(configs, null, "MyAddProperty", propertyValues);
                foreach (var configId in configIds)
                {
                    expectedValues.Add($"SectionlessValue{configId}");
                }
                Assert.That(propertyValues, Is.EquivalentTo(expectedValues));
            }

            [TestCaseSource(nameof(ConfigChains))]
            public void When_ListProperty(ConfigIni[] configs, string[] configIds)
            {
                var evaluator = new PropertyEvaluator();
                var latestConfigId = configIds[configIds.Length - 1];

                var propertyValues = new List<string>();
                var expectedValues = new List<string>();
                evaluator.EvaluatePropertyValues(configs, null, "ListProperty", propertyValues);
                foreach (var configId in configIds)
                {
                    if (expectedValues.Count == 0)
                    {
                        expectedValues.Add($"Value{configId}0");
                    }
                    expectedValues.Add($"Value{configId}1");
                    expectedValues.Add($"Value{configId}2");
                }
                Assert.That(propertyValues, Is.EquivalentTo(expectedValues));
            }

            [TestCaseSource(nameof(ConfigChains))]
            public void When_UnknownProperty(ConfigIni[] configs, string[] configIds)
            {
                var evaluator = new PropertyEvaluator();
                var latestConfigId = configIds[configIds.Length - 1];

                var propertyValues = new List<string>();
                evaluator.EvaluatePropertyValues(configs, "MySection", "UnknownProperty", propertyValues);
                Assert.That(propertyValues, Is.EquivalentTo(new string[] { }));

                propertyValues.Clear();
                evaluator.EvaluatePropertyValues(configs, null, "MyProperty", propertyValues);
                Assert.That(propertyValues, Is.EquivalentTo(new[] { $"SectionlessValue{latestConfigId}" }));
            }
        }
    }
}