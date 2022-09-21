using System;
using System.Collections.Generic;
using UE4Config.Parsing;

namespace UE4Config.Hierarchy
{
    public class VirtualConfigCache
    {
        public enum ConfigFileState
        {
            Unknown,
            /// <summary>
            /// Config has been loaded from file
            /// </summary>
            Loaded,
            /// <summary>
            /// Config has been created in memory because file could not be loaded
            /// </summary>
            Created
        }
        
        public ConfigFileReference FileReference { get; private set; }
        public ConfigFileState FileState { get; private set; }
        public ConfigIni ConfigIni { get; private set; }

        public VirtualConfigCache(ConfigFileReference configFileReference)
        {
            FileReference = configFileReference;
            FileState = ConfigFileState.Unknown;
        }

        public void SetConfigIni(ConfigIni configIni, bool wasLoaded)
        {
            ConfigIni = configIni;
            FileState = wasLoaded ? ConfigFileState.Loaded : ConfigFileState.Created;
        }
        
        public void InvalidateCache()
        {
            FileState = ConfigFileState.Unknown;
            ConfigIni = null;
        }
    }
}

