using System;
using System.Collections;
using NUnit.Framework;
using UE4Config.Parser;

namespace UE4Config.Tests.Parser
{
    [TestFixture]
    class ConfigIniTests
    {
        [TestFixture]
        class ReadLine
        {
            IniToken AssertNAddedTokenToSection(ConfigIni configIni, ConfigIniSection expectedSection, int expectedTokens)
            {
                Assert.That(configIni.Sections, Has.Count.EqualTo(1));
                Assert.That(configIni.Sections[0], Is.SameAs(expectedSection));
                Assert.That(configIni.Sections[0].Tokens, Has.Count.EqualTo(expectedTokens));
                for (int tokenI = 0; tokenI < expectedTokens; tokenI++)
                {
                    Assert.That(configIni.Sections[0].Tokens[tokenI], Is.Not.Null);
                }
                return configIni.Sections[0].Tokens[expectedTokens-1];
            }
            TToken AssertNAddedTokenToSection<TToken>(ConfigIni configIni, ConfigIniSection expectedSection, int expectedTokens)
                where TToken : IniToken
            {
                var token = AssertNAddedTokenToSection(configIni, expectedSection, expectedTokens);
                Assert.That(token, Is.TypeOf<TToken>());
                return token as TToken;
            }

            IniToken AssertSingleAddedTokenToSection(ConfigIni configIni, ConfigIniSection expectedSection)
            {
                return AssertNAddedTokenToSection(configIni, expectedSection, 1);
            }

            TToken AssertSingleAddedTokenToSection<TToken>(ConfigIni configIni, ConfigIniSection expectedSection)
                where TToken : IniToken
            {
                return AssertNAddedTokenToSection<TToken>(configIni, expectedSection, 1);
            }

            [TestCase(null)]
            public void When_LineIsNull_DoesNothing(string line)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);

                Assert.That(configIni.Sections, Has.Count.EqualTo(1));
                Assert.That(configIni.Sections[0], Is.SameAs(currentSection));
                Assert.That(configIni.Sections[0].Tokens, Is.Empty);
            }

            [TestCase("+")]
            [TestCase("-")]
            [TestCase(".")]
            [TestCase("!")]
            [TestCase("text")]
            [TestCase("1234")]
            [TestCase("Key")]
            [TestCase("+Key")]
            [TestCase(".Key")]
            [TestCase("-Key")]
            public void When_LineIsInvalid_TreatsAsText(string line)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);

                var tokenT = AssertSingleAddedTokenToSection<TextToken>(configIni, currentSection);
                Assert.That(tokenT.Text, Is.EqualTo(line));
            }

            [TestCase("")]
            [TestCase(" ")]
            [TestCase("\t")]
            [TestCase(" \t")]
            public void When_LineIsWhitespace(string line)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);

                var tokenT = AssertSingleAddedTokenToSection<WhitespaceToken>(configIni, currentSection);
                Assert.That(tokenT.Lines, Has.Count.EqualTo(1));
                Assert.That(tokenT.Lines[0], Is.EqualTo(line));
            }

            public static IEnumerable RepeatedWhitespaceTestCases
            {
                get
                {
                    yield return new TestCaseData(new[] { new[] { "", "" }});
                    yield return new TestCaseData(new[] { new[] { " ", " " }});
                    yield return new TestCaseData(new[] { new[] { "\t", "\t" }});
                    yield return new TestCaseData(new[] { new[] { "", " " }});
                    yield return new TestCaseData(new[] { new[] { "", " ", "\t" }});
                    yield return new TestCaseData(new[] { new[] { "\t", " ", "" }});
                    yield return new TestCaseData(new[] { new[] { " ", "\t", "" }});
                }
            }

            [TestCaseSource(nameof(RepeatedWhitespaceTestCases))]
            public void When_LineIsRepeatedWhitespace(string[] lines)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                foreach (var line in lines)
                {
                    Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);
                }

                var tokenT = AssertSingleAddedTokenToSection<WhitespaceToken>(configIni, currentSection);
                Assert.That(tokenT.Lines, Has.Count.EqualTo(lines.Length));
                Assert.That(tokenT.Lines, Is.EquivalentTo(lines));
            }

            [TestCase("Key=Value", "Key", "Value")]
            [TestCase("=Value", "", "Value")]
            [TestCase(@"Key=('a=A', 'c=2')", "Key", "('a=A', 'c=2')")]
            public void When_LineIsSet(string line, string expectedKey, string expectedValue)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);

                var tokenT = AssertSingleAddedTokenToSection<InstructionToken>(configIni, currentSection);
                Assert.That(tokenT.InstructionType, Is.EqualTo(InstructionType.Set));
                Assert.That(tokenT.Key, Is.EqualTo(expectedKey));
                Assert.That(tokenT.Value, Is.EqualTo(expectedValue));
            }

            [TestCase("+Key=Value", "Key", "Value")]
            [TestCase("+=Value", "", "Value")]
            [TestCase(@"+Key=('a=A', 'c=2')", "Key", "('a=A', 'c=2')")]
            public void When_LineIsAdd(string line, string expectedKey, string expectedValue)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);

                var tokenT = AssertSingleAddedTokenToSection<InstructionToken>(configIni, currentSection);
                Assert.That(tokenT.InstructionType, Is.EqualTo(InstructionType.Add));
                Assert.That(tokenT.Key, Is.EqualTo(expectedKey));
                Assert.That(tokenT.Value, Is.EqualTo(expectedValue));
            }

            [TestCase(".Key=Value", "Key", "Value")]
            [TestCase(".=Value", "", "Value")]
            [TestCase(@".Key=('a=A', 'c=2')", "Key", "('a=A', 'c=2')")]
            public void When_LineIsAddOverride(string line, string expectedKey, string expectedValue)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);

                var tokenT = AssertSingleAddedTokenToSection<InstructionToken>(configIni, currentSection);
                Assert.That(tokenT.InstructionType, Is.EqualTo(InstructionType.AddOverride));
                Assert.That(tokenT.Key, Is.EqualTo(expectedKey));
                Assert.That(tokenT.Value, Is.EqualTo(expectedValue));
            }

            [TestCase("-Key=Value", "Key", "Value")]
            [TestCase("-=Value", "", "Value")]
            [TestCase(@"-Key=('a=A', 'c=2')", "Key", "('a=A', 'c=2')")]
            public void When_LineIsRemove(string line, string expectedKey, string expectedValue)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);

                var tokenT = AssertSingleAddedTokenToSection<InstructionToken>(configIni, currentSection);
                Assert.That(tokenT.InstructionType, Is.EqualTo(InstructionType.Remove));
                Assert.That(tokenT.Key, Is.EqualTo(expectedKey));
                Assert.That(tokenT.Value, Is.EqualTo(expectedValue));
            }

            [TestCase("!Key", "Key")]
            [TestCase("!Key=Value", "Key=Value")]
            [TestCase("!=Value", "=Value")]
            [TestCase(@"!Key=('a=A', 'c=2')", "Key=('a=A', 'c=2')")]
            public void When_LineIsRemoveAll(string line, string expectedKey)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);

                var tokenT = AssertSingleAddedTokenToSection<InstructionToken>(configIni, currentSection);
                Assert.That(tokenT.InstructionType, Is.EqualTo(InstructionType.RemoveAll));
                Assert.That(tokenT.Key, Is.EqualTo(expectedKey));
                Assert.That(tokenT.Value, Is.Null);
            }

            [TestCase(";")]
            [TestCase("; ")]
            [TestCase(" ;")]
            [TestCase("; Comment")]
            [TestCase(";; Comment")]
            [TestCase("; ;")]
            [TestCase(" ; ; ")]
            public void When_LineIsComment(string line)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);

                var tokenT = AssertSingleAddedTokenToSection<CommentToken>(configIni, currentSection);
                Assert.That(tokenT.Lines, Has.Count.EqualTo(1));
                Assert.That(tokenT.Lines[0], Is.EqualTo(line));
            }

            public static IEnumerable RepeatedCommentsTestCases
            {
                get
                {
                    yield return new TestCaseData(new[] { new[] { ";", ";" } });
                    yield return new TestCaseData(new[] { new[] { "; ", "; Comment" } });
                    yield return new TestCaseData(new[] { new[] { "; Comment", "; Details" } });
                    yield return new TestCaseData(new[] { new[] { "; Comment", "; Details", "; MoreDetails" } });
                    yield return new TestCaseData(new[] { new[] { "; Comment", ";", "; MoreDetails" } });
                }
            }

            [TestCaseSource(nameof(RepeatedCommentsTestCases))]
            public void When_LineIsRepeatedComment(string[] lines)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                foreach (var line in lines)
                {
                    Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);
                }

                var tokenT = AssertSingleAddedTokenToSection<CommentToken>(configIni, currentSection);
                Assert.That(tokenT.Lines, Has.Count.EqualTo(lines.Length));
                Assert.That(tokenT.Lines, Is.EquivalentTo(lines));
            }
        }
    }
}
