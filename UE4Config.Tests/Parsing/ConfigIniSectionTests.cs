using System;
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
        }

        [Test]
        public void When_ConstructedWithName()
        {
            string name = "/Script/Engine.PlayerInput";
            ConfigIniSection section = null;
            Assert.That(() => { section = new ConfigIniSection(name); }, Throws.Nothing);
            Assert.That(section.Name, Is.EqualTo(name));
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

        [TestFixture]
        class MergeConsecutiveTokens
        {
            [Test]
            public void When_HasSingleWhitespaceTokens()
            {
                var section = new ConfigIniSection();
                var token1 = new WhitespaceToken(new[] { "", "\t", "  " });
                section.Tokens.Add(token1);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(1));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(token1.Lines, Is.EquivalentTo(new[] { "", "\t", "  " }));
            }

            [Test]
            public void When_Has2ConsecutiveWhitespaceTokens()
            {
                var section = new ConfigIniSection();
                var token1 = new WhitespaceToken(new[] { "", "\t", "  " });
                var token2 = new WhitespaceToken(new[] { " ", "\t", "" });
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.MergeConsecutiveTokens();
                
                Assert.That(section.Tokens, Has.Count.EqualTo(1));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(token1.Lines, Is.EquivalentTo(new[] { "", "\t", "  ", " ", "\t", "" }));
            }

            [Test]
            public void When_Has3ConsecutiveWhitespaceTokens()
            {
                var section = new ConfigIniSection();
                var token1 = new WhitespaceToken(new[] { "", "\t", "  " });
                var token2 = new WhitespaceToken(new[] { " ", "\t", "" });
                var token3 = new WhitespaceToken(new[] { " \t\t" });
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.Tokens.Add(token3);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(1));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(token1.Lines, Is.EquivalentTo(new[] { "", "\t", "  ", " ", "\t", "", " \t\t" }));
            }

            [Test]
            public void When_HasSingleCommentToken()
            {
                var section = new ConfigIniSection();
                var token1 = new CommentToken(new[] { "; Hey", ";Whats", " ;Up?" });
                section.Tokens.Add(token1);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(1));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(token1.Lines, Is.EquivalentTo(new[] { "; Hey", ";Whats", " ;Up?" }));
            }

            [Test]
            public void When_Has2ConsecutiveCommentTokens()
            {
                var section = new ConfigIniSection();
                var token1 = new CommentToken(new[] { "; Hey", ";Whats", " ;Up?" });
                var token2 = new CommentToken(new[] { ";Foo", ";Bar" });
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(1));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(token1.Lines, Is.EquivalentTo(new[] { "; Hey", ";Whats", " ;Up?", ";Foo", ";Bar" }));
            }

            [Test]
            public void When_Has3ConsecutiveCommentTokens()
            {
                var section = new ConfigIniSection();
                var token1 = new CommentToken(new[] { "; Hey", ";Whats", " ;Up?" });
                var token2 = new CommentToken(new[] { ";Foo", ";Bar" });
                var token3 = new CommentToken(new[] { ";Baz" });
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.Tokens.Add(token3);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(1));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(token1.Lines, Is.EquivalentTo(new[] { "; Hey", ";Whats", " ;Up?", ";Foo", ";Bar", ";Baz" }));
            }

            [Test]
            public void When_Has3NonConsecutiveTokens()
            {
                var section = new ConfigIniSection();
                var token1 = new CommentToken(new[] { "; Hey", ";Whats", " ;Up?" });
                var token2 = new WhitespaceToken(new[] { " ", "\t", "" });
                var token3 = new CommentToken(new[] { ";Baz" });
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.Tokens.Add(token3);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(3));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(section.Tokens[1], Is.SameAs(token2));
                Assert.That(section.Tokens[2], Is.SameAs(token3));
                Assert.That(token1.Lines, Is.EquivalentTo(new[] { "; Hey", ";Whats", " ;Up?" }));
                Assert.That(token2.Lines, Is.EquivalentTo(new[] { " ", "\t", "" }));
                Assert.That(token3.Lines, Is.EquivalentTo(new[] { ";Baz" }));
            }

            [Test]
            public void When_Has4TokensWith2ConsecutiveInBetween()
            {
                var section = new ConfigIniSection();
                var token1 = new CommentToken(new[] { "; Hey", ";Whats", " ;Up?" });
                var token2 = new WhitespaceToken(new[] { " ", "\t", "" });
                var token3 = new WhitespaceToken(new[] { " \t\t" });
                var token4 = new CommentToken(new[] { ";Baz" });
                section.Tokens.Add(token1);
                section.Tokens.Add(token2);
                section.Tokens.Add(token3);
                section.Tokens.Add(token4);
                section.MergeConsecutiveTokens();

                Assert.That(section.Tokens, Has.Count.EqualTo(3));
                Assert.That(section.Tokens[0], Is.SameAs(token1));
                Assert.That(section.Tokens[1], Is.SameAs(token2));
                Assert.That(section.Tokens[2], Is.SameAs(token4));
                Assert.That(token1.Lines, Is.EquivalentTo(new[] { "; Hey", ";Whats", " ;Up?" }));
                Assert.That(token2.Lines, Is.EquivalentTo(new[] { " ", "\t", "", " \t\t" }));
                Assert.That(token4.Lines, Is.EquivalentTo(new[] { ";Baz" }));
            }
        }
    }
}
