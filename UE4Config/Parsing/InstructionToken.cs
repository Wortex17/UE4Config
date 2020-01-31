using System.IO;

namespace UE4Config.Parsing
{
    public class InstructionToken : IniToken
    {
        public InstructionType InstructionType;
        public string Key;
        public string Value;

        public InstructionToken() { }

        public InstructionToken(InstructionType type, string key, string value = null)
        {
            InstructionType = type;
            Key = key;
            Value = value;
        }

        public override void Write(TextWriter writer)
        {
            writer.Write(InstructionType.AsPrefixString());
            writer.Write(Key);
            if (Value != null)
            {
                writer.Write("=");
                writer.Write(Value);
            }
            writer.WriteLine();
        }
    }
}
