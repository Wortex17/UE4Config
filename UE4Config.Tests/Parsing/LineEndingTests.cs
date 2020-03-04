using System;
using System.ComponentModel;
using System.IO;
using NUnit.Framework;
using UE4Config.Parsing;

namespace UE4Config.Tests.Parsing
{
    [TestFixture]
    class LineEndingTests
    {
        [TestFixture]
        class AsString
        {
            [TestCase(LineEnding.None, "")]
            [TestCase(LineEnding.Unix, "\n")]
            [TestCase(LineEnding.Windows, "\r\n")]
            [TestCase(LineEnding.Mac, "\r")]
            public void When_ValidEnum(LineEnding lineEnding, string expectedOutput)
            {
                Assert.That(lineEnding.AsString(), Is.EqualTo(expectedOutput));
            }

            [Test]
            public void When_Unknown_ResolvesToSystemDefault()
            {
                Assert.That(LineEnding.Unknown.AsString(), Is.EqualTo(Environment.NewLine));
            }

            [Test]
            public void When_InvalidEnum()
            {
                Assert.That(() => { ((LineEnding)999).AsString(); }, Throws.TypeOf<InvalidEnumArgumentException>());
            }
        }

        [TestFixture]
        class Write
        {
            [Test]
            public void When_Unknown_WritesWritersNewLine()
            {
                var writer = new StringWriter();
                LineEnding.Unknown.WriteTo(writer);
                Assert.That(writer.ToString(), Is.EqualTo(writer.NewLine));
            }

            [Test]
            public void When_UnknownWithModifiedWriter_WritesWritersNewLine()
            {
                var writer = new StringWriter();
                writer.NewLine = "CustomNewLine\t";
                LineEnding.Unknown.WriteTo(writer);
                Assert.That(writer.ToString(), Is.EqualTo(writer.NewLine));
            }

            [TestCase(LineEnding.None, "")]
            [TestCase(LineEnding.Unix, "\n")]
            [TestCase(LineEnding.Windows, "\r\n")]
            [TestCase(LineEnding.Mac, "\r")]
            public void When_SpecificLineEnding(LineEnding lineEnding, string expectedOutput)
            {
                var writer = new StringWriter();
                lineEnding.WriteTo(writer);
                Assert.That(writer.ToString(), Is.EqualTo(expectedOutput));
            }

            [Test]
            public void When_InvalidEnum()
            {
                var writer = new StringWriter();
                Assert.That(() => { ((LineEnding)999).WriteTo(writer); }, Throws.TypeOf<InvalidEnumArgumentException>());
            }
        }
    }
}
