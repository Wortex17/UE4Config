using System;
using System.Collections.Generic;
using System.IO;

namespace UE4Config.Parsing
{
    /// <remarks>
    /// Special parser for a "command"-like INI.
    /// Structure of the Ini file is preserved and can be evaluated for the actual value.
    /// </remarks>
    public class ConfigIni
    {
        public List<ConfigIniSection> Sections = new List<ConfigIniSection>();

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

            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                ReadLine(line, ref currentSection);
            }
        }

        public void ReadLine(string line, ref ConfigIniSection currentSection)
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
                whitespace.Lines.Add(line);
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
                comment.Lines.Add(line);
                return;
            }

            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                string sectionName = trimmedLine.Substring(1, trimmedLine.Length - 2);
                currentSection = new ConfigIniSection(sectionName);
                currentSection.LineWastePrefix = String.IsNullOrEmpty(lineWastePrefix) ? null : lineWastePrefix;
                currentSection.LineWasteSuffix = String.IsNullOrEmpty(lineWasteSuffix) ? null : lineWasteSuffix;
                Sections.Add(currentSection);
                return;
            }

            if (line.StartsWith("!"))
            {
                var key = line.Substring(1);
                if (!String.IsNullOrWhiteSpace(key))
                {
                    var instruction = new InstructionToken();
                    currentSection.Tokens.Add(instruction);
                    instruction.Key = key;
                    instruction.InstructionType = InstructionType.RemoveAll;
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

                var instruction = new InstructionToken(type, key, value);
                currentSection.Tokens.Add(instruction);
                return;
            }

            var text = new TextToken();
            text.Text = line;
            currentSection.Tokens.Add(text);
            return;
        }

        /// <summary>
        /// Merges together <see cref="Sections"/> that share a <see cref="ConfigIniSection.Name"/>.
        /// <see cref="ConfigIniSection.Tokens"/> will be merge din order of the sections
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
    }
}