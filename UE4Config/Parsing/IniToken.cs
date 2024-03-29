﻿using System;

namespace UE4Config.Parsing
{
    /**
     * Represents a single token of UEs configuration INIs.
     * Each token usually represents a comment, whitespace or an instruction to modify a named value.
     */
    public abstract class IniToken
    {
        /// <summary>
        /// Writes this ini token to a text blob
        /// </summary>
        public abstract void Write(ConfigIniWriter writer);

        /// <summary>
        /// Creates another instance of this token, that contains the same data and can be used else where
        /// </summary>
        public virtual IniToken CreateClone()
        {
            return (IniToken)Activator.CreateInstance(GetType());
        }
    }
}
