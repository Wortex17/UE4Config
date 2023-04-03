using System;
using System.Collections.Generic;

namespace UE4Config.Hierarchy
{
    /// <summary>
    /// A configuration tree provides a common interface to generate chains of configuration files ("branches")
    /// for any type/platform combination.
    /// </summary>
    /// <remarks>
    /// This replaces the ConfigHierarchy used in older versions of this codebase.
    /// This does not support layer-hierarchies below UE427+
    /// This does not yet support user-layers
    /// </remarks>
    public interface IConfigReferenceTree
    {
        /**
         * The <see cref="IConfigFileProvider"/> provided during <see cref="Setup"/>
         */
        IConfigFileProvider FileProvider { get; }
        /**
         * Provides dependencies and initialized the reference tree to return correct root and branches.
         */
        void Setup(IConfigFileProvider configFileProvider);
        /**
         * Calls <see cref="onConfig"/> for the root of the reference tree.
         */
        void VisitConfigRoot(Action<ConfigFileReference> onConfig);
        /**
         * Calls <see cref="onConfig"/> for every config in designated branch of the reference tree,
         * including and starting with the root.
         */
        void VisitConfigBranch(string configType, string platformIdentifier, Action<ConfigFileReference> onConfig);
        IConfigPlatform GetPlatform(string platformIdentifier);
        IConfigPlatform RegisterPlatform(string platformIdentifier, IConfigPlatform parentPlatform = null);
    }
    
    /**
     * Static utility methods to work with <see cref="IConfigReferenceTree"/>s
     */
    public static class IConfigReferenceTreeExtensions
    {
        /**
         * Returns a file reference to the supposed config file serving as the root of the tree
         */
        public static ConfigFileReference? GetConfigRoot(this IConfigReferenceTree configReferenceTree)
        {
            ConfigFileReference? result = null;
            configReferenceTree.VisitConfigRoot(reference => result = reference);
            return result;
        }
        
        /**
         * Returns a list of file references to the supposed config files of the designated branch of the tree,
         * including and starting with the root.
         */
        public static List<ConfigFileReference> GetConfigBranch(this IConfigReferenceTree configReferenceTree, string configType, string platformIdentifier)
        {
            var result = new List<ConfigFileReference>();
            configReferenceTree.VisitConfigBranch(configType, platformIdentifier, reference => result.Add(reference));
            return result;
        }
    }
}

