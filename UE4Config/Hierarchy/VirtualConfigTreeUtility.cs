using System;
using System.Collections.Generic;
using UE4Config.Parsing;

namespace UE4Config.Hierarchy
{
    public static class VirtualConfigTreeUtility
    {
        /// <summary>
        /// Utility method to quickly create a default virtual config tree instance for common use
        /// </summary>
        public static VirtualConfigTree CreateVirtualConfigTree<TConfigFileProvider, TConfigFileIOAdapter, TConfigReferenceTree>(string enginePath, string projectPath)
            where TConfigFileProvider : IConfigFileProvider, new()
            where TConfigFileIOAdapter : IConfigFileIOAdapter, new()
            where TConfigReferenceTree : IConfigReferenceTree, new()
        {
            //This will provide paths and a virtual hierarchy for a project+engine base path combination
            var configProvider = new TConfigFileProvider();
            configProvider.Setup(new TConfigFileIOAdapter(), enginePath, projectPath);
            DataDrivenPlatformProvider dataDrivenPlatformProvider = new DataDrivenPlatformProvider();
            if(configProvider is IConfigFileProviderAutoPlatformModel configProviderAuto)
            {
                //Auto-detect if a project still uses the legacy Config/{Platform}/*.ini setup.
                configProviderAuto.AutoDetectPlatformsUsingLegacyConfig();
                //Detect DataDrivenPlatforms
                dataDrivenPlatformProvider.Setup(configProviderAuto);
                dataDrivenPlatformProvider.CollectDataDrivenPlatforms();
            }
            //Create the base tree model, based on the config hierarchy used by UE since 4.27+
            var configRefTree = new TConfigReferenceTree();
            configRefTree.Setup(configProvider);
            //Apply any detected DataDrivenPlatforms
            dataDrivenPlatformProvider.RegisterDataDrivenPlatforms(configRefTree);
            //Create a virtual config tree to allow us working with an in-memory virtual hierarchy
            var configTree = new VirtualConfigTree(configRefTree);
            return configTree;
        }
        
        public static VirtualConfigTree CreateVirtualConfigTree(string enginePath, string projectPath)
        {
            return CreateVirtualConfigTree<ConfigFileProvider, ConfigFileIOAdapter, ConfigReferenceTree427>(enginePath, projectPath);
        }
    }
}

