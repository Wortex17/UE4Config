namespace UE4Config.Parsing
{
    public abstract class LineToken : IniToken
    {
        public LineEnding LineEnding;

        protected LineToken() { }

        protected LineToken(LineEnding lineEnding)
        {
            LineEnding = lineEnding;
        }
    }
}
