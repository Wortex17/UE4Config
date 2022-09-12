using System;
using System.ComponentModel;
using System.IO;

namespace UE4Config.Parsing
{
    public enum LineEnding
    {
        /// <summary>
        /// Unknown line-ending - will assume environments default newline
        /// </summary>
        Unknown,
        /// <summary>
        /// No line-ending
        /// </summary>
        None,
        /// <summary>
        /// Newline, Unix and Unix-like systems (Linux, macOS, FreeBSD, AIX, Xenix, etc.)
        /// \n
        /// </summary>
        Unix,
        /// <summary>
        /// Newline, Microsoft Windows, DOS (MS-DOS, PC DOS, etc.)
        /// \r\n
        /// </summary>
        Windows,
        /// <summary>
        /// Newline, Commodore 8-bit machines (C64, C128), Acorn BBC, ZX Spectrum, TRS-80, Apple II series, Oberon, the classic Mac OS
        /// \r
        /// </summary>
        Mac
    }

    public static class LineEndingExtensions
    {
        public static string AsString(this LineEnding lineEnding)
        {
            switch (lineEnding)
            {
                case LineEnding.None:
                    return "";
                case LineEnding.Unix:
                    return "\n";
                case LineEnding.Windows:
                    return "\r\n";
                case LineEnding.Mac:
                    return "\r";
                case LineEnding.Unknown:
                    return Environment.NewLine;
                default:
                    throw new InvalidEnumArgumentException(nameof(lineEnding), (int)lineEnding, typeof(LineEnding));
            }
        }

        public static void WriteTo(this LineEnding lineEnding, TextWriter writer)
        {
            switch (lineEnding)
            {
                case LineEnding.None:
                    break;
                case LineEnding.Unknown:
                    writer.WriteLine();
                    break;
                default:
                    writer.Write(lineEnding.AsString());
                    break;
            }
        }
        
        public static void WriteTo(this LineEnding lineEnding, ConfigIniWriter writer)
        {
            writer.WriteLineEnding(lineEnding);
        }
    }
}
