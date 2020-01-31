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

        public override bool Equals(object obj)
        {
            if (obj is string)
            {
                return String.Equals(Content, obj);
            }
            return base.Equals(obj);
        }

        public static implicit operator string(TextLine line)
        {
            return line.Content;
        }

        public static implicit operator TextLine(string content)
        {
            return new TextLine(content);
        }

        public static bool operator ==(TextLine line, string str)
        {
            return line.Equals(str);
        }

        public static bool operator !=(TextLine line, string str)
        {
            return !line.Equals(str);
        }
    }
}
