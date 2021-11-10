using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UE4Config.Parsing;

namespace UE4Config.Tests.Parsing
{
    [TestFixture]
    class ConfigIniTests
    {
        [Test]
        public void When_ConstructedDefault()
        {
            ConfigIni config = null;
            Assert.That(() => { config = new ConfigIni(); }, Throws.Nothing);
            Assert.That(config.Name, Is.Null);
            Assert.That(config.Sections, Is.Empty);
        }

        [Test]
        public void When_ConstructedWithName()
        {
            string name = "Engine/Config/Base.ini";
            ConfigIni config = null;
            Assert.That(() => { config = new ConfigIni(name); }, Throws.Nothing);
            Assert.That(config.Name, Is.EqualTo(name));
            Assert.That(config.Sections, Is.Empty);
        }

        [Test]
        public void When_ConstructedWithSections()
        {
            ConfigIni config = null;
            var sectionA = new ConfigIniSection("A");
            var sectionB = new ConfigIniSection("A");
            Assert.That(() => { config = new ConfigIni(new[] { sectionA, sectionB }); }, Throws.Nothing);
            Assert.That(config.Name, Is.Null);
            Assert.That(config.Sections, Is.EquivalentTo(new[] { sectionA, sectionB }));
        }

        [Test]
        public void When_ConstructedWithNullSections()
        {
            ConfigIni config = null;
            Assert.That(() => { config = new ConfigIni((IEnumerable<ConfigIniSection>)null);}, Throws.Nothing);
            Assert.That(config.Name, Is.Null);
            Assert.That(config.Sections, Is.Empty);
        }

        [Test]
        public void When_ConstructedWithNameAndSections()
        {
            string name = "Engine/Config/Base.ini";
            ConfigIni config = null;
            var sectionA = new ConfigIniSection("A");
            var sectionB = new ConfigIniSection("B");
            Assert.That(() => { config = new ConfigIni(name, new[] {sectionA , sectionB}); }, Throws.Nothing);
            Assert.That(config.Name, Is.EqualTo(name));
            Assert.That(config.Sections, Is.EquivalentTo(new[] {sectionA, sectionB}));
        }

        [Test]
        public void When_ConstructedWithNameAndNullSections()
        {
            string name = "Engine/Config/Base.ini";
            ConfigIni config = null;
            Assert.That(() => { config = new ConfigIni(name, null); }, Throws.Nothing);
            Assert.That(config.Name, Is.EqualTo(name));
            Assert.That(config.Sections, Is.Empty);
        }

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

                Assert.That(() => configIni.ReadLineWithoutLineEnding(line, ref currentSection), Throws.Nothing);

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

                Assert.That(() => configIni.ReadLineWithoutLineEnding(line, ref currentSection), Throws.Nothing);

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

                Assert.That(() => configIni.ReadLineWithoutLineEnding(line, ref currentSection), Throws.Nothing);

                var tokenT = AssertSingleAddedTokenToSection<WhitespaceToken>(configIni, currentSection);
                Assert.That(tokenT.Lines, Has.Count.EqualTo(1));
                Assert.That(tokenT.Lines[0].ToString(), Is.EqualTo(line));
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
                    Assert.That(() => configIni.ReadLineWithoutLineEnding(line, ref currentSection), Throws.Nothing);
                }

                var tokenT = AssertSingleAddedTokenToSection<WhitespaceToken>(configIni, currentSection);
                Assert.That(tokenT.Lines, Has.Count.EqualTo(lines.Length));
                Assert.That(tokenT.GetStringLines(), Is.EquivalentTo(lines));
            }

            [TestCase("Key=Value", "Key", "Value")]
            [TestCase("=Value", "", "Value")]
            [TestCase(@"Key=('a=A', 'c=2')", "Key", "('a=A', 'c=2')")]
            public void When_LineIsSet(string line, string expectedKey, string expectedValue)
            {
                var configIni = new ConfigIni();
                var currentSection = new ConfigIniSection();
                configIni.Sections.Add(currentSection);

                Assert.That(() => configIni.ReadLineWithoutLineEnding(line, ref currentSection), Throws.Nothing);

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

                Assert.That(() => configIni.ReadLineWithoutLineEnding(line, ref currentSection), Throws.Nothing);

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

                Assert.That(() => configIni.ReadLineWithoutLineEnding(line, ref currentSection), Throws.Nothing);

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

                Assert.That(() => configIni.ReadLineWithoutLineEnding(line, ref currentSection), Throws.Nothing);

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

                Assert.That(() => configIni.ReadLineWithoutLineEnding(line, ref currentSection), Throws.Nothing);

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

                Assert.That(() => configIni.ReadLineWithoutLineEnding(line, ref currentSection), Throws.Nothing);

                var tokenT = AssertSingleAddedTokenToSection<CommentToken>(configIni, currentSection);
                Assert.That(tokenT.Lines, Has.Count.EqualTo(1));
                Assert.That(tokenT.Lines[0].ToString(), Is.EqualTo(line));
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
                    Assert.That(() => configIni.ReadLineWithoutLineEnding(line, ref currentSection), Throws.Nothing);
                }

                var tokenT = AssertSingleAddedTokenToSection<CommentToken>(configIni, currentSection);
                Assert.That(tokenT.Lines, Has.Count.EqualTo(lines.Length));
                Assert.That(tokenT.GetStringLines(), Is.EquivalentTo(lines));
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

                Assert.That(() => configIni.ReadLineWithoutLineEnding(line, ref currentSection), Throws.Nothing);

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


        [TestFixture]
        class MergeDuplicateSections
        {

            [Test]
            public void When_HasSingleSection()
            {
                var sectionA1 = new ConfigIniSection("A");
                var tokenA1_1 = new TextToken();
                var tokenA1_2 = new TextToken();
                var tokenA1_3 = new TextToken();
                sectionA1.Tokens.Add(tokenA1_1);
                sectionA1.Tokens.Add(tokenA1_2);
                sectionA1.Tokens.Add(tokenA1_3);

                var config = new ConfigIni();
                config.Sections.Add(sectionA1);

                config.MergeDuplicateSections();

                Assert.That(config.Sections, Is.EquivalentTo(new[] { sectionA1 }));
                Assert.That(sectionA1.Tokens, Is.EquivalentTo(new[] { tokenA1_1, tokenA1_2, tokenA1_3 }));
            }

            [Test]
            public void When_Has2DuplicateSections()
            {
                var sectionA1 = new ConfigIniSection("A");
                var tokenA1_1 = new TextToken();
                var tokenA1_2 = new TextToken();
                var tokenA1_3 = new TextToken();
                sectionA1.Tokens.Add(tokenA1_1);
                sectionA1.Tokens.Add(tokenA1_2);
                sectionA1.Tokens.Add(tokenA1_3);

                var sectionA2 = new ConfigIniSection("A");
                var tokenA2_1 = new TextToken();
                var tokenA2_2 = new TextToken();
                var tokenA2_3 = new TextToken();
                sectionA2.Tokens.Add(tokenA2_1);
                sectionA2.Tokens.Add(tokenA2_2);
                sectionA2.Tokens.Add(tokenA2_3);

                var config = new ConfigIni();
                config.Sections.Add(sectionA1);
                config.Sections.Add(sectionA2);

                config.MergeDuplicateSections();

                Assert.That(config.Sections, Is.EquivalentTo(new[] { sectionA1 }));
                Assert.That(sectionA1.Tokens, Is.EquivalentTo(new[] { tokenA1_1, tokenA1_2, tokenA1_3, tokenA2_1, tokenA2_2, tokenA2_3 }));
            }

            [Test]
            public void When_Has3DuplicateSections()
            {
                var sectionA1 = new ConfigIniSection("A");
                var tokenA1_1 = new TextToken();
                var tokenA1_2 = new TextToken();
                var tokenA1_3 = new TextToken();
                sectionA1.Tokens.Add(tokenA1_1);
                sectionA1.Tokens.Add(tokenA1_2);
                sectionA1.Tokens.Add(tokenA1_3);

                var sectionA2 = new ConfigIniSection("A");
                var tokenA2_1 = new TextToken();
                var tokenA2_2 = new TextToken();
                var tokenA2_3 = new TextToken();
                sectionA2.Tokens.Add(tokenA2_1);
                sectionA2.Tokens.Add(tokenA2_2);
                sectionA2.Tokens.Add(tokenA2_3);

                var sectionA3 = new ConfigIniSection("A");
                var tokenA3_1 = new TextToken();
                var tokenA3_2 = new TextToken();
                var tokenA3_3 = new TextToken();
                sectionA3.Tokens.Add(tokenA3_1);
                sectionA3.Tokens.Add(tokenA3_2);
                sectionA3.Tokens.Add(tokenA3_3);

                var config = new ConfigIni();
                config.Sections.Add(sectionA1);
                config.Sections.Add(sectionA2);
                config.Sections.Add(sectionA3);

                config.MergeDuplicateSections();

                Assert.That(config.Sections, Is.EquivalentTo(new[] { sectionA1 }));
                Assert.That(sectionA1.Tokens, Is.EquivalentTo(new[] { tokenA1_1, tokenA1_2, tokenA1_3, tokenA2_1, tokenA2_2, tokenA2_3, tokenA3_1, tokenA3_2, tokenA3_3 }));
            }

            [Test]
            public void When_Has2DistinctSections()
            {
                var sectionA1 = new ConfigIniSection("A");
                var tokenA1_1 = new TextToken();
                var tokenA1_2 = new TextToken();
                var tokenA1_3 = new TextToken();
                sectionA1.Tokens.Add(tokenA1_1);
                sectionA1.Tokens.Add(tokenA1_2);
                sectionA1.Tokens.Add(tokenA1_3);

                var sectionB1 = new ConfigIniSection("B");
                var tokenB1_1 = new TextToken();
                var tokenB1_2 = new TextToken();
                var tokenB1_3 = new TextToken();
                sectionB1.Tokens.Add(tokenB1_1);
                sectionB1.Tokens.Add(tokenB1_2);
                sectionB1.Tokens.Add(tokenB1_3);

                var config = new ConfigIni();
                config.Sections.Add(sectionA1);
                config.Sections.Add(sectionB1);

                config.MergeDuplicateSections();

                Assert.That(config.Sections, Is.EquivalentTo(new[] { sectionA1, sectionB1 }));
                Assert.That(sectionA1.Tokens, Is.EquivalentTo(new[] { tokenA1_1, tokenA1_2, tokenA1_3 }));
                Assert.That(sectionB1.Tokens, Is.EquivalentTo(new[] { tokenB1_1, tokenB1_2, tokenB1_3 }));
            }

            [Test]
            public void When_HasDistinctWithin2DuplicateSections()
            {
                var sectionA1 = new ConfigIniSection("A");
                var tokenA1_1 = new TextToken();
                var tokenA1_2 = new TextToken();
                var tokenA1_3 = new TextToken();
                sectionA1.Tokens.Add(tokenA1_1);
                sectionA1.Tokens.Add(tokenA1_2);
                sectionA1.Tokens.Add(tokenA1_3);

                var sectionB1 = new ConfigIniSection("B");
                var tokenB1_1 = new TextToken();
                var tokenB1_2 = new TextToken();
                var tokenB1_3 = new TextToken();
                sectionB1.Tokens.Add(tokenB1_1);
                sectionB1.Tokens.Add(tokenB1_2);
                sectionB1.Tokens.Add(tokenB1_3);

                var sectionA2 = new ConfigIniSection("A");
                var tokenA2_1 = new TextToken();
                var tokenA2_2 = new TextToken();
                var tokenA2_3 = new TextToken();
                sectionA2.Tokens.Add(tokenA2_1);
                sectionA2.Tokens.Add(tokenA2_2);
                sectionA2.Tokens.Add(tokenA2_3);

                var config = new ConfigIni();
                config.Sections.Add(sectionA1);
                config.Sections.Add(sectionB1);
                config.Sections.Add(sectionA2);

                config.MergeDuplicateSections();

                Assert.That(config.Sections, Is.EquivalentTo(new[] { sectionA1, sectionB1 }));
                Assert.That(sectionA1.Tokens, Is.EquivalentTo(new[] { tokenA1_1, tokenA1_2, tokenA1_3, tokenA2_1, tokenA2_2, tokenA2_3 }));
                Assert.That(sectionB1.Tokens, Is.EquivalentTo(new[] { tokenB1_1, tokenB1_2, tokenB1_3 }));
            }

            [Test]
            public void When_Has2DuplicateWithin2DistinctSections()
            {
                var sectionA1 = new ConfigIniSection("A");
                var tokenA1_1 = new TextToken();
                var tokenA1_2 = new TextToken();
                var tokenA1_3 = new TextToken();
                sectionA1.Tokens.Add(tokenA1_1);
                sectionA1.Tokens.Add(tokenA1_2);
                sectionA1.Tokens.Add(tokenA1_3);

                var sectionB1 = new ConfigIniSection("B");
                var tokenB1_1 = new TextToken();
                var tokenB1_2 = new TextToken();
                var tokenB1_3 = new TextToken();
                sectionB1.Tokens.Add(tokenB1_1);
                sectionB1.Tokens.Add(tokenB1_2);
                sectionB1.Tokens.Add(tokenB1_3);

                var sectionB2 = new ConfigIniSection("B");
                var tokenB2_1 = new TextToken();
                var tokenB2_2 = new TextToken();
                var tokenB2_3 = new TextToken();
                sectionB2.Tokens.Add(tokenB2_1);
                sectionB2.Tokens.Add(tokenB2_2);
                sectionB2.Tokens.Add(tokenB2_3);

                var sectionC1 = new ConfigIniSection("C");
                var tokenC1_1 = new TextToken();
                var tokenC1_2 = new TextToken();
                var tokenC1_3 = new TextToken();
                sectionC1.Tokens.Add(tokenC1_1);
                sectionC1.Tokens.Add(tokenC1_2);
                sectionC1.Tokens.Add(tokenC1_3);

                var config = new ConfigIni();
                config.Sections.Add(sectionA1);
                config.Sections.Add(sectionB1);
                config.Sections.Add(sectionB2);
                config.Sections.Add(sectionC1);

                config.MergeDuplicateSections();

                Assert.That(config.Sections, Is.EquivalentTo(new[] { sectionA1, sectionB1, sectionC1 }));
                Assert.That(sectionA1.Tokens, Is.EquivalentTo(new[] { tokenA1_1, tokenA1_2, tokenA1_3 }));
                Assert.That(sectionB1.Tokens, Is.EquivalentTo(new[] { tokenB1_1, tokenB1_2, tokenB1_3, tokenB2_1, tokenB2_2, tokenB2_3 }));
                Assert.That(sectionC1.Tokens, Is.EquivalentTo(new[] { tokenC1_1, tokenC1_2, tokenC1_3 }));
            }
        }


        [TestFixture]
        class CondenseWhitespace
        {

            [Test]
            public void When_HasNothingToCondense()
            {
                var sectionA1 = new ConfigIniSection("A");
                var tokenA1_1 = new TextToken();
                var tokenA1_2 = new TextToken();
                var tokenA1_3 = new TextToken();
                sectionA1.Tokens.Add(tokenA1_1);
                sectionA1.Tokens.Add(tokenA1_2);
                sectionA1.Tokens.Add(tokenA1_3);

                var config = new ConfigIni();
                config.Sections.Add(sectionA1);

                config.CondenseWhitespace();

                Assert.That(config.Sections, Is.EquivalentTo(new[] { sectionA1 }));
                Assert.That(sectionA1.Tokens, Is.EquivalentTo(new[] { tokenA1_1, tokenA1_2, tokenA1_3 }));
            }

            [Test]
            public void When_HasWhitespaceToCondense()
            {
                var sectionA1 = new ConfigIniSection("A");
                var tokenA1_1 = new TextToken();
                var tokenA1_2_WS = new WhitespaceToken(new List<string>(new []{"   "}), LineEnding.None);
                var tokenA1_3 = new TextToken();
                sectionA1.Tokens.Add(tokenA1_1);
                sectionA1.Tokens.Add(tokenA1_2_WS);
                sectionA1.Tokens.Add(tokenA1_3);

                var config = new ConfigIni();
                config.Sections.Add(sectionA1);

                config.CondenseWhitespace();

                Assert.That(config.Sections, Is.EquivalentTo(new[] { sectionA1 }));
                Assert.That(sectionA1.Tokens, Is.EquivalentTo(new IniToken[] { tokenA1_1, tokenA1_2_WS, tokenA1_3 }));
                Assert.That(tokenA1_2_WS.Lines, Is.EquivalentTo(new TextLine[]{String.Empty}));
            }

            [Test]
            public void When_HasConsecutiveWhitespaceToCondense()
            {
                var sectionA1 = new ConfigIniSection("A");
                var tokenA1_1 = new TextToken();
                var tokenA1_2_WS = new WhitespaceToken(new List<string>(new[] { "   " }), LineEnding.None);
                var tokenA1_3_WS = new WhitespaceToken(new List<string>(new[] { "   " }), LineEnding.None);
                var tokenA1_4_WS = new WhitespaceToken(new List<string>(new[] { "   " }), LineEnding.None);
                var tokenA1_5 = new TextToken();
                sectionA1.Tokens.Add(tokenA1_1);
                sectionA1.Tokens.Add(tokenA1_2_WS);
                sectionA1.Tokens.Add(tokenA1_3_WS);
                sectionA1.Tokens.Add(tokenA1_4_WS);
                sectionA1.Tokens.Add(tokenA1_5);

                var config = new ConfigIni();
                config.Sections.Add(sectionA1);

                config.CondenseWhitespace();

                Assert.That(config.Sections, Is.EquivalentTo(new[] { sectionA1 }));
                Assert.That(sectionA1.Tokens, Is.EquivalentTo(new IniToken[] { tokenA1_1, tokenA1_2_WS, tokenA1_5 }));
                Assert.That(tokenA1_2_WS.Lines, Is.EquivalentTo(new TextLine[] { String.Empty }));
            }

            [Test]
            public void When_HasMultipleWhitespaceToCondense()
            {
                var sectionA1 = new ConfigIniSection("A");
                var tokenA1_1 = new TextToken();
                var tokenA1_2_WS = new WhitespaceToken(new List<string>(new[] { "   " }), LineEnding.None);
                var tokenA1_3 = new TextToken();
                var tokenA1_4_WS = new WhitespaceToken(new List<string>(new[] { "   " }), LineEnding.None);
                var tokenA1_5 = new TextToken();
                sectionA1.Tokens.Add(tokenA1_1);
                sectionA1.Tokens.Add(tokenA1_2_WS);
                sectionA1.Tokens.Add(tokenA1_3);
                sectionA1.Tokens.Add(tokenA1_4_WS);
                sectionA1.Tokens.Add(tokenA1_5);

                var config = new ConfigIni();
                config.Sections.Add(sectionA1);

                config.CondenseWhitespace();

                Assert.That(config.Sections, Is.EquivalentTo(new[] { sectionA1 }));
                Assert.That(sectionA1.Tokens, Is.EquivalentTo(new IniToken[] { tokenA1_1, tokenA1_2_WS, tokenA1_3, tokenA1_4_WS, tokenA1_5 }));
                Assert.That(tokenA1_2_WS.Lines, Is.EquivalentTo(new TextLine[] { String.Empty }));
                Assert.That(tokenA1_4_WS.Lines, Is.EquivalentTo(new TextLine[] { String.Empty }));
            }
        }

        [TestFixture]
        class NormalizeLineEndings
        {
            [TestCase(LineEnding.None)]
            [TestCase(LineEnding.Unix)]
            [TestCase(LineEnding.Mac)]
            [TestCase(LineEnding.Windows)]
            [TestCase(LineEnding.Unknown)]
            public void When_ChangingToSpecificLineEndingOnMultipleSections(LineEnding targetLineEnding)
            {
                var config = new ConfigIni();

                var sectionA1 = new ConfigIniSection("A1");
                sectionA1.LineEnding = LineEnding.Mac;
                var tokenA1 = new InstructionToken(InstructionType.Add, "InstA", LineEnding.Windows);
                var tokenA2 = new WhitespaceToken(new[] { " \t\t", " " }, LineEnding.None);
                var tokenA3 = new CommentToken(new[] { ";Baz", "OO" }, LineEnding.Unix);
                sectionA1.Tokens.Add(tokenA1);
                sectionA1.Tokens.Add(tokenA2);
                sectionA1.Tokens.Add(tokenA3);
                config.Sections.Add(sectionA1);


                var sectionB1 = new ConfigIniSection("B1");
                sectionB1.LineEnding = LineEnding.Unix;
                var tokenB1 = new InstructionToken(InstructionType.Add, "InstB", LineEnding.Unknown);
                var tokenB2 = new WhitespaceToken(new[] { " \t\t", " " }, LineEnding.Windows);
                var tokenB3 = new CommentToken(new[] { ";Baz", "OO" }, LineEnding.Mac);
                sectionB1.Tokens.Add(tokenB1);
                sectionB1.Tokens.Add(tokenB2);
                sectionB1.Tokens.Add(tokenB3);
                config.Sections.Add(sectionB1);

                config.NormalizeLineEndings(targetLineEnding);

                Assert.That(sectionA1.LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(tokenA1.LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(tokenA2.Lines[0].LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(tokenA2.Lines[1].LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(tokenA3.Lines[0].LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(tokenA3.Lines[1].LineEnding, Is.EqualTo(targetLineEnding));

                Assert.That(sectionB1.LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(tokenB1.LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(tokenB2.Lines[0].LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(tokenB2.Lines[1].LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(tokenB3.Lines[0].LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(tokenB3.Lines[1].LineEnding, Is.EqualTo(targetLineEnding));
            }


            [TestCase(LineEnding.Unix)]
            [TestCase(LineEnding.Mac)]
            [TestCase(LineEnding.Windows)]
            public void When_ChangingToAutoDetectedLineEnding_FirstSectionHeader(LineEnding expectedLineEnding)
            {
                var config = new ConfigIni();

                var sectionA1 = new ConfigIniSection("A1");
                sectionA1.LineEnding = expectedLineEnding; //LineEnding on Header of first section is expected to be used
                var tokenA1 = new InstructionToken(InstructionType.Add, "InstA", LineEnding.Windows);
                var tokenA2 = new WhitespaceToken(new[] { " \t\t", " " }, LineEnding.None);
                var tokenA3 = new CommentToken(new[] { ";Baz", "OO" }, LineEnding.Unix);
                sectionA1.Tokens.Add(tokenA1);
                sectionA1.Tokens.Add(tokenA2);
                sectionA1.Tokens.Add(tokenA3);
                config.Sections.Add(sectionA1);


                var sectionB1 = new ConfigIniSection("B1");
                sectionB1.LineEnding = LineEnding.Unix;
                var tokenB1 = new InstructionToken(InstructionType.Add, "InstB", LineEnding.Unknown);
                var tokenB2 = new WhitespaceToken(new[] { " \t\t", " " }, LineEnding.Windows);
                var tokenB3 = new CommentToken(new[] { ";Baz", "OO" }, LineEnding.Mac);
                sectionB1.Tokens.Add(tokenB1);
                sectionB1.Tokens.Add(tokenB2);
                sectionB1.Tokens.Add(tokenB3);
                config.Sections.Add(sectionB1);

                config.NormalizeLineEndings();

                Assert.That(sectionA1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA2.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA2.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA3.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA3.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));

                Assert.That(sectionB1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB2.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB2.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB3.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB3.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
            }

            [TestCase(LineEnding.Unix)]
            [TestCase(LineEnding.Mac)]
            [TestCase(LineEnding.Windows)]
            public void When_ChangingToAutoDetectedLineEnding_FirstInstructionToken(LineEnding expectedLineEnding)
            {
                var config = new ConfigIni();

                var sectionA1 = new ConfigIniSection("A1");
                var tokenA1 = new InstructionToken(InstructionType.Add, "InstA", expectedLineEnding);
                var tokenA2 = new WhitespaceToken(new[] { " \t\t", " " }, LineEnding.None);
                var tokenA3 = new CommentToken(new[] { ";Baz", "OO" }, LineEnding.Unix);
                sectionA1.Tokens.Add(tokenA1);
                sectionA1.Tokens.Add(tokenA2);
                sectionA1.Tokens.Add(tokenA3);
                config.Sections.Add(sectionA1);


                var sectionB1 = new ConfigIniSection("B1");
                sectionB1.LineEnding = LineEnding.Unix;
                var tokenB1 = new InstructionToken(InstructionType.Add, "InstB", LineEnding.Unknown);
                var tokenB2 = new WhitespaceToken(new[] { " \t\t", " " }, LineEnding.Windows);
                var tokenB3 = new CommentToken(new[] { ";Baz", "OO" }, LineEnding.Mac);
                sectionB1.Tokens.Add(tokenB1);
                sectionB1.Tokens.Add(tokenB2);
                sectionB1.Tokens.Add(tokenB3);
                config.Sections.Add(sectionB1);

                config.NormalizeLineEndings();

                Assert.That(sectionA1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA2.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA2.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA3.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA3.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));

                Assert.That(sectionB1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB2.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB2.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB3.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB3.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
            }

            [TestCase(LineEnding.Unix)]
            [TestCase(LineEnding.Mac)]
            [TestCase(LineEnding.Windows)]
            public void When_ChangingToAutoDetectedLineEnding_FirstWhitespaceToken(LineEnding expectedLineEnding)
            {
                var config = new ConfigIni();

                var sectionA1 = new ConfigIniSection("A1");
                var tokenA1 = new InstructionToken(InstructionType.Add, "InstA");
                var tokenA2 = new WhitespaceToken(new[] { " \t\t", " " }, expectedLineEnding);
                var tokenA3 = new CommentToken(new[] { ";Baz", "OO" }, LineEnding.Unix);
                sectionA1.Tokens.Add(tokenA1);
                sectionA1.Tokens.Add(tokenA2);
                sectionA1.Tokens.Add(tokenA3);
                config.Sections.Add(sectionA1);


                var sectionB1 = new ConfigIniSection("B1");
                sectionB1.LineEnding = LineEnding.Unix;
                var tokenB1 = new InstructionToken(InstructionType.Add, "InstB", LineEnding.Unknown);
                var tokenB2 = new WhitespaceToken(new[] { " \t\t", " " }, LineEnding.Windows);
                var tokenB3 = new CommentToken(new[] { ";Baz", "OO" }, LineEnding.Mac);
                sectionB1.Tokens.Add(tokenB1);
                sectionB1.Tokens.Add(tokenB2);
                sectionB1.Tokens.Add(tokenB3);
                config.Sections.Add(sectionB1);

                config.NormalizeLineEndings();

                Assert.That(sectionA1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA2.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA2.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA3.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA3.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));

                Assert.That(sectionB1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB2.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB2.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB3.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB3.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
            }

            [TestCase(LineEnding.Unix)]
            [TestCase(LineEnding.Mac)]
            [TestCase(LineEnding.Windows)]
            public void When_ChangingToAutoDetectedLineEnding_FirstWhitespaceTokenSecondLine(LineEnding expectedLineEnding)
            {
                var config = new ConfigIni();

                var sectionA1 = new ConfigIniSection("A1");
                var tokenA1 = new InstructionToken(InstructionType.Add, "InstA");
                var tokenA2 = new WhitespaceToken();
                tokenA2.AddLine("\t\t");
                tokenA2.AddLine(" ", expectedLineEnding); // First time a line ending would be found
                tokenA2.AddLine(" ");
                var tokenA3 = new CommentToken(new[] { ";Baz", "OO" }, LineEnding.Unix);
                sectionA1.Tokens.Add(tokenA1);
                sectionA1.Tokens.Add(tokenA2);
                sectionA1.Tokens.Add(tokenA3);
                config.Sections.Add(sectionA1);


                var sectionB1 = new ConfigIniSection("B1");
                sectionB1.LineEnding = LineEnding.Unix;
                var tokenB1 = new InstructionToken(InstructionType.Add, "InstB", LineEnding.Unknown);
                var tokenB2 = new WhitespaceToken(new[] { " \t\t", " " }, LineEnding.Windows);
                var tokenB3 = new CommentToken(new[] { ";Baz", "OO" }, LineEnding.Mac);
                sectionB1.Tokens.Add(tokenB1);
                sectionB1.Tokens.Add(tokenB2);
                sectionB1.Tokens.Add(tokenB3);
                config.Sections.Add(sectionB1);

                config.NormalizeLineEndings();

                Assert.That(sectionA1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA2.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA2.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA3.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA3.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));

                Assert.That(sectionB1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB2.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB2.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB3.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB3.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
            }

            [Test]
            public void When_ChangingToAutoDetectedLineEnding_NoKnownLineEndings()
            {
                var config = new ConfigIni();

                var sectionA1 = new ConfigIniSection("A1");
                var tokenA1 = new InstructionToken(InstructionType.Add, "InstA");
                var tokenA2 = new WhitespaceToken();
                tokenA2.AddLine("\t\t");
                tokenA2.AddLine(" ");
                tokenA2.AddLine(" ");
                var tokenA3 = new CommentToken();
                tokenA3.AddLine(";Baz");
                tokenA3.AddLine("OO");
                sectionA1.Tokens.Add(tokenA1);
                sectionA1.Tokens.Add(tokenA2);
                sectionA1.Tokens.Add(tokenA3);
                config.Sections.Add(sectionA1);


                var sectionB1 = new ConfigIniSection("B1");
                var tokenB1 = new InstructionToken(InstructionType.Add, "InstB");
                var tokenB2 = new WhitespaceToken();
                tokenB2.AddLine("\t\t");
                tokenB2.AddLine(" ");
                var tokenB3 = new CommentToken();
                tokenB3.AddLine(";Baz");
                tokenB3.AddLine("OO");
                sectionB1.Tokens.Add(tokenB1);
                sectionB1.Tokens.Add(tokenB2);
                sectionB1.Tokens.Add(tokenB3);
                config.Sections.Add(sectionB1);

                config.NormalizeLineEndings();

                var expectedLineEnding = LineEnding.Unknown;

                Assert.That(sectionA1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA2.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA2.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA3.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenA3.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));

                Assert.That(sectionB1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB1.LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB2.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB2.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB3.Lines[0].LineEnding, Is.EqualTo(expectedLineEnding));
                Assert.That(tokenB3.Lines[1].LineEnding, Is.EqualTo(expectedLineEnding));
            }

            [Test]
            public void When_ChangingToAutoDetectedLineEnding_EmptyIni()
            {
                var config = new ConfigIni();

                Assert.That(() => {config.NormalizeLineEndings();}, Throws.Nothing);
            }
        }

        [TestFixture]
        public class Write
        {
            class SpyConfigIniSection : ConfigIniSection
            {
                public SpyConfigIniSection(string name) : base(name) { }

                public List<ConfigIniSection> Write_CallLog;

                public override void Write(TextWriter writer)
                {
                    Write_CallLog?.Add(this);
                    base.Write(writer);
                }
            }

            [Test]
            public void When_HasSections_RelaysCallsToSections()
            {
                var callLog = new List<ConfigIniSection>();
                var config = new ConfigIni();
                var spySectionA = new SpyConfigIniSection("A") { Write_CallLog = callLog };
                var spySectionB = new SpyConfigIniSection("B") { Write_CallLog = callLog };
                config.Sections.Add(spySectionA);
                config.Sections.Add(spySectionB);

                var writer = new StringWriter();
                config.Write(writer);
                Assert.That(callLog, Is.EquivalentTo(new[] { spySectionA, spySectionB }));
            }

            [Test]
            public void When_HasNullSections_DoesSkipNull()
            {
                var callLog = new List<ConfigIniSection>();
                var config = new ConfigIni();
                var spySectionA = new SpyConfigIniSection("A") { Write_CallLog = callLog };
                var spySectionB = new SpyConfigIniSection("B") { Write_CallLog = callLog };
                config.Sections.Add(spySectionA);
                config.Sections.Add(null);
                config.Sections.Add(spySectionB);

                var writer = new StringWriter();
                config.Write(writer);
                Assert.That(config.Sections, Has.Count.EqualTo(3));
                Assert.That(callLog, Is.EquivalentTo(new[] { spySectionA, spySectionB }));
            }
        }

        [TestFixture]
        public class ReadWriteSmoke
        {
            public static IEnumerable Cases_When_Unmodified
            {
                get
                {
                    yield return new TestCaseData(new[]
                    {
                        ""
                    }).SetName("Empty String");

                    yield return new TestCaseData(new[]
                    {
                        Environment.NewLine
                    }).SetName("Pure System Line Ending");

                    yield return new TestCaseData(new[]
                    {
                        "\n"
                    }).SetName("Pure Unix Line Ending");

                    yield return new TestCaseData(new[]
                    {
                        "\r\n"
                    }).SetName("Pure Windows Line Ending");

                    yield return new TestCaseData(new[]
                    {
                        "\r"
                    }).SetName("Pure Mac Line Ending");

                    yield return new TestCaseData(new[]
                    {
                        "Text"
                    }).SetName("Text, No Line Ending");

                    yield return new TestCaseData(new[]
                    {
                        "Text"+Environment.NewLine
                    }).SetName("Text, System Line Ending");

                    yield return new TestCaseData(new[]
                    {
                        "Text\n"
                    }).SetName("Text, Unix Line Ending");

                    yield return new TestCaseData(new[]
                    {
                        "Text\r\n"
                    }).SetName("Text, Windows Line Ending");

                    yield return new TestCaseData(new[]
                    {
                        "Text\r"
                    }).SetName("Text, Mac Line Ending");

                    yield return new TestCaseData(new[]
                    {
                        "MyKey=MyValue"+Environment.NewLine
                    }).SetName("Single Instruction, System Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        "MyKey=MyValue"
                    }).SetName("Single Instruction, No Line Ending");

                    yield return new TestCaseData(new[]
                    {
                        "MyKey=MyValue\n"
                    }).SetName("Single Instruction, Unix Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        "MyKey=MyValue\r\n"
                    }).SetName("Single Instruction, Windows Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        "MyKey=MyValue\r"
                    }).SetName("Single Instruction, Mac Line Endings");
                    
                    yield return new TestCaseData(new[]
                    {
                        "[MySection]"+Environment.NewLine
                    }).SetName("Single Section, System Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        "[MySection]"
                    }).SetName("Single Section, No Line Ending");

                    yield return new TestCaseData(new[]
                    {
                        "[MySection]\n"
                    }).SetName("Single Section, Unix Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        "[MySection]\r\n"
                    }).SetName("Single Section, Windows Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        "[MySection]\r"
                    }).SetName("Single Section, Mac Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        String.Join(Environment.NewLine, new[]
                        {
                            "MyKey=MyValue",
                            "+MyKey=MyValue",
                            ".MyKey=MyValue",
                            "-MyKey=MyValue",
                            "!MyKey",
                            "!MyKey=SuperfluousValueForRemoveAll"
                        })
                    }).SetName("All Instructions, System Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        String.Join(Environment.NewLine, new[]
                        {
                            ";MyComment",
                            "MyKey=MyValue",
                            "Blurb",
                            "",
                            "[MySection]",
                            "MySpecialKey=MySpecialValue",
                            ""
                        })
                    }).SetName("Simple ConfigIni, System Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        String.Join("\n", new[]
                        {
                            ";MyComment",
                            "MyKey=MyValue",
                            "Blurb",
                            "",
                            "[MySection]",
                            "MySpecialKey=MySpecialValue",
                            ""
                        })
                    }).SetName("Simple ConfigIni, Unix Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        String.Join("\r\n", new[]
                        {
                            ";MyComment",
                            "MyKey=MyValue",
                            "Blurb",
                            "",
                            "[MySection]",
                            "MySpecialKey=MySpecialValue",
                            ""
                        })
                    }).SetName("Simple ConfigIni, Windows Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        String.Join("\r", new[]
                        {
                            ";MyComment",
                            "MyKey=MyValue",
                            "Blurb",
                            "",
                            "[MySection]",
                            "MySpecialKey=MySpecialValue",
                            ""
                        })
                    }).SetName("Simple ConfigIni, Mac Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        String.Concat(new[]
                        {
                            ";MyComment\n",
                            "MyKey=MyValue\n",
                            "Blurb\n",
                            "\n",
                            "[MySection]\n",
                            "MySpecialKey=MySpecialValue\n",
                            "\n",
                            ";MyComment\r\n",
                            "MyKey=MyValue\r\n",
                            "Blurb\r\n",
                            "\r\n",
                            "[MySection]\r\n",
                            "MySpecialKey=MySpecialValue\r\n",
                            "\r\n",
                            ";MyComment\r",
                            "MyKey=MyValue\r",
                            "Blurb\r",
                            "\r",
                            "[MySection]\r",
                            "MySpecialKey=MySpecialValue\r",
                            "\r"
                        })
                    }).SetName("Simple ConfigIni, Mixed Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        String.Concat(new[]
                        {
                            "\t \n",
                            "\t  \r\n",
                            "\t   \r"
                        })
                    }).SetName("Whitespace, Mixed Line Endings");

                    yield return new TestCaseData(new[]
                    {
                        String.Concat(new[]
                        {
                            ";Comment \n",
                            " ; Comment  \r\n",
                            "  ;  Comment   \r"
                        })
                    }).SetName("Comments, Mixed Line Endings");
                }
            }

            [TestCaseSource(nameof(Cases_When_Unmodified))]
            public void When_Unmodified_DoesRetainEverything(string original)
            {
                var config = new ConfigIni();
                config.Read(new StringReader(original));
                var writer = new StringWriter();
                config.Write(writer);

                Assert.That(writer.ToString(), Is.EqualTo(original));
            }



            [TestCase("MockProject/Config/DefaultEditor.ini")]
            [TestCase("MockProject/Config/DefaultGame.ini")]
            [TestCase("MockProject/Config/DefaultEngine.ini")]
            public void When_UnmodifiedFile_IsUnchanged(string file)
            {
                var filePath = TestUtils.GetTestDataPath(file);
                var reader = File.OpenText(filePath);
                var config = new ConfigIni();
                config.Read(reader);
                reader.Close();

                var fileContent = File.ReadAllText(filePath);
                var writer = new StringWriter();
                config.Write(writer);
                var writerContent = writer.ToString();
                Assert.That(writerContent, Is.EqualTo(fileContent));
            }

        }
    }
}
