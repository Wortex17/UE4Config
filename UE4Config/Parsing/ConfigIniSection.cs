using System.Collections.Generic;

namespace UE4Config.Parsing
{
    public class ConfigIniSection
    {
        public string Name = null;
        public List<IniToken> Tokens = new List<IniToken>();

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
    }
}