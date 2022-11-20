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
        /// Condenses the whitespace to a single empty line
        /// </summary>
        public void Condense()
        {
            //Look for any lineEnding to take over
            LineEnding condensedLineEnding = LineEnding.None;
            foreach (var textLine in Lines)
            {
                if (textLine.LineEnding != LineEnding.None)
                {
                    condensedLineEnding = textLine.LineEnding;
                }
            }
            if (Lines.Count > 0)
            {
                Lines.Clear();
                AddLine(String.Empty, condensedLineEnding);
            }
        }
    }
}
