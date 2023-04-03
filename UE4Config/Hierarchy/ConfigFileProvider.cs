using System;
using System.Collections.Generic;
using System.IO;
using UE4Config.Parsing;

namespace UE4Config.Hierarchy
{
    /// <summary>
    /// Translates ConfigFileReferences to file paths as used in UE4.27+ and above.
    /// Supports legacy platform config file paths, but assumes modern platform setups as default.
    /// </summary>
    public class ConfigFileProvider : IConfigFileProviderAutoPlatformModel
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
                if (String.IsNullOrEmpty(platformIdentifier))
                {
                    return;
                }

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
                case ConfigDomain.None:
                    return null; //Not yet supported
                case ConfigDomain.EngineBase:
                    if (EnginePath == null)
                    {
                        return null;
                    }
                    if (!String.IsNullOrEmpty(reference.Platform?.Identifier) && String.IsNullOrEmpty(reference.Type))
                    {
                        return null;
                    }
                    basePath = EnginePath;
                    prefix = "Base";
                    platformLegacyConfig = EnginePlatformLegacyConfig;
                    break;
                case ConfigDomain.Engine:
                    if (EnginePath == null)
                    {
                        return null;
                    }
                    if (String.IsNullOrEmpty(reference.Type))
                    {
                        return null;
                    }
                    basePath = EnginePath;
                    platformLegacyConfig = EnginePlatformLegacyConfig;
                    break;
                case ConfigDomain.Project:
                    if (ProjectPath == null)
                    {
                        return null;
                    }
                    if (String.IsNullOrEmpty(reference.Type))
                    {
                        return null;
                    }
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
                    {
                        return null;
                    }
                    if (String.IsNullOrEmpty(reference.Type))
                    {
                        return null;
                    }
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

            if (type == "DataDrivenPlatformInfo")
            {
                platform = "";
            }
            
            return Path.Combine(basePath, configDirPath, $"{prefix}{platform}{type}.ini");
        }

        public bool LoadOrCreateConfig(ConfigFileReference configFileReference, out ConfigIni configIni)
        {
            string configFilePath = ResolveConfigFilePath(configFileReference);
            string configFileName = Path.GetFileName(configFilePath);

            configIni = LoadConfig(configFilePath, configFileName, configFileReference);
            if (configIni == null)
            {
                configIni = new ConfigIni(configFileName, configFileReference);
                return false;
            }
            return true;
        }

        public bool LoadOrCreateDataDrivenPlatformConfig(string platformIdentifier, out ConfigIni configIni)
        {
            ConfigFileReference configFileReference = new ConfigFileReference(ConfigDomain.Engine,
                new ConfigPlatform(platformIdentifier), "DataDrivenPlatformInfo");
            string configFilePath = ResolveConfigFilePath(configFileReference);
            string configFileName = Path.GetFileName(configFilePath);
            
            configIni = LoadConfig(configFilePath, configFileName, configFileReference);
            if (configIni == null)
            {
                configIni = new ConfigIni(configFileName, configFileReference);
                return false;
            }
            return true;
        }

        protected ConfigIni LoadConfig(string configFilePath, string configFileName, ConfigFileReference configFileReference)
        {
            StreamReader reader = OpenReaderIfAvailable(configFilePath);
            if (reader == null)
                return null;
            
            var config = new ConfigIni(configFileName, configFileReference);
            config.Read(reader);
            reader.Close();
            
            return config;
        }

        protected StreamReader OpenReaderIfAvailable(string filePath)
        {
            StreamReader reader;
            try
            {
                reader = FileIOAdapter.OpenRead(filePath);
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
            catch (FileNotFoundException)
            {
                return null;
            }

            return reader;
        }

        public void SaveConfig(ConfigFileReference configFileReference, ConfigIni configIni)
        {
            string configFilePath = ResolveConfigFilePath(configFileReference);

            var previewWriter = new StringWriter();
            var writer = new ConfigIniWriter(previewWriter);
            configIni.Write(writer);
            writer.ContentWriter.Close();

            var targetContent = WriteConfigToString(configIni);
            var shouldWrite = false;
            
            StreamReader presentConfigReader = OpenReaderIfAvailable(configFilePath);
            if (presentConfigReader == null)
            {
                shouldWrite = !String.IsNullOrWhiteSpace(targetContent);
            }
            else
            {
                var presentContent = presentConfigReader.ReadToEnd();
                if (String.CompareOrdinal(targetContent, presentContent) != 0)
                {
                    shouldWrite = true;
                }
                presentConfigReader.Close();
            }
            
            if (shouldWrite)
            {
                var contentWriter = FileIOAdapter.OpenWrite(configFilePath);
                contentWriter.Write(targetContent);
                contentWriter.Close();
            }
        }

        protected string WriteConfigToString(ConfigIni configIni)
        {
            var stringWriter = new StringWriter();
            var writer = new ConfigIniWriter(stringWriter);
            configIni.Write(writer);
            writer.ContentWriter.Close();
            return stringWriter.ToString();
        }

        public List<string> FindAllPlatforms()
        {
            var platforms = new List<string>();
            IterateAllPlatforms(EnginePath, (platform)  =>
            {
                if (!platforms.Contains(platform))
                {
                    platforms.Add(platform);
                }
            }, (platform)  =>
            {
                if (!platforms.Contains(platform))
                {
                    platforms.Add(platform);
                }
            });
            return platforms;
        }

        public void AutoDetectPlatformsUsingLegacyConfig()
        {
            AutoDetectPlatformsUsingLegacyConfig(EnginePath, EnginePlatformLegacyConfig);
            AutoDetectPlatformsUsingLegacyConfig(ProjectPath, ProjectPlatformLegacyConfig);
        }

        protected void IterateAllPlatforms(string basePath, Action<string> onLegacyPlatform, Action<string> onModernPlatform)
        {
            var legacyPlatformDirs = FileIOAdapter.GetDirectories(Path.Combine(basePath, "Config"));
            var modernPlatformDirs = FileIOAdapter.GetDirectories(Path.Combine(basePath, "Platforms"));

            foreach (var legacyPlatformDir in legacyPlatformDirs)
            {
                onLegacyPlatform(Path.GetFileName(legacyPlatformDir));
            }
            foreach (var modernPlatformDir in modernPlatformDirs)
            {
                onModernPlatform(Path.GetFileName(modernPlatformDir));
            }
        }

        protected void AutoDetectPlatformsUsingLegacyConfig(string basePath, PlatformLegacyConfig platformLegacyConfig)
        {
            if (FileIOAdapter != null && !String.IsNullOrEmpty(basePath))
            {
                var legacyPlatforms = new List<string>();
                var modernPlatforms = new List<string>();
                IterateAllPlatforms(basePath, legacyPlatforms.Add, modernPlatforms.Add);
                
                foreach (var legacyPlatform in legacyPlatforms)
                {
                    if (!modernPlatforms.Contains(legacyPlatform))
                    {
                        platformLegacyConfig.SetPlatformLegacyConfig(legacyPlatform, true);
                    }
                    else
                    {
                        platformLegacyConfig.SetPlatformLegacyConfig(legacyPlatform, false);
                    }
                }
                foreach (var modernPlatform in modernPlatforms)
                {
                    platformLegacyConfig.SetPlatformLegacyConfig(modernPlatform, false);
                }
            }
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

