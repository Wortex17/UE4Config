using System.IO;

namespace UE4Config.Parsing
{
    /// <summary>
    /// Token representing a pure text line
    /// </summary>
    public class TextToken : LineToken
    {
        public string Text;

        public override void Write(TextWriter writer)
        {
            writer.WriteLine(Text);
        }
    }
}
