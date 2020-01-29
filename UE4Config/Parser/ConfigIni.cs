using System;
using System.Collections.Generic;
using System.IO;

namespace UE4Config.Parser
{
    /// <remarks>
    /// Special parser for a "command"-like INI.
    /// Structure of the Ini file is preserved and can be evaluated for the actual value.
    /// </remarks>
    public class ConfigIni
    {
        public List<ConfigIniSection> Sections = new List<ConfigIniSection>();

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

            if (line.TrimStart().StartsWith(";"))
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

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                string sectionName = line.Substring(1, line.Length - 2);
                currentSection = new ConfigIniSection(sectionName);
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
                            type = InstructionType.AddOverride;
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

                var instruction = new InstructionToken();
                currentSection.Tokens.Add(instruction);
                instruction.Key = key;
                instruction.Value = value;
                instruction.InstructionType = type;
                return;
            }

            var text = new TextToken();
            text.Text = line;
            currentSection.Tokens.Add(text);
            return;
        }
    }
}