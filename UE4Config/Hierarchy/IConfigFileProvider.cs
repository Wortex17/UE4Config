using System.Collections.Generic;
using UE4Config.Parsing;

namespace UE4Config.Hierarchy
{
    public interface IConfigFileProvider
    {
        /// <summary>
        /// The ConfigFileIOAdapter used to retrieve information and contents of the file storage
        /// </summary>
        IConfigFileIOAdapter FileIOAdapter { get; }
        
        /// <summary>
        /// Base path to the engine directory (containing e.g. the /Engine and /Source subdirectory).
        /// Can be null to ignore.
        /// </summary>
        string EnginePath { get; }
        
        /// <summary>
        /// Base path to the project directory (containing the *.uproject file).
        /// Can be null to ignore.
        /// </summary>
        string ProjectPath { get; }
        
        bool IsSetup { get; }

        /// <summary>
        /// Sets up the provider with base paths.
        /// </summary>
        /// <param name="fileIOAdapter">
        /// The ConfigFileIOAdapter used to retrieve information and contents of the file storage
        /// </param>
        /// <param name="enginePath">
        /// Base path to the engine directory (containing e.g. the /Engine and /Source subdirectory).
        /// Can be null to ignore.
        /// </param>
        /// <param name="projectPath">
        /// Base path to the project directory (containing the *.uproject file).
        /// Can be null to ignore.
        /// </param>
        void Setup(IConfigFileIOAdapter fileIOAdapter, string enginePath, string projectPath);
        
        /// <summary>
        /// Returns a string filepath to a config file pointed at by the reference.
        /// Returns null if the reference cannot be resolved.
        /// </summary>
        string ResolveConfigFilePath(ConfigFileReference reference);

        /// <summary>
        /// Loads a config from the filesystem, if it exists or creates a new one.
        /// Returns true if the config was loaded.
        /// </summary>
        bool LoadOrCreateConfig(ConfigFileReference configFileReference, out ConfigIni configIni);

        /// <summary>
        /// Loads multiple configs from the filesystem, if they exist, or creates a new one for the missing.
        /// </summary>
        IList<ConfigIni> LoadOrCreateConfigs(IList<ConfigFileReference> configBranch);

        /// <summary>
        /// Saves a config to the filesystem, overwriting a possibly existing one.
        /// Will not save if there were no meaningful changes.
        /// </summary>
        void SaveConfig(ConfigFileReference configFileReference, ConfigIni configIni);
    }
}

