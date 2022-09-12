using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UE4Config.Parsing;

namespace UE4Config.Tests.Parsing
{
    [TestFixture]
    class ConfigIniWriterTests
    {
        
        struct ConfigIniCase
        {
            public string CaseName;
            public string Content;
            public bool NoLEInformed;
        }
        
        struct LineEndingCase
        {
            public string CaseName;
            public LineEnding Content;
            public string ContentAsString => Content.AsString();
        }

        static IEnumerable<LineEndingCase> Cases_LineEndings
        {
            get
            {
                yield return new LineEndingCase()
                {
                    CaseName = "Unix",
                    Content = LineEnding.Unix
                };
                yield return new LineEndingCase()
                {
                    CaseName = "Windows",
                    Content = LineEnding.Windows
                };
                yield return new LineEndingCase()
                {
                    CaseName = "Mac",
                    Content = LineEnding.Mac
                };
            }
        }
        
        static IEnumerable<ConfigIniCase> Get_Cases_ConfigInis(string lineEnding)
        {
            yield return new ConfigIniCase()
            {
                CaseName = "Empty",
                Content = "",
                NoLEInformed = true
            };
            yield return new ConfigIniCase()
            {
                CaseName = "Sole Header (Leading LE)",
                Content = "[MySection]",
                NoLEInformed = true
            };
            yield return new ConfigIniCase()
            {
                CaseName = "One Section",
                Content = "[MySection]"+lineEnding+
                          "MyProp=4"
            };
            yield return new ConfigIniCase()
            {
                CaseName = "One Section + Leading Comment",
                Content = ";MyComment"+lineEnding+
                          "[MySection]"+lineEnding+
                          "MyProp=4"
            };
            yield return new ConfigIniCase()
            {
                CaseName = "One Section + Closing Comment",
                Content = "[MySection]"+lineEnding+
                          "MyProp=4"+lineEnding+
                          ";MyComment"
            };
            yield return new ConfigIniCase()
            {
                CaseName = "One Section with superfl. newline",
                Content = "[MySection]"+lineEnding+
                          "MyProp=4"+lineEnding+
                          lineEnding+
                          "MyProp3=44"
            };
            yield return new ConfigIniCase()
            {
                CaseName = "Two Sections",
                Content = "[MySection]"+lineEnding+
                          "MyProp=4"+lineEnding+
                          lineEnding+
                          "[MySection2]"+lineEnding+
                          "MyProp2=4"
            };
        }
            
        [TestFixture]
        class AppendQuirkFileEnding
        {
            private ConfigIniWriter Writer;
            
            [SetUp]
            public void Setup()
            {
                Writer = new ConfigIniWriter(new StringWriter());
                Writer.AppendQuirkFileEnding = true;
            }
            
            static IEnumerable Cases_When_HasDoubleLineEndingAtEndOfFile
            {
                get
                {
                    foreach (var lineEndingCase in Cases_LineEndings)
                    {
                        foreach (var configIniCase in Get_Cases_ConfigInis(lineEndingCase.ContentAsString))
                        {
                            string caseSuffix = lineEndingCase.ContentAsString + lineEndingCase.ContentAsString;
                            yield return new TestCaseData(
                                    new object[] { configIniCase.Content + caseSuffix })
                                .SetName(configIniCase.CaseName + $"(LE:{lineEndingCase.CaseName})");
                        }
                    }
                }
            }

            [TestCaseSource(nameof(Cases_When_HasDoubleLineEndingAtEndOfFile))]
            public void When_HasDoubleLineEndingAtEndOfFile(string original)
            {
                var config = new ConfigIni();
                config.Read(new StringReader(original));
                
                config.Write(Writer);
                Assert.That(Writer.ToString(), Is.EqualTo(original));
            }

            static IEnumerable Cases_When_HasTooFewLineEndingAtEndOfFile
            {
                get
                {
                    foreach (var lineEndingCase in Cases_LineEndings)
                    {
                        foreach (var configIniCase in Get_Cases_ConfigInis(lineEndingCase.ContentAsString))
                        {
                            const string caseVariantName = "No LE@EOF";
                            const string caseSuffix = "";
                            yield return new TestCaseData(
                                    new object[] { configIniCase.Content + caseSuffix, lineEndingCase.Content, configIniCase.NoLEInformed })
                                .SetName(configIniCase.CaseName + ", " + caseVariantName + $" (LE:{lineEndingCase.CaseName})");
                        }
                        foreach (var configIniCase in Get_Cases_ConfigInis(lineEndingCase.ContentAsString))
                        {
                            const string caseVariantName = "Single LE@EOF";
                            string caseSuffix = lineEndingCase.ContentAsString;
                            yield return new TestCaseData(
                                    new object[] { configIniCase.Content + caseSuffix, lineEndingCase.Content, configIniCase.NoLEInformed })
                                .SetName(configIniCase.CaseName + ", " + caseVariantName + $" (LE:{lineEndingCase.CaseName})");
                        }
                    }
                }
            }
            static IEnumerable Cases_When_HasTooManyLineEndingAtEndOfFile
            {
                get
                {
                    foreach (var lineEndingCase in Cases_LineEndings)
                    {
                        foreach (var configIniCase in Get_Cases_ConfigInis(lineEndingCase.ContentAsString))
                        {
                            const string caseVariantName = "Three LEs@EOF";
                            string caseSuffix = lineEndingCase.ContentAsString+lineEndingCase.ContentAsString+lineEndingCase.ContentAsString;
                            yield return new TestCaseData(
                                    new object[] { configIniCase.Content + caseSuffix, lineEndingCase.Content, configIniCase.NoLEInformed })
                                .SetName(configIniCase.CaseName + ", " + caseVariantName + $" (LE:{lineEndingCase.CaseName})");
                        }
                    }
                }
            }

            [TestCaseSource(nameof(Cases_When_HasTooFewLineEndingAtEndOfFile))]
            [TestCaseSource(nameof(Cases_When_HasTooManyLineEndingAtEndOfFile))]
            public void When_HasNoDoubleLineEndingAtEndOfFile(string original, LineEnding lineEnding, bool noLEInformed)
            {
                var config = new ConfigIni();
                config.Read(new StringReader(original));
                if (noLEInformed)
                {
                    Writer.LineEnding = lineEnding;
                }

                var leStr = lineEnding.AsString();
                config.Write(Writer);
                var writtenString = Writer.ToString();
                Assert.That(writtenString.Length - original.Length, Is.InRange(0, leStr.Length*2));
                Assert.That(writtenString.Substring(writtenString.Length-leStr.Length*2), Is.EqualTo(leStr+leStr));
                Assert.That(writtenString.Substring(0, original.Length), Is.EqualTo(original));
            }
        }
        
    }
}
