using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UE4Config.Parsing;

namespace UE4Config.Tests.Parsing
{
    [TestFixture]
    class LineTokenTests
    {
        static TextToken CreateTextToken(string text)
        {
            return new TextToken() {Text = text};
        }

        static TextToken CreateTextToken(string text, LineEnding lineEnding)
        {
            return new TextToken() { Text = text, LineEnding = lineEnding};
        }

        public static IEnumerable Cases_Write_TextTokens
        {
            get
            {
                yield return new TestCaseData(new object[] { CreateTextToken("foobar"), "foobar" + Environment.NewLine }).SetName("TextToken Unspecified");
                yield return new TestCaseData(new object[] { CreateTextToken("foobar", LineEnding.Unknown), "foobar" + Environment.NewLine }).SetName("TextToken Unknown");
                yield return new TestCaseData(new object[] { CreateTextToken("foobar", LineEnding.None), "foobar"}).SetName("TextToken None");
                yield return new TestCaseData(new object[] { CreateTextToken("foobar", LineEnding.Unix), "foobar\n" }).SetName("TextToken Unix");
                yield return new TestCaseData(new object[] { CreateTextToken("foobar", LineEnding.Windows), "foobar\r\n" }).SetName("TextToken Windows");
                yield return new TestCaseData(new object[] { CreateTextToken("foobar", LineEnding.Mac), "foobar\r" }).SetName("TextToken Mac");
            }
        }

        [TestCaseSource(nameof(Cases_Write_TextTokens))]
        public void Write(LineToken token, string expectedText)
        {
            var writer = new StringWriter();
            token.Write(writer);
            Assert.That(writer.ToString(), Is.EqualTo(expectedText));
        }
    }
}
