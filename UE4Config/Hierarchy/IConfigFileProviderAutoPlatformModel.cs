using System.Collections.Generic;

namespace UE4Config.Hierarchy
{
    public interface IConfigFileProviderAutoPlatformModel : IConfigFileProvider
    {
        /// <summary>
        /// Finds and returns all platform identifiers that seem to have a config directory in either engine or project
        /// </summary>
        List<string> FindAllPlatforms();
        
        /// <summary>
        /// Automatically scans the <see cref="IConfigFileProvider.EnginePath"/> and <see cref="IConfigFileProvider.ProjectPath"/>
        /// for all Platforms that require legacy config setup.
        /// Will likely override manual settings made before, but will leave undiscovered platforms intact.
        /// </summary>
        void AutoDetectPlatformsUsingLegacyConfig();
    }
}

