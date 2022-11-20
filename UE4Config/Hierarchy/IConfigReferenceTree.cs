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
        IConfigFileProvider FileProvider { get; }
        void Setup(IConfigFileProvider configFileProvider);
        void VisitConfigRoot(Action<ConfigFileReference> onConfig);
        void VisitConfigBranch(string configType, string platformIdentifier, Action<ConfigFileReference> onConfig);
        IConfigPlatform GetPlatform(string platformIdentifier);
        IConfigPlatform RegisterPlatform(string platformIdentifier, IConfigPlatform parentPlatform = null);
    }
    
    public static class IConfigReferenceTreeExtensions
    {
        public static ConfigFileReference? GetConfigRoot(this IConfigReferenceTree configReferenceTree)
        {
            ConfigFileReference? result = null;
            configReferenceTree.VisitConfigRoot(reference => result = reference);
            return result;
        }
        
        public static List<ConfigFileReference> GetConfigBranch(this IConfigReferenceTree configReferenceTree, string configType, string platformIdentifier)
        {
            var result = new List<ConfigFileReference>();
            configReferenceTree.VisitConfigBranch(configType, platformIdentifier, reference => result.Add(reference));
            return result;
        }
    }
}

