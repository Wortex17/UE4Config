using System;
using System.Collections;
using System.IO;
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
                Assert.That(token.Lines, Is.EquivalentTo(new []{ Environment.NewLine }));
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
                Assert.That(token.Lines, Is.EquivalentTo(new[] { Environment.NewLine }));
            }

            [TestCase("\n")] //Unix
            [TestCase("\r\n")] //Windows
            [TestCase("\r")] //Mac
            public void When_HasOneLine_WithCustomNewline(string newline)
            {
                var token = new WhitespaceToken();
                token.Lines.Add("  ");

                Assert.That(() => token.Condense(newline), Throws.Nothing);

                Assert.That(token.Lines, Has.Count.EqualTo(1));
                Assert.That(token.Lines, Is.EquivalentTo(new[] { newline }));
            }

            [TestCase("\n")] //Unix
            [TestCase("\r\n")] //Windows
            [TestCase("\r")] //Mac
            public void When_HasMultipleLines_WithCustomNewline(string newline)
            {
                var token = new WhitespaceToken();
                token.Lines.Add("    ");
                token.Lines.Add("       ");
                token.Lines.Add("   \t ");

                Assert.That(() => token.Condense(newline), Throws.Nothing);

                Assert.That(token.Lines, Has.Count.EqualTo(1));
                Assert.That(token.Lines, Is.EquivalentTo(new[] { newline }));
            }
        }
    }
}
