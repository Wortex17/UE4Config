using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UE4Config.Evaluation;

namespace UE4Config.Parsing
{
    /// <remarks>
    /// Special parser for a "command"-like INI.
    /// Structure of the Ini file is preserved and can be evaluated for the actual value.
    /// </remarks>
    public class ConfigIni
    {
        public string Name = null;
        public List<ConfigIniSection> Sections = new List<ConfigIniSection>();

        public ConfigIni() { }

        public ConfigIni(string name)
        {
            Name = name;
        }

        public ConfigIni(IEnumerable<ConfigIniSection> sections)
        {
            if (sections != null)
            {
                Sections.AddRange(sections);
            }
        }

        public ConfigIni(string name, IEnumerable<ConfigIniSection> sections)
        {
            Name = name;
            if (sections != null)
            {
                Sections.AddRange(sections);
            }
        }

        public void FindPropertyInstructions(string sectionName, string propertyKey, IList<InstructionToken> instructions)
        {
            foreach (var iniSection in Sections)
            {
                if (iniSection.Name == sectionName)
                {
                    iniSection.FindPropertyInstructions(propertyKey, instructions);
                }
            }
        }

        public void Read(TextReader reader)
        {
            ConfigIniSection currentSection = null;
            if (Sections.Count > 0)
            {
                currentSection = Sections[Sections.Count - 1];
            }

            if (currentSection == null)
            {
                // Default section without a Name
                currentSection = new ConfigIniSection();
                Sections.Add(currentSection);
            }

            char[] readBuffer = new char[1];
            char currentChar = '\0';
            StringBuilder stringBuilder = new StringBuilder();
            bool hasLine = false;

            while (reader.Read(readBuffer, 0, 1) != 0)
            {
                if (!hasLine)
                {
                    //Start a line
                    stringBuilder.Clear();
                    hasLine = true;
                }

                var previousChar = currentChar;
                currentChar = readBuffer[0];

                if (currentChar == '\n')
                {
                    if (previousChar == '\r')
                    {
                        ReadLine(stringBuilder.ToString(), LineEnding.Windows, ref currentSection);
                        //Consume the line
                        stringBuilder.Clear();
                        hasLine = false;
                    }
                    else
                    {
                        ReadLine(stringBuilder.ToString(), LineEnding.Unix, ref currentSection);
                        //Consume the line
                        stringBuilder.Clear();
                        hasLine = false;
                    }
                }
                else if (currentChar == '\r')
                {
                    if (previousChar == '\r')
                    {
                        //We received an \r earlier, but this character is not an \n, so treat the previous one as line break Mac-style
                        ReadLine(stringBuilder.ToString(), LineEnding.Mac, ref currentSection);//Consume the line, but reset it to a new one for the current character
                        stringBuilder.Clear();
                    }
                    //Wait and see if there's a '\n' upcoming to make a LineEnding.Windows
                    continue;
                }
                else if (previousChar == '\r')
                {
                    //We received an \r earlier, but this character is not an \n, so treat the previous one as line break Mac-style
                    ReadLine(stringBuilder.ToString(), LineEnding.Mac, ref currentSection);
                    //Consume the line, but reset it to a new one to capture the current character
                    stringBuilder.Clear();
                    hasLine = true;
                }

                if (hasLine)
                {
                    stringBuilder.Append(currentChar);
                }
            }

            if (hasLine)
            {
                //Finish a possible open Mac-line break
                if (currentChar == '\r')
                {
                    ReadLine(stringBuilder.ToString(), LineEnding.Mac, ref currentSection);
                }
                else
                {
                    //Otherwise, treat the final line as one without ending
                    ReadLine(stringBuilder.ToString(), LineEnding.None, ref currentSection);
                }
            }
        }

        public void ReadLine(string line, LineEnding lineEnding, ref ConfigIniSection currentSection)
        {
            if (line == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                var lastToken = currentSection.GetLastToken();
                var whitespace = lastToken as WhitespaceToken;
                if (whitespace == null)
                {
                    whitespace = new WhitespaceToken();
                    currentSection.Tokens.Add(whitespace);
                }
                whitespace.AddLine(line, lineEnding);
                return;
            }

            string endTrimmedLine = line.TrimEnd();
            string lineWasteSuffix = line.Substring(endTrimmedLine.Length);
            string trimmedLine = endTrimmedLine.TrimStart();
            string lineWastePrefix = line.Substring(0, endTrimmedLine.Length - trimmedLine.Length);

            if (trimmedLine.StartsWith(";"))
            {
                var lastToken = currentSection.GetLastToken();
                var comment = lastToken as CommentToken;
                if (comment == null)
                {
                    comment = new CommentToken();
                    currentSection.Tokens.Add(comment);
                }
                comment.AddLine(line, lineEnding);
                return;
            }

            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                string sectionName = trimmedLine.Substring(1, trimmedLine.Length - 2);
                currentSection = new ConfigIniSection(sectionName);
                currentSection.LineWastePrefix = String.IsNullOrEmpty(lineWastePrefix) ? null : lineWastePrefix;
                currentSection.LineWasteSuffix = String.IsNullOrEmpty(lineWasteSuffix) ? null : lineWasteSuffix;
                currentSection.LineEnding = lineEnding;
                Sections.Add(currentSection);
                return;
            }

            if (line.StartsWith("!"))
            {
                var key = line.Substring(1);
                if (!String.IsNullOrWhiteSpace(key))
                {
                    var instruction = new InstructionToken(InstructionType.RemoveAll, key, lineEnding);
                    currentSection.Tokens.Add(instruction);
                    return;
                }
            }

            int separatorIdx = line.IndexOf("=");
            if (separatorIdx >= 0)
            {
                string key = line.Substring(0, separatorIdx);
                string value = line.Substring(separatorIdx + 1);

                InstructionType type = InstructionType.Set;
                if (key.Length > 0)
                {
                    switch (key[0])
                    {
                        case '+':
                            type = InstructionType.Add;
                            key = key.Substring(1);
                            break;
                        case '.':
                            type = InstructionType.AddForce;
                            key = key.Substring(1);
                            break;
                        case '-':
                            type = InstructionType.Remove;
                            key = key.Substring(1);
                            break;
                        default:
                            break;
                    }
                }

                var instruction = new InstructionToken(type, key, value, lineEnding);
                currentSection.Tokens.Add(instruction);
                return;
            }

            var text = new TextToken();
            text.Text = line;
            text.LineEnding = lineEnding;
            currentSection.Tokens.Add(text);
            return;
        }

        public void ReadLineWithoutLineEnding(string line, ref ConfigIniSection currentSection)
        {
            ReadLine(line, LineEnding.None, ref currentSection);
        }

        /// <summary>
        /// Writes the whole config to a text blob
        /// </summary>
        /// <param name="writer"></param>
        public void Write(TextWriter writer)
        {
            foreach (var section in Sections)
            {
                if (section == null)
                    continue;

                section.Write(writer);
            }
        }

        public void EvaluatePropertyValues(string sectionName, string propertyKey, IList<string> values, PropertyEvaluator evaluator = null)
        {
            evaluator = PropertyEvaluator.CustomOrDefault(evaluator);
            evaluator.EvaluatePropertyValues(this, sectionName, propertyKey, values);
        }

        /// <summary>
        /// Merges together <see cref="Sections"/> that share a <see cref="ConfigIniSection.Name"/>.
        /// <see cref="Sections"/> will be order in order of appearance of the first section of their name.
        /// <see cref="ConfigIniSection.Tokens"/> will be merged in by simply appending their appearances.
        /// </summary>
        public void MergeDuplicateSections()
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                var pivotSection = Sections[i];
                for (int j = i+1; j < Sections.Count; j++)
                {
                    var otherSection = Sections[j];
                    if (otherSection.Name == pivotSection.Name)
                    {
                        pivotSection.Tokens.AddRange(otherSection.Tokens);
                        Sections.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        /// <summary>
        /// Condenses all whitespace to a maximum one newline.
        /// <seealso cref="WhitespaceToken.Condense()"/>.
        /// Will delete whitespace tokens directly following other whitespace tokens.
        /// </summary>
        public void CondenseWhitespace()
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                var pivotSection = Sections[i];
                pivotSection.MergeConsecutiveTokens();
                pivotSection.CondenseWhitespace();
            }
        }

        /// <summary>
        /// Automatically detects the line ending this config uses, based on the first encounter
        /// of a specified line ending.
        /// Used for <seealso cref="NormalizeLineEndings()"/>
        /// </summary>
        public LineEnding AutoDetectLineEnding()
        {
            foreach (var section in Sections)
            {
                if (section.LineEnding != LineEnding.Unknown)
                    return section.LineEnding;

                foreach (var token in section.Tokens)
                {
                    if (token is LineToken lineToken)
                    {
                        if (lineToken.LineEnding != LineEnding.Unknown)
                            return lineToken.LineEnding;
                    }

                    if (token is MultilineToken multilineToken)
                    {
                        foreach (var line in multilineToken.Lines)
                        {
                            if (line.LineEnding != LineEnding.Unknown)
                                return line.LineEnding;
                        }
                    }
                }
            }
            return LineEnding.Unknown;
        }

        /// <summary>
        /// Makes sure all lined use the same line ending, detected automatically via <seealso cref="AutoDetectLineEnding"/>
        /// </summary>
        public void NormalizeLineEndings()
        {
            NormalizeLineEndings(AutoDetectLineEnding());
        }

        /// <summary>
        /// Makes sure all lined use the same, given line ending
        /// </summary>
        public void NormalizeLineEndings(LineEnding lineEnding)
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                var pivotSection = Sections[i];
                pivotSection.NormalizeLineEndings(lineEnding);
            }
        }
    }
}