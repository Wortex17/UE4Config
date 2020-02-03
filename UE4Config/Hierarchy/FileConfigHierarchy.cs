using System;
using System.ComponentModel;
using System.IO;
using UE4Config.Parsing;

namespace UE4Config.Hierarchy
{
    /// <remarks>
    /// STUB
    /// </remarks>
    public class FileConfigHierarchy : ConfigHierarchy
    {
        /// <summary>
        /// The path to the engine directory (called 'Engine')
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
            throw new System.NotImplementedException();
        }

        public string GetConfigFilePath(ConfigHierarchyLevel level, string platform, string category)
        {
            switch (level)
            {
                case ConfigHierarchyLevel.Base:
                    return Path.Combine(Environment.CurrentDirectory, EnginePath, "Config/Base.ini");
                case ConfigHierarchyLevel.BaseCategory:
                    return Path.Combine(Environment.CurrentDirectory, EnginePath, $"Config/Base{category}.ini");
                case ConfigHierarchyLevel.BasePlatformCategory:
                    return Path.Combine(Environment.CurrentDirectory, EnginePath, $"Config/{platform}/{platform}{category}.ini");
                case ConfigHierarchyLevel.ProjectCategory:
                    return Path.Combine(Environment.CurrentDirectory, ProjectPath, $"Config/Default{category}.ini");
                case ConfigHierarchyLevel.ProjectPlatformCategory:
                    return Path.Combine(Environment.CurrentDirectory, ProjectPath, $"Config/{platform}/{platform}{category}.ini");
                default:
                    throw new InvalidEnumArgumentException(nameof(level), (int)level, typeof(ConfigHierarchyLevel));
            }
        }
    }
}
