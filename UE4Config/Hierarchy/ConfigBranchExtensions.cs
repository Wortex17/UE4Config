using System;
using System.Collections.Generic;

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

            return default(ConfigFileReference);
        }

        private static int FindHeadConfigIndex(this IReadOnlyList<ConfigFileReference> configBranch, ConfigDomain configDomain, ConfigBranchPlatformSelector platformSelector, string specifcPlatformIdentifier)
        {
            for (int i = configBranch.Count - 1; i >= 0; i--)
            {
                var pivot = configBranch[i];
                if (pivot.Domain != configDomain)
                    continue;
                switch (platformSelector)
                {
                    case ConfigBranchPlatformSelector.None:
                        if(pivot.IsPlatformConfig) 
                            continue;
                        break;
                    case ConfigBranchPlatformSelector.Any:
                        if(!pivot.IsPlatformConfig) 
                            continue;
                        break;
                    case ConfigBranchPlatformSelector.Specific:
                        if(pivot.Platform?.Identifier != specifcPlatformIdentifier)
                            continue;
                        break;
                    case ConfigBranchPlatformSelector.NoneOrAny:
                    default:
                        break;
                }
                return i;
            }

            return -1;
        }
    }
}

