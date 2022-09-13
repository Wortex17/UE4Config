using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UE4Config.Hierarchy
{
    /// <summary>
    /// Translates ConfigFileReferences to file paths as use din UE4.27+ and above.
    /// Supports legacy config file paths, but assumes modern ones as default.
    /// </summary>
    public class ConfigFileProvider : IConfigFileProvider
    {
        /// <summary>
        /// Utility class to manage which platforms are using the legacy config model
        /// </summary>
        public class PlatformLegacyConfig
        {
            /// <summary>
            /// Enables a platform identifier to use the legacy config model
            /// %Project%/Config/%Platform%/%Platform%%Type%.ini
            /// instead of the modern one
            /// %Project%/Platforms/%Platform%/Config/%Platform%%Type%.ini
            /// </summary>
            public void SetPlatformLegacyConfig(string platformIdentifier, bool isLegacyConfig)
            {
                if(String.IsNullOrEmpty(platformIdentifier))
                    return;
                
                if (isLegacyConfig)
                {
                    m_LegacyPlatforms.Add(platformIdentifier);
                }
                else
                {
                    m_LegacyPlatforms.Remove(platformIdentifier);
                }
            }

            public bool IsPlatformUsingLegacyConfig(string platformIdentifier)
            {
                return m_LegacyPlatforms.Contains(platformIdentifier);
            }

            public IEnumerable<string> GetPlatformsUsingLegacyConfig()
            {
                return m_LegacyPlatforms;
            }
        
            public void ClearPlatformUsingLegacyConfig()
            {
                m_LegacyPlatforms.Clear();
            }
            
            private HashSet<string> m_LegacyPlatforms = new HashSet<string>();
        }
        
        public IConfigFileIOAdapter FileIOAdapter { get; private set; }
        
        public string EnginePath { get; private set; }
        
        public string ProjectPath { get; private set; }
        
        public bool IsSetup { get; private set; }

        public PlatformLegacyConfig EnginePlatformLegacyConfig { get; } = new PlatformLegacyConfig();
        
        public PlatformLegacyConfig ProjectPlatformLegacyConfig { get; } = new PlatformLegacyConfig();

        public void Setup(IConfigFileIOAdapter fileIOAdapter, string enginePath, string projectPath)
        {
            FileIOAdapter = fileIOAdapter;
            EnginePath = enginePath;
            ProjectPath = projectPath;
            IsSetup = FileIOAdapter != null && (EnginePath != null || ProjectPath != null);
        }

        public string ResolveConfigFilePath(ConfigFileReference reference)
        {
            if (!IsSetup)
                return null;

            string basePath;
            string configDirPath = "Config";
            string prefix = "";
            string platform = reference.Platform?.Identifier??"";
            string type = reference.Type??"";

            PlatformLegacyConfig platformLegacyConfig = null;
            
            switch (reference.Domain)
            {
                case ConfigDomain.Custom:
                    return null; //Not yet supported
                case ConfigDomain.EngineBase:
                    if (EnginePath == null)
                        return null;
                    if (!String.IsNullOrEmpty(reference.Platform?.Identifier) && String.IsNullOrEmpty(reference.Type))
                        return null;
                    basePath = EnginePath;
                    prefix = "Base";
                    platformLegacyConfig = EnginePlatformLegacyConfig;
                    break;
                case ConfigDomain.Engine:
                    if (EnginePath == null)
                        return null;
                    if (String.IsNullOrEmpty(reference.Type))
                        return null;
                    basePath = EnginePath;
                    platformLegacyConfig = EnginePlatformLegacyConfig;
                    break;
                case ConfigDomain.Project:
                    if (ProjectPath == null)
                        return null;
                    if (String.IsNullOrEmpty(reference.Type))
                        return null;
                    basePath = ProjectPath;
                    platformLegacyConfig = ProjectPlatformLegacyConfig;
                    if (String.IsNullOrWhiteSpace(platform))
                    {
                        platform = "Default";
                        platformLegacyConfig = null;
                    }
                    break;
                case ConfigDomain.ProjectGenerated:
                    if (EnginePath == null)
                        return null;
                    if (String.IsNullOrEmpty(reference.Type))
                        return null;
                    basePath = ProjectPath;
                    prefix = "Generated";
                    platformLegacyConfig = ProjectPlatformLegacyConfig;
                    break;
                default:
                    return null;
            }

            if (!String.IsNullOrEmpty(platform) && platformLegacyConfig != null)
            {
                if (platformLegacyConfig.IsPlatformUsingLegacyConfig(platform))
                {
                    configDirPath = GenerateLegacyPlatformConfigDirectory(platform);
                }
                else
                {
                    configDirPath = GeneratePlatformConfigDirectory(platform);
                }
            }
            
            return Path.Combine(basePath, configDirPath, $"{prefix}{platform}{type}.ini");
        }
        
        protected string GeneratePlatformConfigDirectory(string platformIdentifier)
        {
            return Path.Combine("Platforms", platformIdentifier, "Config");
        }
        
        protected string GenerateLegacyPlatformConfigDirectory(string platformIdentifier)
        {
            return Path.Combine("Config", platformIdentifier);
        }
    }
}

