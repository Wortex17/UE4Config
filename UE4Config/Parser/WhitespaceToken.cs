using System;
using System.Collections.Generic;

namespace UE4Config.Parser
{
    /// <summary>
    /// Describes whitespace characters found within the INI file that are neither part of an instruction nor comment
    /// </summary>
    public class WhitespaceToken : IniToken
    {
        public List<string> Lines = new List<string>();

        /// <summary>
        /// Condenses the whietspace to a single newline
        /// </summary>
        public void Condense()
        {
            Lines.Clear();
            Lines.Add(Environment.NewLine);
        }
    }
}
