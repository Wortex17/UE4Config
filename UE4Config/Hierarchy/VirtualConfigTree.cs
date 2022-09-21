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
        public IConfigTree ReferenceTree { get; private set; }
        public IConfigFileProvider FileProvider => ReferenceTree.FileProvider;
        public VirtualConfigsCache ConfigsCache { get; private set; }

        public VirtualConfigTree(IConfigTree referenceTree)
        {
            ReferenceTree = referenceTree;
            ConfigsCache = new VirtualConfigsCache();
        }

        public void VisitConfigRoot(Action<ConfigIni> onConfig)
        {
            ReferenceTree.VisitConfigRoot(reference =>
            {
                var config = ConfigsCache.GetOrLoadConfig(reference, FileProvider);
                onConfig?.Invoke(config);
            });
        }
        
        public void VisitConfigBranch(string configType, string platformIdentifier, Action<ConfigIni> onConfig)
        {
            ReferenceTree.VisitConfigBranch(configType, platformIdentifier, reference =>
            {
                var config = ConfigsCache.GetOrLoadConfig(reference, FileProvider);
                onConfig?.Invoke(config);
            });
        }

        public ConfigIni FetchConfigRoot()
        {
            ConfigIni result = null;
            VisitConfigRoot(config =>
            {
                result = config;
            });
            return result;
        }
        
        public List<ConfigIni> FetchConfigBranch(string configType, string platformIdentifier)
        {
            List<ConfigIni> result = new List<ConfigIni>();
            VisitConfigBranch(configType, platformIdentifier, config =>
            {
                result.Add(config);
            });
            return result;
        }

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

