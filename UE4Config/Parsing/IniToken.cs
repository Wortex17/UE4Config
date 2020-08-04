using System;
using System.IO;

namespace UE4Config.Parsing
{
    public abstract class IniToken
    {
        /// <summary>
        /// Writes this ini token to a text blob
        /// </summary>
        public abstract void Write(TextWriter writer);

        /// <summary>
        /// Creates another instance of this token, that contains the same data and can be used else where
        /// </summary>
        public virtual IniToken CreateClone()
        {
            return (IniToken)Activator.CreateInstance(GetType());
        }
    }
}
