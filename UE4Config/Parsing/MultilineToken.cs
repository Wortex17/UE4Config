using System.Collections.Generic;
using System.IO;

namespace UE4Config.Parsing
{
    public abstract class MultilineToken : IniToken
    {
        public List<string> Lines = new List<string>();
        public LineEnding SharedLineEnding;

        public void AddLine(string content, LineEnding lineEnding = LineEnding.Unknown)
        {
            Lines.Add(content);
            SharedLineEnding = lineEnding;
        }

        public void AddLines(IEnumerable<string> contents, LineEnding lineEnding = LineEnding.Unknown)
        {
            foreach (var content in contents)
            {
                AddLine(content, lineEnding);
            }
        }

        protected MultilineToken() { }

        protected MultilineToken(IEnumerable<string> lines)
        {
            AddLines(lines);
        }

        public override void Write(TextWriter writer)
        {
            foreach (var line in Lines)
            {
                if (line != null)
                {
                    writer.Write(line);
                    SharedLineEnding.WriteTo(writer);
                }
            }
        }
    }
}
