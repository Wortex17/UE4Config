using System.IO;

namespace UE4Config.Parsing
{
    public abstract class IniToken
    {
        /// <summary>
        /// Writes this ini token to a text blob
        /// </summary>
        public abstract void Write(TextWriter writer);
    }
}
