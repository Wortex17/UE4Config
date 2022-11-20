using System;
using System.Collections.Generic;

namespace UE4Config.Parsing
{
    public class ConfigIniSection
    {
        public string Name = null;
        public List<IniToken> Tokens = new List<IniToken>();
        public LineEnding LineEnding;

        /// <summary>
        /// Trimmed whitespace characters prefixing this sections header line
        /// </summary>
        public string LineWastePrefix = null;
        /// <summary>
        /// Trimmed whitespace characters suffixing this sections header line
        /// </summary>
        public string LineWasteSuffix = null;

        public ConfigIniSection() { }

        public ConfigIniSection(string name)
        {
            Name = name;
        }

        public ConfigIniSection(IEnumerable<IniToken> tokens)
        {
            if (tokens != null)
            {
                Tokens.AddRange(tokens);
            }
        }

        public ConfigIniSection(string name, IEnumerable<IniToken> tokens)
        {
            Name = name;
            if (tokens != null)
            {
                Tokens.AddRange(tokens);
            }
        }

        public static ConfigIniSection Clone(ConfigIniSection template)
        {
            var cloned = new ConfigIniSection(template.Name)
            {
                LineEnding = template.LineEnding,
                LineWastePrefix = template.LineWastePrefix,
                LineWasteSuffix = template.LineWasteSuffix
            };
            foreach (var templateToken in template.Tokens)
            {
                var clonedToken = templateToken.CreateClone();
                cloned.Tokens.Add(clonedToken);
            }
            return cloned;
        }

        /// <summary>
        /// Adds all instructions that could be found for the given key to the list, in order of declaration
        /// </summary>
        public void FindPropertyInstructions(string propertyKey, IList<InstructionToken> instructions)
        {
            foreach (var iniToken in Tokens)
            {
                if (iniToken is InstructionToken instruction)
                {
                    if (instruction.Key == propertyKey)
                    {
                        instructions.Add(instruction);
                    }
                }
            }
        }

        public IniToken GetLastToken()
        {
            if (Tokens.Count > 0)
            {
                return Tokens[Tokens.Count - 1];
            }

            return null;
        }

        /// <summary>
        /// Groups together instruction tokens, keeping their order of declaration intact
        /// </summary>
        public void GroupPropertyInstructions()
        {
            for (int i = Tokens.Count - 1; i > 0; i--)
            {
                var token = Tokens[i];
                if (token is InstructionToken instruction)
                {
                    FindPropertyInstructionGroup(instruction.Key, out _, out int lastIndex);
                    if (lastIndex < i)
                    {
                        Tokens.RemoveAt(i);
                        Tokens.Insert(lastIndex+1, token);
                    }
                }
            }
        }

        protected void FindPropertyInstructionGroup(string key, out int indexOfFirstInstruction, out int indexOfLastInstruction)
        {
            indexOfFirstInstruction = -1;
            indexOfLastInstruction = -1;
            bool hasGroup = false;
            for (int i = 0; i < Tokens.Count; i++)
            {
                var token = Tokens[i];
                bool isPartOfGroup = false;
                if (token is InstructionToken instruction)
                {
                    isPartOfGroup = instruction.Key == key;
                }

                if (isPartOfGroup && indexOfFirstInstruction < 0)
                {
                    indexOfFirstInstruction = i;
                }
                else if (!isPartOfGroup && indexOfFirstInstruction >= 0)
                {
                    indexOfLastInstruction = i - 1;
                    break;
                }
            }
        }

        /// <summary>
        /// Merges together supported consecutive tokens of the same type.
        /// This includes <see cref="WhitespaceToken"/> and <see cref="CommentToken"/>
        /// </summary>
        public void MergeConsecutiveTokens()
        {
            for (int i = Tokens.Count - 1; i > 0; i--)
            {
                var token = Tokens[i];
                var precedingToken = Tokens[i - 1];

                if (token is WhitespaceToken whitespace)
                {
                    if (precedingToken is WhitespaceToken precedingWhitespace)
                    {
                        precedingWhitespace.Lines.AddRange(whitespace.Lines);
                        Tokens.RemoveAt(i);
                    }
                } else if (token is CommentToken comment)
                {
                    if (precedingToken is CommentToken precedingComment)
                    {
                        precedingComment.Lines.AddRange(comment.Lines);
                        Tokens.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Condenses all whitespace to a maximum one newline.
        /// <seealso cref="WhitespaceToken.Condense()"/>.
        /// Note that consecutive whitespace tokens might still results in multiple newlines.
        /// Use <seealso cref="MergeConsecutiveTokens"/> before to avoid that.
        /// </summary>
        public void CondenseWhitespace()
        {
            for (int i = Tokens.Count - 1; i > 0; i--)
            {
                var token = Tokens[i];

                if (token is WhitespaceToken whitespace)
                {
                    whitespace.Condense();
                }
            }
        }

        /// <summary>
        /// Applies the given lineEnding to the section header and all tokens
        /// </summary>
        public void NormalizeLineEndings(LineEnding lineEnding)
        {
            LineEnding = lineEnding;
            foreach (var token in Tokens)
            {

                if (token is LineToken lineToken)
                {
                    lineToken.LineEnding = lineEnding;
                }
                else if (token is MultilineToken multilineToken)
                {
                    for (int t = 0; t < multilineToken.Lines.Count; t++)
                    {
                        var line = multilineToken.Lines[t];
                        line.LineEnding = lineEnding;
                        multilineToken.Lines[t] = line;
                    }
                }
            }
        }

        /// <summary>
        /// Writes this sections to a string
        /// </summary>
        public virtual void Write(ConfigIniWriter writer)
        {
            WriteHeader(writer);
            WriteTokens(writer);
        }

        /// <summary>
        /// Writes this sections header to a text blob
        /// </summary>
        public void WriteHeader(ConfigIniWriter writer)
        {
            if (Name != null)
            {
                if (!String.IsNullOrEmpty(LineWastePrefix))
                {
                    writer.Write(LineWastePrefix);
                }

                writer.Write($"[{Name}]");

                if (!String.IsNullOrEmpty(LineWasteSuffix))
                {
                    writer.Write(LineWasteSuffix);
                }
                LineEnding.WriteTo(writer); //Finish the line
            }
            //If there is no Name but there is line-waste, we consider this a parsing error.
        }

        /// <summary>
        /// Writes this sections <see cref="Tokens"/> to a text blob
        /// </summary>
        /// <param name="writer"></param>
        public virtual void WriteTokens(ConfigIniWriter writer)
        {
            foreach (var token in Tokens)
            {
                if (token != null)
                {
                    token.Write(writer);
                }
            }
        }
    }
}