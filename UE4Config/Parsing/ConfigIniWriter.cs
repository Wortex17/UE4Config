using System.IO;

namespace UE4Config.Parsing
{
    /// <remarks>
    /// A wrapper for writers of ConfigIni files.
    /// </remarks>
    public class ConfigIniWriter
    {
        public TextWriter ContentWriter = null;
        /// <summary>
        /// The Unreal Engine 4 config file serializer has a quirk that always makes the file end with 2 line endings
        /// (one for the last token, one empty line).
        /// Enable this to keep consistent with UE4 and avoid additional changes e.g. in source control.
        /// </summary>
        public bool AppendQuirkFileEnding = false;

        public ConfigIniWriter(TextWriter textContentWriter)
        {
            ContentWriter = textContentWriter;
        }

        public void Write(string content)
        {
            ContentWriter.Write(content);
        }

        public override string ToString()
        {
            var writtenString = ContentWriter.ToString();
            if (AppendQuirkFileEnding)
            {
                if (!writtenString.EndsWith("\n\n"))
                {
                    if (!writtenString.EndsWith("\n"))
                    {
                        writtenString += "\n\n";
                    }
                    else
                    {
                        writtenString += "\n";
                    }
                }
            }
            return writtenString;
        }
    }
}