using System.ComponentModel;
using NUnit.Framework;
using UE4Config.Parsing;

namespace UE4Config.Tests.Parsing
{
    [TestFixture]
    class InstructionTypeTests
    {
        [TestFixture]
        class AsPrefixString
        {
            [TestCase(InstructionType.Set, "")]
            [TestCase(InstructionType.Add, "+")]
            [TestCase(InstructionType.AddForce, ".")]
            [TestCase(InstructionType.Remove, "-")]
            [TestCase(InstructionType.RemoveAll, "!")]
            public void When_ValidEnum(InstructionType instructionType, string expectedOutput)
            {
                Assert.That(instructionType.AsPrefixString(), Is.EqualTo(expectedOutput));
            }

            [Test]
            public void When_InvalidEnum()
            {
                Assert.That(() => { ((InstructionType)999).AsPrefixString(); }, Throws.TypeOf<InvalidEnumArgumentException>());
            }
        }
    }
}
