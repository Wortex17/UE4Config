using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UE4Config.Parsing;

namespace UE4Config.Tests.Parsing
{
    [TestFixture]
    class ConfigIniSectionTests
    {

        [Test]
        public void When_ConstructedDefault()
        {
            ConfigIniSection section = null;
            Assert.That(() => { section = new ConfigIniSection(); }, Throws.Nothing);
            Assert.That(section.Name, Is.Null);
            Assert.That(section.Tokens, Is.Empty);
        }

        [Test]
        public void When_ConstructedWithName()
        {
            string name = "/Script/Engine.PlayerInput";
            ConfigIniSection section = null;
            Assert.That(() => { section = new ConfigIniSection(name); }, Throws.Nothing);
            Assert.That(section.Name, Is.EqualTo(name));
            Assert.That(section.Tokens, Is.Empty);
        }

        [Test]
        public void When_ConstructedWithTokens()
        {
            ConfigIniSection section = null;
            var token1 = new TextToken();
            var token2 = new TextToken();
            Assert.That(() => { section = new ConfigIniSection(new[] { token1, token2 }); }, Throws.Nothing);
            Assert.That(section.Name, Is.Null);
            Assert.That(section.Tokens, Is.EquivalentTo(new[] { token1, token2 }));
        }

        [Test]
        public void When_ConstructedWithNullTokens()
        {
            ConfigIniSection section = null;
            Assert.That(() => { section = new ConfigIniSection((IEnumerable<IniToken>)null); }, Throws.Nothing);
            Assert.That(section.Name, Is.Null);
            Assert.That(section.Tokens, Is.Empty);
        }

        [Test]
        public void When_ConstructedWithNameAndTokens()
        {
            string name = "/Script/Engine.PlayerInput";
            ConfigIniSection section = null;
            var token1 = new TextToken();
            var token2 = new TextToken();
            Assert.That(() => { section = new ConfigIniSection(name, new[] { token1, token2 }); }, Throws.Nothing);
            Assert.That(section.Name, Is.EqualTo(name));
            Assert.That(section.Tokens, Is.EquivalentTo(new[] { token1, token2 }));
        }

        [Test]
        public void When_ConstructedWithNameAndNullTokens()
        {
            string name = "/Script/Engine.PlayerInput";
            ConfigIniSection section = null;
            Assert.That(() => { section = new ConfigIniSection(name, null); }, Throws.Nothing);
            Assert.That(section.Name, Is.EqualTo(name));
            Assert.That(section.Tokens, Is.Empty);
        }

        [TestFixture]
        class WriteTokens
        {

            class SpyIniToken : IniToken
            {
                public List<IniToken> Write_CallLog;

                public override void Write(TextWriter writer)
                {
                    Write_CallLog?.Add(this);
                }
            }

            [Test]
            public void When_HasTokens()
            {
                List<IniToken> callLog = new List<IniToken>();
                var spySection = new ConfigIniSection();
                var spyToken1 = new SpyIniToken() { Write_CallLog = callLog };
                var spyToken2 = new SpyIniToken() { Write_CallLog = callLog };
                spySection.Tokens.Add(spyToken1);
                spySection.Tokens.Add(spyToken2);
                var writer = new StringWriter();
                spySection.WriteTokens(writer);
                Assert.That(callLog, Is.EquivalentTo(new[] { spyToken1 , spyToken2}));
            }

            [Test]
            public void When_HasNullTokens_DoesSkipNull()
            {
                List<IniToken> callLog = new List<IniToken>();
                var spySection = new ConfigIniSection();
                var spyToken1 = new SpyIniToken() { Write_CallLog = callLog };
                var spyToken2 = new SpyIniToken() { Write_CallLog = callLog };
                spySection.Tokens.Add(spyToken1);
                spySection.Tokens.Add(null);
                spySection.Tokens.Add(spyToken2);
                var writer = new StringWriter();
                spySection.WriteTokens(writer);
                Assert.That(spySection.Tokens, Has.Count.EqualTo(3));
                Assert.That(callLog, Is.EquivalentTo(new[] { spyToken1, spyToken2 }));
            }
        }

        [TestFixture]
        class Write
        {
            class SpyConfigIniSection : ConfigIniSection
            {
                public int WriteTokens_CallCount;

                public override void WriteTokens(TextWriter writer)
                {
                    WriteTokens_CallCount++;
                    base.WriteTokens(writer);
                }
            }

            [Test]
            public void Does_RelayCalls()
            {
                var spySection = new SpyConfigIniSection();
                var writer = new StringWriter();
                spySection.Write(writer);
                Assert.That(spySection.WriteTokens_CallCount, Is.EqualTo(1));
            }

            public static IEnumerable Cases_WriteHeader
            {
                get
                {

                    yield return new TestCaseData(new object[] { new ConfigIniSection() { }, $"" }).SetName($"Unnamed Section, Unspecified LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection() { LineWastePrefix = " " }, $"" }).SetName($"Unnamed Section with LineWastePrefix, Unspecified LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection() { LineWasteSuffix = " " }, $"" }).SetName($"Unnamed Section with LineWasteSuffix, Unspecified LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection() { LineWastePrefix = " ", LineWasteSuffix = " " }, $"" }).SetName($"Unnamed Section with LineWaste, Unspecified LineEnding");

                    string sectionName = "/Script/Engine.PlayerInput";
                    var expectedLineEnding = Environment.NewLine;

                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { }, $"[{sectionName}]{expectedLineEnding}" }).SetName($"Section, Unspecified LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineWastePrefix = " " }, $" [{sectionName}]{expectedLineEnding}" }).SetName($"Section with LineWastePrefix, Unspecified LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineWasteSuffix = " " }, $"[{sectionName}] {expectedLineEnding}" }).SetName($"Section with LineWasteSuffix, Unspecified LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineWastePrefix = " ", LineWasteSuffix = " " }, $" [{sectionName}] {expectedLineEnding}" }).SetName($"Section with LineWaste, Unspecified LineEnding");

                    var lineEnding = LineEnding.Unknown;

                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding }, $"[{sectionName}]{expectedLineEnding}" }).SetName($"Section, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWastePrefix = " " }, $" [{sectionName}]{expectedLineEnding}" }).SetName($"Section with LineWastePrefix, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWasteSuffix = " " }, $"[{sectionName}] {expectedLineEnding}" }).SetName($"Section with LineWasteSuffix, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWastePrefix = " ", LineWasteSuffix = " " }, $" [{sectionName}] {expectedLineEnding}" }).SetName($"Section with LineWaste, {lineEnding} LineEnding");

                    lineEnding = LineEnding.None;
                    expectedLineEnding = "";

                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding }, $"[{sectionName}]{expectedLineEnding}" }).SetName($"Section, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWastePrefix = " " }, $" [{sectionName}]{expectedLineEnding}" }).SetName($"Section with LineWastePrefix, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWasteSuffix = " " }, $"[{sectionName}] {expectedLineEnding}" }).SetName($"Section with LineWasteSuffix, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWastePrefix = " ", LineWasteSuffix = " " }, $" [{sectionName}] {expectedLineEnding}" }).SetName($"Section with LineWaste, {lineEnding} LineEnding");

                    lineEnding = LineEnding.Unix;
                    expectedLineEnding = "\n";

                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding }, $"[{sectionName}]{expectedLineEnding}" }).SetName($"Section, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWastePrefix = " " }, $" [{sectionName}]{expectedLineEnding}" }).SetName($"Section with LineWastePrefix, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWasteSuffix = " " }, $"[{sectionName}] {expectedLineEnding}" }).SetName($"Section with LineWasteSuffix, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWastePrefix = " ", LineWasteSuffix = " " }, $" [{sectionName}] {expectedLineEnding}" }).SetName($"Section with LineWaste, {lineEnding} LineEnding");

                    lineEnding = LineEnding.Windows;
                    expectedLineEnding = "\r\n";

                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding }, $"[{sectionName}]{expectedLineEnding}" }).SetName($"Section, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWastePrefix = " " }, $" [{sectionName}]{expectedLineEnding}" }).SetName($"Section with LineWastePrefix, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWasteSuffix = " " }, $"[{sectionName}] {expectedLineEnding}" }).SetName($"Section with LineWasteSuffix, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWastePrefix = " ", LineWasteSuffix = " " }, $" [{sectionName}] {expectedLineEnding}" }).SetName($"Section with LineWaste, {lineEnding} LineEnding");

                    lineEnding = LineEnding.Mac;
                    expectedLineEnding = "\r";

                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding }, $"[{sectionName}]{expectedLineEnding}" }).SetName($"Section, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWastePrefix = " " }, $" [{sectionName}]{expectedLineEnding}" }).SetName($"Section with LineWastePrefix, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWasteSuffix = " " }, $"[{sectionName}] {expectedLineEnding}" }).SetName($"Section with LineWasteSuffix, {lineEnding} LineEnding");
                    yield return new TestCaseData(new object[] { new ConfigIniSection(sectionName) { LineEnding = lineEnding, LineWastePrefix = " ", LineWasteSuffix = " " }, $" [{sectionName}] {expectedLineEnding}" }).SetName($"Section with LineWaste, {lineEnding} LineEnding");

                }
            }

            [TestCaseSource(nameof(Cases_WriteHeader))]
            public void WriteHeader(ConfigIniSection section, string expectedText)
            {
                var writer = new StringWriter();
                section.WriteHeader(writer);
                Assert.That(writer.ToString(), Is.EqualTo(expectedText));
            }
        }

        [TestFixture]
        class Clone
        {
            [Test]
            public void When_CloningIniSection()
            {
                var section = new ConfigIniSection();
                section.LineWastePrefix = "AAA";
                section.LineWasteSuffix = "BBB";
                section.Name = "MySection";
                section.LineEnding = LineEnding.Unix;
                var token1 = new WhitespaceToken(new[] { "", "\t", "  " }, LineEnding.None);
                var token2 = new InstructionToken(InstructionType.Add, "MyKey", "MyVal");
                var token3 = new CommentToken();
                token3.AddLine("What a nice", LineEnding.Mac);
                token3.AddLine("Day", LineEnding.Unix);
                var token4 = new TextToken {Text = "SomeGarbageText", LineEnding = LineEnding.Windows};
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.Tokens.Add(token3);
                section.Tokens.Add(token4);

                var clone = ConfigIniSection.Clone(section);

                Assert.That(clone, Is.Not.SameAs(section));
                Assert.That(clone.Tokens.Count, Is.EqualTo(section.Tokens.Count));
                for(int t = 0; t < clone.Tokens.Count; t++)
                {
                    var originToken = section.Tokens[t];
                    var cloneToken = clone.Tokens[t];

                    Assert.That(cloneToken, Is.Not.SameAs(originToken));
                    Assert.That(cloneToken.GetType(), Is.EqualTo(originToken.GetType()));
                }

                StringWriter originWriter = new StringWriter();
                StringWriter cloneWriter = new StringWriter();

                section.Write(originWriter);
                clone.Write(cloneWriter);

                Assert.That(cloneWriter.ToString(), Is.EqualTo(originWriter.ToString()));

            }
        }

        [TestFixture]
        class MergeConsecutiveTokens
        {
            [Test]
            public void When_HasSingleWhitespaceTokens()
            {
                var section = new ConfigIniSection();
                var token1 = new WhitespaceToken(new[] { "", "\t", "  " }, LineEnding.None);
                section.Tokens.Add(token1);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(1));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(token1.GetStringLines(), Is.EquivalentTo(new[] { "", "\t", "  " }));
            }

            [Test]
            public void When_Has2ConsecutiveWhitespaceTokens()
            {
                var section = new ConfigIniSection();
                var token1 = new WhitespaceToken(new[] { "", "\t", "  " }, LineEnding.None);
                var token2 = new WhitespaceToken(new[] { " ", "\t", "" }, LineEnding.None);
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.MergeConsecutiveTokens();
                
                Assert.That(section.Tokens, Has.Count.EqualTo(1));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(token1.GetStringLines(), Is.EquivalentTo(new[] { "", "\t", "  ", " ", "\t", "" }));
            }

            [Test]
            public void When_Has3ConsecutiveWhitespaceTokens()
            {
                var section = new ConfigIniSection();
                var token1 = new WhitespaceToken(new[] { "", "\t", "  " }, LineEnding.None);
                var token2 = new WhitespaceToken(new[] { " ", "\t", "" }, LineEnding.None);
                var token3 = new WhitespaceToken(new[] { " \t\t" }, LineEnding.None);
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.Tokens.Add(token3);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(1));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(token1.GetStringLines(), Is.EquivalentTo(new[] { "", "\t", "  ", " ", "\t", "", " \t\t" }));
            }

            [Test]
            public void When_HasSingleCommentToken()
            {
                var section = new ConfigIniSection();
                var token1 = new CommentToken(new[] { "; Hey", ";Whats", " ;Up?" }, LineEnding.None);
                section.Tokens.Add(token1);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(1));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(token1.GetStringLines(), Is.EquivalentTo(new[] { "; Hey", ";Whats", " ;Up?" }));
            }

            [Test]
            public void When_Has2ConsecutiveCommentTokens()
            {
                var section = new ConfigIniSection();
                var token1 = new CommentToken(new[] { "; Hey", ";Whats", " ;Up?" }, LineEnding.None);
                var token2 = new CommentToken(new[] { ";Foo", ";Bar" }, LineEnding.None);
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(1));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(token1.GetStringLines(), Is.EquivalentTo(new[] { "; Hey", ";Whats", " ;Up?", ";Foo", ";Bar" }));
            }

            [Test]
            public void When_Has3ConsecutiveCommentTokens()
            {
                var section = new ConfigIniSection();
                var token1 = new CommentToken(new[] { "; Hey", ";Whats", " ;Up?" }, LineEnding.None);
                var token2 = new CommentToken(new[] { ";Foo", ";Bar" }, LineEnding.None);
                var token3 = new CommentToken(new[] { ";Baz" }, LineEnding.None);
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.Tokens.Add(token3);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(1));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(token1.GetStringLines(), Is.EquivalentTo(new[] { "; Hey", ";Whats", " ;Up?", ";Foo", ";Bar", ";Baz" }));
            }

            [Test]
            public void When_Has3NonConsecutiveTokens()
            {
                var section = new ConfigIniSection();
                var token1 = new CommentToken(new[] { "; Hey", ";Whats", " ;Up?" }, LineEnding.None);
                var token2 = new WhitespaceToken(new[] { " ", "\t", "" }, LineEnding.None);
                var token3 = new CommentToken(new[] { ";Baz" }, LineEnding.None);
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.Tokens.Add(token3);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(3));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(section.Tokens[1], Is.SameAs(token2));
                Assert.That(section.Tokens[2], Is.SameAs(token3));
                Assert.That(token1.GetStringLines(), Is.EquivalentTo(new[] { "; Hey", ";Whats", " ;Up?" }));
                Assert.That(token2.GetStringLines(), Is.EquivalentTo(new[] { " ", "\t", "" }));
                Assert.That(token3.GetStringLines(), Is.EquivalentTo(new[] { ";Baz" }));
            }

            [Test]
            public void When_Has4TokensWith2ConsecutiveInBetween()
            {
                var section = new ConfigIniSection();
                var token1 = new CommentToken(new[] { "; Hey", ";Whats", " ;Up?" }, LineEnding.None);
                var token2 = new WhitespaceToken(new[] { " ", "\t", "" }, LineEnding.None);
                var token3 = new WhitespaceToken(new[] { " \t\t" }, LineEnding.None);
                var token4 = new CommentToken(new[] { ";Baz" }, LineEnding.None);
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.Tokens.Add(token3);
                section.Tokens.Add(token4);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(3));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(section.Tokens[1], Is.SameAs(token2));
                Assert.That(section.Tokens[2], Is.SameAs(token4));
                Assert.That(token1.GetStringLines(), Is.EquivalentTo(new[] { "; Hey", ";Whats", " ;Up?" }));
                Assert.That(token2.GetStringLines(), Is.EquivalentTo(new[] { " ", "\t", "", " \t\t" }));
                Assert.That(token4.GetStringLines(), Is.EquivalentTo(new[] { ";Baz" }));
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
            public void When_ChangingToSpecificLineEnding(LineEnding targetLineEnding)
            {
                var section = new ConfigIniSection();
                section.LineEnding = LineEnding.Mac;
                var token1 = new InstructionToken(InstructionType.Add, "InstA", LineEnding.Windows);
                var token2 = new WhitespaceToken(new[] { " \t\t", " " }, LineEnding.None);
                var token3 = new CommentToken(new[] { ";Baz", "OO" }, LineEnding.Unix);
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.Tokens.Add(token3);

                section.NormalizeLineEndings(targetLineEnding);

                Assert.That(section.LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(token1.LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(token2.Lines[0].LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(token2.Lines[1].LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(token3.Lines[0].LineEnding, Is.EqualTo(targetLineEnding));
                Assert.That(token3.Lines[1].LineEnding, Is.EqualTo(targetLineEnding));
            }
        }
    }
}
