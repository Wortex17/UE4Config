using System;
using System.Collections.Generic;

namespace UE4Config.Parsing
{
    /// <summary>
    /// Describes whitespace characters found within the INI file that are neither part of an instruction nor comment
    /// </summary>
    public class WhitespaceToken : MultilineToken
    {
        public WhitespaceToken() : base() { }

        public WhitespaceToken(IEnumerable<string> lines, LineEnding lineEnding) : base(lines, lineEnding) { }

        /// <summary>
        /// Condenses the whitespace to a single newline
        /// </summary>
        public void Condense()
        {
            Condense(Environment.NewLine);
        }

        public void Condense(string newline)
        {
            if (Lines.Count > 0)
            {
                Lines.Clear();
                Lines.Add(newline);
            }
        }
    }
}
