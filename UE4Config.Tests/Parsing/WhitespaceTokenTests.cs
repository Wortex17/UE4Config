using System;
using NUnit.Framework;
using UE4Config.Parsing;

namespace UE4Config.Tests.Parsing
{
    [TestFixture]
    class WhitespaceTokenTests
    {
        [TestFixture]
        class Condense
        {
            [Test]
            public void When_HasNoLines_DoesNotAdd()
            {
                var token = new WhitespaceToken();

                Assert.That(() => token.Condense(), Throws.Nothing);

                Assert.That(token.Lines, Has.Count.EqualTo(0));
            }

            [Test]
            public void When_HasOneLine()
            {
                var token = new WhitespaceToken();
                token.Lines.Add("    ");

                Assert.That(() => token.Condense(), Throws.Nothing);

                Assert.That(token.Lines, Has.Count.EqualTo(1));
                Assert.That(token.GetStringLines(), Is.EquivalentTo(new []{ string.Empty }));
            }

            [Test]
            public void When_HasMultipleLines()
            {
                var token = new WhitespaceToken();
                token.Lines.Add("    ");
                token.Lines.Add("       ");
                token.Lines.Add("   \t ");

                Assert.That(() => token.Condense(), Throws.Nothing);

                Assert.That(token.Lines, Has.Count.EqualTo(1));
                Assert.That(token.GetStringLines(), Is.EquivalentTo(new[] { string.Empty }));
            }
        }
    }
}
