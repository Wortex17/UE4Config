using System.Collections.Generic;

namespace UE4Config.Parser
{
    public class ConfigIniSection
    {
        public string Name = null;
        public List<IniToken> Tokens = new List<IniToken>();

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