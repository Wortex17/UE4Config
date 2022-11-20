using System;
using System.Collections.Generic;
using UE4Config.Parsing;

namespace UE4Config.Hierarchy
{
    /// <summary>
    /// The virtual configuration tree provides a bundled solution to manage in-memory version of the reference config tree.
    /// If effectively caches configs of the underlying tree and allows you to create, load, reload and publish them.
    /// </summary>
    public class VirtualConfigTree
    {
        public IConfigReferenceTree ReferenceTree { get; private set; }
        public IConfigFileProvider FileProvider => ReferenceTree.FileProvider;
        public VirtualConfigsCache ConfigsCache { get; private set; }

        public VirtualConfigTree(IConfigReferenceTree referenceTree)
        {
            ReferenceTree = referenceTree;
            ConfigsCache = new VirtualConfigsCache();
        }
        
        /// <summary>
        /// Calls <paramref name="onConfig"/> passing the root config as referenced by <see cref="ReferenceTree"/>.
        /// Either passes the cached instance or creates a new instance, loading contents from disk if possible.
        /// </summary>
        public void VisitConfigRoot(Action<ConfigIni> onConfig)
        {
            ReferenceTree.VisitConfigRoot(reference =>
            {
                var config = ConfigsCache.GetOrLoadConfig(reference, FileProvider);
                onConfig?.Invoke(config);
            });
        }
        
        /// <summary>
        /// Calls <paramref name="onConfig"/> passing every config on a branch referenced by <see cref="ReferenceTree"/>.
        /// Either passes the cached instances or creates new instances, loading contents from disk if possible.
        /// </summary>
        public void VisitConfigBranch(string configType, string platformIdentifier, Action<ConfigIni> onConfig)
        {
            ReferenceTree.VisitConfigBranch(configType, platformIdentifier, reference =>
            {
                var config = ConfigsCache.GetOrLoadConfig(reference, FileProvider);
                onConfig?.Invoke(config);
            });
        }

        /// <summary>
        /// Utility method wrapping <see cref="VisitConfigRoot"/>.
        /// Returns the root config as referenced by <see cref="ReferenceTree"/>.
        /// Either returns the cached instance or creates a new instance, loading contents from disk if possible.
        /// </summary>
        public ConfigIni FetchConfigRoot()
        {
            ConfigIni result = null;
            VisitConfigRoot(config =>
            {
                result = config;
            });
            return result;
        }
        
        /// <summary>
        /// Utility method wrapping <see cref="VisitConfigBranch"/>.
        /// Returns every config on a branch referenced by <see cref="ReferenceTree"/>.
        /// Either returns the cached instances or creates new instances, loading contents from disk if possible.
        /// </summary>
        public List<ConfigIni> FetchConfigBranch(string configType, string platformIdentifier)
        {
            List<ConfigIni> result = new List<ConfigIni>();
            VisitConfigBranch(configType, platformIdentifier, config =>
            {
                result.Add(config);
            });
            return result;
        }

        /// <summary>
        /// Caches the given config instance, possibly overwriting a currently cached instance, before also
        /// saving it to disk. Use the <see cref="ConfigIni.Reference"/> of the given <paramref name="configIni"/> as
        /// cache identifier.
        /// </summary>
        public void PublishConfig(ConfigIni configIni)
        {
            if (configIni == null)
                throw new ArgumentNullException(nameof(configIni));
            var configReference = configIni.Reference;
            var cache = ConfigsCache.Peek(configReference);
            FileProvider.SaveConfig(configReference, configIni);
            cache.SetConfigIni(configIni, true);
        }
    }
}

