using System.Collections.Generic;

namespace UE4Config.Parsing
{
    public class CommentToken : MultilineToken
    {
        public CommentToken() : base() { }

        public CommentToken(IEnumerable<string> lines, LineEnding lineEnding) : base(lines, lineEnding) {}
    }
}
