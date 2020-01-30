using System.Collections.Generic;

namespace UE4Config.Parsing
{
    public abstract class MultilineToken : IniToken
    {
        public List<string> Lines = new List<string>();

        protected MultilineToken() { }

        protected MultilineToken(IEnumerable<string> lines)
        {
            Lines.AddRange(lines);
        }
    }
}
