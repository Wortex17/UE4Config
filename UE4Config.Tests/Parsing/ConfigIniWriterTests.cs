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
        }
        static IEnumerable<ConfigIniCase> Cases_ConfigInis
        {
            get
            {
                yield return new ConfigIniCase()
                {
                    CaseName = "Empty",
                    Content = ""
                };
                yield return new ConfigIniCase()
                {
                    CaseName = "Sole Header",
                    Content = "[MySection]"
                };
                yield return new ConfigIniCase()
                {
                    CaseName = "One Section",
                    Content = "[MySection]\n" +
                              "MyProp=4"
                };
                yield return new ConfigIniCase()
                {
                    CaseName = "One Section + Leading Comment",
                    Content = ";MyComment\n" +
                              "[MySection]\n" +
                              "MyProp=4"
                };
                yield return new ConfigIniCase()
                {
                    CaseName = "One Section + Closing Comment",
                    Content = "[MySection]\n" +
                              "MyProp=4\n" +
                              ";MyComment"
                };
                yield return new ConfigIniCase()
                {
                    CaseName = "One Section with superfl. newline",
                    Content = "[MySection]\n" + 
                              "MyProp=4\n" + 
                              "\n" + 
                              "MyProp3=44"
                };
                yield return new ConfigIniCase()
                {
                    CaseName = "Two Sections",
                    Content = "[MySection]\n" +
                              "MyProp=4\n" +
                              "\n" +
                              "[MySection2]\n" +
                              "MyProp2=4"
                };
            }
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
                    foreach (var configIniCase in Cases_ConfigInis)
                    {
                        const string caseSuffix = "\n\n";
                        yield return new TestCaseData(
                                new object[] { configIniCase.Content + caseSuffix })
                            .SetName(configIniCase.CaseName);
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
                    foreach (var configIniCase in Cases_ConfigInis)
                    {
                        const string caseVariantName = "No LE";
                        const string caseSuffix = "";
                        yield return new TestCaseData(
                            new object[] { configIniCase.Content + caseSuffix })
                            .SetName(configIniCase.CaseName + ", " + caseVariantName);
                    }
                    foreach (var configIniCase in Cases_ConfigInis)
                    {
                        const string caseVariantName = "Single LE";
                        const string caseSuffix = "\n";
                        yield return new TestCaseData(
                                new object[] { configIniCase.Content + caseSuffix })
                            .SetName(configIniCase.CaseName + ", " + caseVariantName);
                    }
                }
            }
            static IEnumerable Cases_When_HasTooManyLineEndingAtEndOfFile
            {
                get
                {
                    foreach (var configIniCase in Cases_ConfigInis)
                    {
                        const string caseVariantName = "Three LEs";
                        const string caseSuffix = "\n\n\n";
                        yield return new TestCaseData(
                                new object[] { configIniCase.Content + caseSuffix })
                            .SetName(configIniCase.CaseName + ", " + caseVariantName);
                    }
                }
            }

            [TestCaseSource(nameof(Cases_When_HasTooFewLineEndingAtEndOfFile))]
            [TestCaseSource(nameof(Cases_When_HasTooManyLineEndingAtEndOfFile))]
            public void When_HasNoDoubleLineEndingAtEndOfFile(string original)
            {
                var config = new ConfigIni();
                config.Read(new StringReader(original));
                
                config.Write(Writer);
                var writtenString = Writer.ToString();
                Assert.That(writtenString.Length - original.Length, Is.InRange(0, 2));
                Assert.That(writtenString.Substring(writtenString.Length-2), Is.EqualTo("\n\n"));
                Assert.That(writtenString.Substring(0, original.Length), Is.EqualTo(original));
            }
        }
        
    }
}
