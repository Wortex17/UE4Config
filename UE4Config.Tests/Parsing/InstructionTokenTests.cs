using System;
using System.Collections;
using NUnit.Framework;
using UE4Config.Parsing;

namespace UE4Config.Tests.Parsing
{
    [TestFixture]
    class InstructionTokenTests
    {
        [TestFixture]
        class Constructor
        {
            [Test]
            public void When_WithoutArguments()
            {
                var instruction = new InstructionToken();
                Assert.That(instruction.InstructionType, Is.EqualTo(InstructionType.Set));
                Assert.That(instruction.Key, Is.Null);
                Assert.That(instruction.Value, Is.Null);
                Assert.That(instruction.LineEnding, Is.EqualTo(LineEnding.Unknown));
            }

            static IEnumerable Cases_When_WithTypeAndKey
            {
                get
                {
                    var instructionTypes = (InstructionType[])Enum.GetValues(typeof(InstructionType));
                    foreach (var instructionType in instructionTypes)
                    {
                        yield return new TestCaseData(new object[] { instructionType });
                    }
                }
            }

            [TestCaseSource(nameof(Cases_When_WithTypeAndKey))]
            public void When_WithTypeAndKey(InstructionType type)
            {
                var instruction = new InstructionToken(type, "key");
                Assert.That(instruction.InstructionType, Is.EqualTo(type));
                Assert.That(instruction.Key, Is.EqualTo("key"));
                Assert.That(instruction.Value, Is.Null);
                Assert.That(instruction.LineEnding, Is.EqualTo(LineEnding.Unknown));
            }

            [TestCaseSource(nameof(Cases_When_WithTypeAndKey))]
            public void When_WithTypeKeyAndValue(InstructionType type)
            {
                var instruction = new InstructionToken(type, "key", "value");
                Assert.That(instruction.InstructionType, Is.EqualTo(type));
                Assert.That(instruction.Key, Is.EqualTo("key"));
                Assert.That(instruction.Value, Is.EqualTo("value"));
                Assert.That(instruction.LineEnding, Is.EqualTo(LineEnding.Unknown));
            }

            static IEnumerable Cases_When_WithTypeKeyAndLineEnding
            {
                get
                {
                    var instructionTypes = (InstructionType[])Enum.GetValues(typeof(InstructionType));
                    var lineEndings = (LineEnding[])Enum.GetValues(typeof(LineEnding));
                    foreach (var instructionType in instructionTypes)
                    {
                        foreach (var lineEnding in lineEndings)
                        {
                            yield return new TestCaseData(new object[] { instructionType, lineEnding });
                        }
                    }
                }
            }

            [TestCaseSource(nameof(Cases_When_WithTypeKeyAndLineEnding))]
            public void When_WithTypeKeyAndLineEnding(InstructionType type, LineEnding lineEnding)
            {
                var instruction = new InstructionToken(type, "key", lineEnding);
                Assert.That(instruction.InstructionType, Is.EqualTo(type));
                Assert.That(instruction.Key, Is.EqualTo("key"));
                Assert.That(instruction.Value, Is.Null);
                Assert.That(instruction.LineEnding, Is.EqualTo(lineEnding));
            }

            [TestCaseSource(nameof(Cases_When_WithTypeKeyAndLineEnding))]
            public void When_WithTypeKeyLineEndingAndValue(InstructionType type, LineEnding lineEnding)
            {
                var instruction = new InstructionToken(type, "key", "value", lineEnding);
                Assert.That(instruction.InstructionType, Is.EqualTo(type));
                Assert.That(instruction.Key, Is.EqualTo("key"));
                Assert.That(instruction.Value, Is.EqualTo("value"));
                Assert.That(instruction.LineEnding, Is.EqualTo(lineEnding));
            }
        }
    }
}
