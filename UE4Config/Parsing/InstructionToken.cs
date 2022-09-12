using System.IO;

namespace UE4Config.Parsing
{
    public class InstructionToken : LineToken
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

        public InstructionToken(InstructionType type, string key, LineEnding lineEnding) : base(lineEnding)
        {
            InstructionType = type;
            Key = key;
        }

        public InstructionToken(InstructionType type, string key, string value, LineEnding lineEnding) : base(lineEnding)
        {
            InstructionType = type;
            Key = key;
            Value = value;
        }

        public override IniToken CreateClone()
        {
            var clone = base.CreateClone() as InstructionToken;
            clone.InstructionType = InstructionType;
            clone.Key = Key;
            clone.Value = Value;
            return clone;
        }

        public override void Write(ConfigIniWriter writer)
        {
            writer.Write(InstructionType.AsPrefixString());
            writer.Write(Key);
            if (Value != null)
            {
                writer.Write("=");
                writer.Write(Value);
            }
            LineEnding.WriteTo(writer);
        }
    }
}
