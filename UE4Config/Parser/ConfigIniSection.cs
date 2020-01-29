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