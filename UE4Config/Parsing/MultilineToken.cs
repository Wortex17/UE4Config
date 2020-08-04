using System.Collections.Generic;
using System.IO;

namespace UE4Config.Parsing
{
    public abstract class MultilineToken : IniToken
    {
        public List<TextLine> Lines = new List<TextLine>();

        public void AddLine(string content, LineEnding lineEnding = LineEnding.Unknown)
        {
            Lines.Add(new TextLine(content, lineEnding));
        }

        public void AddLines(IEnumerable<string> contents, LineEnding lineEnding = LineEnding.Unknown)
        {
            foreach (var content in contents)
            {
                AddLine(content, lineEnding);
            }
        }

        /// <summary>
        /// Returns all lines converted into an array of strings.
        /// </summary>
        public string[] GetStringLines()
        {
            string[] strings = new string[Lines.Count];
            for(int i = 0; i < Lines.Count; i++)
            {
                strings[i] = Lines[i].ToString();
            }
            return strings;
        }


        protected MultilineToken() { }

        protected MultilineToken(IEnumerable<string> lines, LineEnding lineEnding)
        {
            AddLines(lines, lineEnding);
        }

        public override IniToken CreateClone()
        {
            var clone = base.CreateClone() as MultilineToken;
            clone.Lines.AddRange(Lines);
            return clone;
        }

        public override void Write(TextWriter writer)
        {
            foreach (var line in Lines)
            {
                if (!line.IsNull)
                {
                    writer.Write(line.Content);
                    line.LineEnding.WriteTo(writer);
                }
            }
        }
    }
}
