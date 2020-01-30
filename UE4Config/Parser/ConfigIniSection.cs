using System.Collections.Generic;

namespace UE4Config.Parser
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
    }
}