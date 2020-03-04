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

        public static IEnumerable Cases_Write_TextToken
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

        public static IEnumerable Cases_Write_SetInstructionToken
        {
            get
            {
                InstructionType instructionType = InstructionType.Set;
                string tokenTypeName = "SetInstruction";
                string expectedString = "myKey=myValue";

                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue"), $"{expectedString}{Environment.NewLine}" }).SetName($"{tokenTypeName} Unspecified");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Unknown), $"{expectedString}{Environment.NewLine}" }).SetName($"{tokenTypeName} Unknown");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.None), expectedString }).SetName($"{tokenTypeName} None");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Unix), $"{expectedString}\n" }).SetName($"{tokenTypeName} Unix");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Windows), $"{expectedString}\r\n" }).SetName($"{tokenTypeName} Windows");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Mac), $"{expectedString}\r" }).SetName($"{tokenTypeName} Mac");
            }
        }

        public static IEnumerable Cases_Write_AddInstructionToken
        {
            get
            {
                InstructionType instructionType = InstructionType.Add;
                string tokenTypeName = "AddInstruction";
                string expectedString = "+myKey=myValue";

                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue"), $"{expectedString}{Environment.NewLine}" }).SetName($"{tokenTypeName} Unspecified");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Unknown), $"{expectedString}{Environment.NewLine}" }).SetName($"{tokenTypeName} Unknown");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.None), expectedString }).SetName($"{tokenTypeName} None");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Unix), $"{expectedString}\n" }).SetName($"{tokenTypeName} Unix");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Windows), $"{expectedString}\r\n" }).SetName($"{tokenTypeName} Windows");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Mac), $"{expectedString}\r" }).SetName($"{tokenTypeName} Mac");
            }
        }

        public static IEnumerable Cases_Write_AddForceInstructionToken
        {
            get
            {
                InstructionType instructionType = InstructionType.AddForce;
                string tokenTypeName = "AddForceInstruction";
                string expectedString = ".myKey=myValue";

                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue"), $"{expectedString}{Environment.NewLine}" }).SetName($"{tokenTypeName} Unspecified");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Unknown), $"{expectedString}{Environment.NewLine}" }).SetName($"{tokenTypeName} Unknown");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.None), expectedString }).SetName($"{tokenTypeName} None");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Unix), $"{expectedString}\n" }).SetName($"{tokenTypeName} Unix");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Windows), $"{expectedString}\r\n" }).SetName($"{tokenTypeName} Windows");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Mac), $"{expectedString}\r" }).SetName($"{tokenTypeName} Mac");
            }
        }

        public static IEnumerable Cases_Write_RemoveInstructionToken
        {
            get
            {
                InstructionType instructionType = InstructionType.Remove;
                string tokenTypeName = "RemoveInstruction";
                string expectedString = "-myKey=myValue";

                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue"), $"{expectedString}{Environment.NewLine}" }).SetName($"{tokenTypeName} Unspecified");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Unknown), $"{expectedString}{Environment.NewLine}" }).SetName($"{tokenTypeName} Unknown");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.None), expectedString }).SetName($"{tokenTypeName} None");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Unix), $"{expectedString}\n" }).SetName($"{tokenTypeName} Unix");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Windows), $"{expectedString}\r\n" }).SetName($"{tokenTypeName} Windows");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", "myValue", LineEnding.Mac), $"{expectedString}\r" }).SetName($"{tokenTypeName} Mac");
            }
        }

        public static IEnumerable Cases_Write_RemoveAllInstructionToken
        {
            get
            {
                InstructionType instructionType = InstructionType.Remove;
                string tokenTypeName = "RemoveAllInstruction";
                string expectedString = "-myKey";

                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey"), $"{expectedString}{Environment.NewLine}" }).SetName($"{tokenTypeName} Unspecified");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", LineEnding.Unknown), $"{expectedString}{Environment.NewLine}" }).SetName($"{tokenTypeName} Unknown");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", LineEnding.None), expectedString }).SetName($"{tokenTypeName} None");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", LineEnding.Unix), $"{expectedString}\n" }).SetName($"{tokenTypeName} Unix");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", LineEnding.Windows), $"{expectedString}\r\n" }).SetName($"{tokenTypeName} Windows");
                yield return new TestCaseData(new object[] { new InstructionToken(instructionType, "myKey", LineEnding.Mac), $"{expectedString}\r" }).SetName($"{tokenTypeName} Mac");
            }
        }

        [TestCaseSource(nameof(Cases_Write_TextToken))]
        [TestCaseSource(nameof(Cases_Write_SetInstructionToken))]
        [TestCaseSource(nameof(Cases_Write_AddInstructionToken))]
        [TestCaseSource(nameof(Cases_Write_AddForceInstructionToken))]
        [TestCaseSource(nameof(Cases_Write_RemoveInstructionToken))]
        [TestCaseSource(nameof(Cases_Write_RemoveAllInstructionToken))]
        public void Write(LineToken token, string expectedText)
        {
            var writer = new StringWriter();
            token.Write(writer);
            Assert.That(writer.ToString(), Is.EqualTo(expectedText));
        }
    }
}
