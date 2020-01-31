using System;

namespace UE4Config.Parsing
{
    public struct TextLine
    {
        public TextLine(string content = null, LineEnding lineEnding = LineEnding.None)
        {
            Content = content;
            LineEnding = lineEnding;
        }

        public string Content;
        public LineEnding LineEnding;

        public bool IsNull => Content == null;

        public override string ToString()
        {
            return Content + LineEnding.AsString();
        }

        public static implicit operator TextLine(string content)
        {
            return new TextLine(content);
        }
    }
}
