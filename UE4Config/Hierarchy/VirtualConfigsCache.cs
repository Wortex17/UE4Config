using System.Collections.Generic;
using UE4Config.Parsing;

namespace UE4Config.Hierarchy
{
    public class VirtualConfigsCache
    {
        public VirtualConfigCache Peek(ConfigFileReference configFileReference)
        {
            if (!m_Cache.ContainsKey(configFileReference))
            {
                m_Cache[configFileReference] = new VirtualConfigCache(configFileReference);
            }

            return m_Cache[configFileReference];
        }

        public ConfigIni GetOrLoadConfig(ConfigFileReference configFileReference, IConfigFileProvider fileProvider)
        {
            var cache = Peek(configFileReference);
            if (cache.ConfigIni == null)
            {
                bool wasLoaded = fileProvider.LoadOrCreateConfig(configFileReference, out var configIni);
                cache.SetConfigIni(configIni, wasLoaded);
            }
            return cache.ConfigIni;
        }
        
        public void InvalidateCache()
        {
            foreach (var cacheEntry in m_Cache)
            {
                cacheEntry.Value.InvalidateCache();
            }
        }

        private Dictionary<ConfigFileReference, VirtualConfigCache> m_Cache = new Dictionary<ConfigFileReference, VirtualConfigCache>();
    }
    
}

