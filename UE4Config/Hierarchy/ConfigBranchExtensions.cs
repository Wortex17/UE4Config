using System;
using System.Collections.Generic;
using UE4Config.Parsing;

namespace UE4Config.Hierarchy
{
    public enum ConfigBranchPlatformSelector
    {
        NoneOrAny,
        None,
        Any,
        Specific
    }
    
    public static class ConfigBranchExtensions
    {
        /// <summary>
        /// Utility method to select the last config file of the branch that is still in the given domain and given platform
        /// </summary>
        /// <param name="configBranch"></param>
        /// <param name="configDomain"></param>
        /// <param name="platformSelector"></param>
        /// <param name="specifcPlatformIdentifier"></param>
        /// <returns>
        /// The most prioritized config file in the <paramref name="configDomain"/>, or an invalid <see cref="ConfigFileReference"/>
        /// </returns>
        public static ConfigFileReference SelectHeadConfig(this IReadOnlyList<ConfigFileReference> configBranch, ConfigDomain configDomain, ConfigBranchPlatformSelector platformSelector = ConfigBranchPlatformSelector.NoneOrAny, string specifcPlatformIdentifier = null)
        {
            var index = FindHeadConfigIndex(configBranch, configDomain, platformSelector, specifcPlatformIdentifier);
            if (index >= 0)
                return configBranch[index];

            return default;
        }
        
        public static ConfigIni SelectHeadConfig(this IReadOnlyList<ConfigIni> configBranch, ConfigDomain configDomain, ConfigBranchPlatformSelector platformSelector = ConfigBranchPlatformSelector.NoneOrAny, string specifcPlatformIdentifier = null)
        {
            var index = FindHeadConfigIndex(configBranch, configDomain, platformSelector, specifcPlatformIdentifier);
            if (index >= 0)
                return configBranch[index];

            return default;
        }

        private static int FindHeadConfigIndex(this IReadOnlyList<ConfigFileReference> configBranch, ConfigDomain configDomain, ConfigBranchPlatformSelector platformSelector, string specifcPlatformIdentifier)
        {
            for (int i = configBranch.Count - 1; i >= 0; i--)
            {
                var pivotRef = configBranch[i];
                if (IsHeadConfigReference(pivotRef, configDomain, platformSelector, specifcPlatformIdentifier))
                    return i;
            }

            return -1;
        }
        
        private static int FindHeadConfigIndex(this IReadOnlyList<ConfigIni> configBranch, ConfigDomain configDomain, ConfigBranchPlatformSelector platformSelector, string specifcPlatformIdentifier)
        {
            for (int i = configBranch.Count - 1; i >= 0; i--)
            {
                var pivot = configBranch[i];
                var pivotRef = pivot.Reference;
                if (IsHeadConfigReference(pivotRef, configDomain, platformSelector, specifcPlatformIdentifier))
                    return i;
            }

            return -1;
        }
        
        private static bool IsHeadConfigReference(ConfigFileReference reference, ConfigDomain configDomain, ConfigBranchPlatformSelector platformSelector, string specifcPlatformIdentifier)
        {
            if (reference.Domain != configDomain)
                return false;
            switch (platformSelector)
            {
                case ConfigBranchPlatformSelector.None:
                    if(reference.IsPlatformConfig) 
                        return false;
                    break;
                case ConfigBranchPlatformSelector.Any:
                    if(!reference.IsPlatformConfig) 
                        return false;
                    break;
                case ConfigBranchPlatformSelector.Specific:
                    if(reference.Platform == null || reference.Platform?.Identifier != specifcPlatformIdentifier)
                        return false;
                    break;
                case ConfigBranchPlatformSelector.NoneOrAny:
                default:
                    break;
            }

            return true;
        }
    }
}

