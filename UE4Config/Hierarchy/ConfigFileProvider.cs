using System;
using System.Collections.Generic;
using System.IO;

namespace UE4Config.Hierarchy
{
    public class ConfigFileProvider : IConfigFileProvider
    {
        public string EnginePath { get; private set; }
        
        public string ProjectPath { get; private set; }
        
        public bool IsSetup { get; private set; }

        public void Setup(string enginePath, string projectPath)
        {
            EnginePath = enginePath;
            ProjectPath = projectPath;
            IsSetup = EnginePath != null || ProjectPath != null;
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

            if (!String.IsNullOrEmpty(platform))
            {
                if (IsPlatformUsingLegacyConfig(platform))
                {
                    configDirPath = GenerateLegacyPlatformConfigDirectory(platform);
                }
                else
                {
                    configDirPath = GeneratePlatformConfigDirectory(platform);
                }
            }
            
            switch (reference.Domain)
            {
                case ConfigDomain.Custom:
                    return null; //Not yet supported
                case ConfigDomain.EngineBase:
                    if (EnginePath == null)
                        return null;
                    basePath = EnginePath;
                    prefix = "Base";
                    break;
                case ConfigDomain.Engine:
                    if (EnginePath == null)
                        return null;
                    basePath = EnginePath;
                    break;
                case ConfigDomain.Project:
                    if (ProjectPath == null)
                        return null;
                    basePath = ProjectPath;
                    if (String.IsNullOrWhiteSpace(platform))
                        platform = "Default";
                    break;
                case ConfigDomain.ProjectGenerated:
                    if (EnginePath == null)
                        return null;
                    basePath = ProjectPath;
                    prefix = "Generated";
                    break;
                default:
                    return null;
            }
            return Path.Combine(basePath, configDirPath, $"{prefix}{platform}{type}.ini");
        }

        /// <summary>
        /// Enables a platform identifier to use the legacy config model
        /// %Project%/Config/%Platform%/%Platform%%Type%.ini
        /// instead of the modern one
        /// %Project%/Platforms/%Platform%/Config/%Platform%%Type%.ini
        /// </summary>
        public void SetPlatformLegacyConfig(string platformIdentifier, bool isLegacyConfig)
        {
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

        protected string GeneratePlatformConfigDirectory(string platformIdentifier)
        {
            return Path.Combine("Platforms", platformIdentifier, "Config");
        }
        
        protected string GenerateLegacyPlatformConfigDirectory(string platformIdentifier)
        {
            return Path.Combine("Config", platformIdentifier);
        }

        private HashSet<string> m_LegacyPlatforms = new HashSet<string>();
    }
}

