using System;
using System.Collections.Generic;

namespace UE4Config.Hierarchy
{
    /// <summary>
    /// The configuration tree provides a common interface to generate config chains that represent
    /// config types for specific target platforms.
    /// </summary>
    /// <remarks>
    /// This replaces the ConfigHierarchy used in older versions of this codebase
    /// This does not support layer-hierarchies below UE427+
    /// This does not yet support user-layers
    /// </remarks>
    // TODO: Add support for DataDrivenPlatformInfo.ini to generate platforms
    // TODO: Add support for (DataDrivenPlatformInfo:IniParent) to setup platform hierarchy
    public class ConfigReferenceTree427 : IConfigReferenceTree
    {
        public IConfigFileProvider FileProvider { get; private set; }

        public void Setup(IConfigFileProvider configFileProvider)
        {
            FileProvider = configFileProvider;
        }
        
        public void VisitConfigRoot(Action<ConfigFileReference> onConfig)
        {
            onConfig?.Invoke(GetOrCreateConfigFileReference(ConfigDomain.EngineBase, null, null));
        }

        public void VisitConfigBranch(string configType, string platformIdentifier, Action<ConfigFileReference> onConfig)
        {
            VisitConfigRoot(onConfig);
            onConfig?.Invoke(GetOrCreateConfigFileReference(ConfigDomain.EngineBase, null, configType));
            var targetPlatform = GetOrCreatePlatform(platformIdentifier);
            var platforms = new List<IConfigPlatform>();
            targetPlatform?.ResolvePlatformInheritance(ref platforms);
            foreach (var platform in platforms)
            {
                onConfig?.Invoke(GetOrCreateConfigFileReference(ConfigDomain.EngineBase, platform, configType));
            }
            onConfig?.Invoke(GetOrCreateConfigFileReference(ConfigDomain.Project, null, configType));
            onConfig?.Invoke(GetOrCreateConfigFileReference(ConfigDomain.ProjectGenerated, null, configType));
            foreach (var platform in platforms)
            {
                onConfig?.Invoke(GetOrCreateConfigFileReference(ConfigDomain.Engine, platform, configType));
                onConfig?.Invoke(GetOrCreateConfigFileReference(ConfigDomain.Project, platform, configType));
                onConfig?.Invoke(GetOrCreateConfigFileReference(ConfigDomain.ProjectGenerated, platform, configType));
            }
        }

        public IConfigPlatform GetPlatform(string platformIdentifier)
        {
            if (m_Platforms.ContainsKey(platformIdentifier))
            {
                return m_Platforms[platformIdentifier];
            }

            return null;
        }

        public IConfigPlatform RegisterPlatform(string platformIdentifier, IConfigPlatform parentPlatform = null)
        {
            var platform = new ConfigPlatform(platformIdentifier, parentPlatform);
            m_Platforms.Add(platformIdentifier, platform);
            return platform;
        }

        private IConfigPlatform GetOrCreatePlatform(string platformIdentifier)
        {
            if (String.IsNullOrEmpty(platformIdentifier))
            {
                return null;
            }
            
            if (!m_Platforms.ContainsKey(platformIdentifier))
            {
                m_Platforms.Add(platformIdentifier, new ConfigPlatform(platformIdentifier));
            }

            return m_Platforms[platformIdentifier];
        }

        private ConfigFileReference GetOrCreateConfigFileReference(ConfigDomain domain, IConfigPlatform platform,
            string type)
        {
            return new ConfigFileReference(domain, platform, type);
        }

        private Dictionary<string, IConfigPlatform> m_Platforms = new Dictionary<string, IConfigPlatform>();
    }
    
}

