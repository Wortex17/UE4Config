using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UE4Config.Parsing;

namespace UE4Config.Tests.Parsing
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
            public void When_LineIsAddForce(string line, string expectedKey, string expectedValue)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);

                var tokenT = AssertSingleAddedTokenToSection<InstructionToken>(configIni, currentSection);
                Assert.That(tokenT.InstructionType, Is.EqualTo(InstructionType.AddForce));
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

            [TestCase("[NewSection]", "NewSection")]
            [TestCase("[Player.Values]", "Player.Values")]
            [TestCase("[Engine_Settings]", "Engine_Settings")]
            [TestCase("[/Script/Settings]", "/Script/Settings")]
            [TestCase("[ NewSection]", " NewSection")]
            [TestCase("[ NewSection ]", " NewSection ")]
            [TestCase("[ New Section ]", " New Section ")]
            [TestCase(" [NewSection]  ", "NewSection", " ", "  ")]
            public void When_LineIsSectionHeader(string line, string expectedName, string expectedWastePrefix = null, string expectedWasteSuffix = null)
            {
                var configIni = new ConfigIni();
                var initialSection = new ConfigIniSection();
                configIni.Sections.Add(initialSection);
                var currentSection = initialSection;

                Assert.That(() => configIni.ReadLine(line, ref currentSection), Throws.Nothing);

                Assert.That(initialSection, Is.Not.SameAs(currentSection));
                Assert.That(currentSection, Is.Not.Null);
                Assert.That(configIni.Sections, Has.Count.EqualTo(2));
                Assert.That(configIni.Sections[0], Is.SameAs(initialSection));
                Assert.That(configIni.Sections[1], Is.SameAs(currentSection));
                Assert.That(currentSection.Name, Is.EqualTo(expectedName));
                Assert.That(currentSection.LineWastePrefix, Is.EqualTo(expectedWastePrefix));
                Assert.That(currentSection.LineWasteSuffix, Is.EqualTo(expectedWasteSuffix));
            }
        }


        [TestFixture]
        class Read
        {
            [Test]
            public void When_ReaderHasMultipleValidLines()
            {
                var configIni = new ConfigIni();
                string textString = String.Join("\n", new[] {
                    ";Comment",
                    "",
                    "[Header]",
                    "Key=Value"
                });
                var textStringReader = new StringReader(textString);

                Assert.That(() => configIni.Read(textStringReader), Throws.Nothing);
                
                Assert.That(configIni.Sections, Has.Count.EqualTo(2));
                Assert.That(configIni.Sections[0].Name, Is.Null);
                Assert.That(configIni.Sections[0].Tokens, Has.Count.EqualTo(2));
                Assert.That(configIni.Sections[0].Tokens[0], Is.TypeOf<CommentToken>());
                Assert.That(configIni.Sections[0].Tokens[1], Is.TypeOf<WhitespaceToken>());
                Assert.That(configIni.Sections[1].Tokens, Has.Count.EqualTo(1));
                Assert.That(configIni.Sections[1].Tokens[0], Is.TypeOf<InstructionToken>());
            }

            [Test]
            public void When_ReaderHasMultipleEmptyLines()
            {
                var configIni = new ConfigIni();
                var textStringReader = new StringReader("\n\n\n\n");

                Assert.That(() => configIni.Read(textStringReader), Throws.Nothing);

                Assert.That(configIni.Sections, Has.Count.EqualTo(1));
                Assert.That(configIni.Sections[0].Name, Is.Null);
                Assert.That(configIni.Sections[0].Tokens, Has.Count.EqualTo(1));
                Assert.That(configIni.Sections[0].Tokens[0], Is.TypeOf<WhitespaceToken>());
                var whitespaceToken = configIni.Sections[0].Tokens[0] as WhitespaceToken;
                Assert.That(whitespaceToken.Lines, Has.Count.EqualTo(4));
            }

            [Test]
            public void When_ReadingRepeatedly()
            {
                var configIni = new ConfigIni();

                var textStringReaderA = new StringReader(";CommentA\n;CommentB");
                var textStringReaderB = new StringReader(";CommentC\n;CommentD");

                Assert.That(() => configIni.Read(textStringReaderA), Throws.Nothing);
                Assert.That(() => configIni.Read(textStringReaderB), Throws.Nothing);

                Assert.That(configIni.Sections, Has.Count.EqualTo(1));
                Assert.That(configIni.Sections[0].Name, Is.Null);
                Assert.That(configIni.Sections[0].Tokens, Has.Count.EqualTo(1));
                Assert.That(configIni.Sections[0].Tokens[0], Is.TypeOf<CommentToken>());
                var commentToken = configIni.Sections[0].Tokens[0] as CommentToken;
                Assert.That(commentToken.Lines, Has.Count.EqualTo(4));
            }
        }
    }
}
