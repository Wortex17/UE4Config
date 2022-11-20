namespace UE4Config.Hierarchy
{
    public interface IConfigFileProviderAutoPlatformModel : IConfigFileProvider
    {
        /// <summary>
        /// Automatically scans the <see cref="IConfigFileProvider.EnginePath"/> and <see cref="IConfigFileProvider.ProjectPath"/>
        /// for all Platforms that require legacy config setup.
        /// Will likely override manual settings made before, but will leave undiscovered platforms intact.
        /// </summary>
        void AutoDetectPlatformsUsingLegacyConfig();
    }
}

