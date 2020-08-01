using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UE4Config.Parsing;

namespace UE4Config.Hierarchy
{
    /// <remarks>
    /// Helps load config files in hierarchical chains, emulating the Unreal Engine toolset.
    /// Allows fetching specific configs as well as evaluating a propertys values at any level of the hierarchy.
    /// </remarks>
    public class FileConfigHierarchy : ConfigHierarchy
    {
        /// <summary>
        /// The path to the engine directory (called and including 'Engine')
        /// </summary>
        public string EnginePath
        {
            get;
            protected set;
        }

        /// <summary>
        /// The path to the project root directory (which contains the *.uproject file)
        /// </summary>
        public string ProjectPath
        {
            get;
            protected set;
        }

        public FileConfigHierarchy(string projectPath, string enginePath)
        {
            EnginePath = enginePath;
            ProjectPath = projectPath;
        }

        public override ConfigIni GetConfig(string platform, string category, ConfigHierarchyLevel level)
        {
            ConfigIni config = null;
            if (!IsConfigCached(platform, category, level))
            {
                config = LoadConfig(platform, category, level);
                CacheConfig(platform, category, level, config);
            }
            else
            {
                config = GetCachedConfig(platform, category, level);
            }

            return config;
        }

        /// <summary>
        /// Constructs the file path that would lead to the requested config file
        /// </summary>
        public virtual string GetConfigFilePath(string platform, string category, ConfigHierarchyLevel level)
        {
            switch (level)
            {
                case ConfigHierarchyLevel.Base:
                    return Path.Combine(Environment.CurrentDirectory, EnginePath, ConfigDirName, $"{BaseConfigFilePrefix}.ini");
                case ConfigHierarchyLevel.BaseCategory:
                    return Path.Combine(Environment.CurrentDirectory, EnginePath, ConfigDirName, $"{BaseConfigFilePrefix}{category}.ini");
                case ConfigHierarchyLevel.BasePlatformCategory:
                    if (platform == DefaultPlatform)
                    {
                        return null;
                    }
                    else
                    {
                        return Path.Combine(Environment.CurrentDirectory, EnginePath, ConfigDirName, platform, $"{platform}{category}.ini");
                    }
                case ConfigHierarchyLevel.ProjectCategory:
                    return Path.Combine(Environment.CurrentDirectory, ProjectPath, ConfigDirName, $"{DefaultConfigFilePrefix}{category}.ini");
                case ConfigHierarchyLevel.ProjectPlatformCategory:
                    if (platform == DefaultPlatform)
                    {
                        return null;
                    }
                    else
                    {
                        return Path.Combine(Environment.CurrentDirectory, ProjectPath, ConfigDirName, platform, $"{platform}{category}.ini");
                    }
                default:
                    throw new InvalidEnumArgumentException(nameof(level), (int)level, typeof(ConfigHierarchyLevel));
            }
        }

        /// <summary>
        /// Load config from filesystem
        /// </summary>
        protected virtual ConfigIni LoadConfig(string platform, string category, ConfigHierarchyLevel level)
        {
            var filePath = GetConfigFilePath(platform, category, level);
            if (filePath == null)
                return null;

            StreamReader reader;
            try
            {
                reader = File.OpenText(filePath);
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            var config = new ConfigIni(Path.GetFileName(filePath));
            config.Read(reader);
            reader.Close();
            return config;
        }

        protected virtual void CacheConfig(string platform, string category, ConfigHierarchyLevel level, ConfigIni config)
        {
            var key = new ConfigKey(platform, category, level);
            m_ConfigCache[key] = config;
        }

        protected virtual ConfigIni GetCachedConfig(string platform, string category, ConfigHierarchyLevel level)
        {
            ConfigIni result;
            if (m_ConfigCache.TryGetValue(new ConfigKey(platform, category, level), out result))
            {
                return result;
            }
            return null;
        }

        protected virtual bool IsConfigCached(string platform, string category, ConfigHierarchyLevel level)
        {
            return m_ConfigCache.ContainsKey(new ConfigKey(platform, category, level));
        }

        private struct ConfigKey
        {
            public ConfigKey(string platform, string category, ConfigHierarchyLevel level)
            {
                Platform = platform;
                Category = category;
                Level = level;
            }

            public string Platform;
            public string Category;
            public ConfigHierarchyLevel Level;
        }

        private Dictionary<ConfigKey,ConfigIni> m_ConfigCache = new Dictionary<ConfigKey, ConfigIni>();

        private const string ConfigDirName = "Config";
        private const string BaseConfigFilePrefix = "Base";
        private const string DefaultConfigFilePrefix = "Default";
    }
}
