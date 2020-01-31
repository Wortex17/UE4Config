using System.Collections.Generic;
using System.IO;

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

        public override void Write(TextWriter writer)
        {
            foreach (var line in Lines)
            {
                if (line != null)
                {
                    writer.WriteLine(line);
                }
            }
        }
    }
}
