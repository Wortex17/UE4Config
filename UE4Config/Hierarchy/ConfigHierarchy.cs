using System.Collections.Generic;
using UE4Config.Evaluation;
using UE4Config.Parsing;

namespace UE4Config.Hierarchy
{
    /// <remarks>
    /// STUB
    /// </remarks>
    public abstract class ConfigHierarchy
    {
        /// <summary>
        /// Returns the platform config of the given <see cref="category"/> and the given <see cref="level"/>, if available.
        /// Returns null otherwise.
        /// </summary>
        public abstract ConfigIni GetConfig(string platform, string category, ConfigHierarchyLevel level);

        /// <summary>
        /// Returns the default config of the given <see cref="category"/> and the given <see cref="level"/>, if available.
        /// Returns null otherwise.
        /// <seealso cref="GetConfig(string,string,UE4Config.Hierarchy.ConfigHierarchyLevel)"/>
        /// </summary>
        public ConfigIni GetConfig(string category, ConfigHierarchyLevel level)
        {
            return GetConfig("Default", category, level);
        }

        /// <summary>
        /// Gets the configs for the given platform & category in order of ascending levels (<see cref="ConfigHierarchyLevel.Base"/> being the first)
        /// </summary>
        public void GetConfigs(string platform, string category, IList<ConfigIni> configs)
        {
            var levels = ConfigHierarchyLevelExtensions.GetLevelsAscending();
            foreach (var level in levels)
            {
                ConfigIni config = GetConfig(platform, category, level);
                if (config != null)
                {
                    configs.Add(config);
                }
            }
        }

        /// <summary>
        /// Gets the default configs for the category in order of ascending levels (<see cref="ConfigHierarchyLevel.Base"/> being the first)
        /// </summary>
        public void GetConfigs(string category, IList<ConfigIni> configs)
        {
            GetConfigs("Default", category, configs);
        }

        /// <summary>
        /// Evaluates a properties values over this hierarchy of configs.
        /// </summary>
        public void EvaluatePropertyValues(string platform, string category, string sectionName, string propertyKey,
            PropertyEvaluator evaluator, IList<string> values)
        {
            List<ConfigIni> configs = new List<ConfigIni>();
            GetConfigs(platform, category, configs);
            evaluator = PropertyEvaluator.CustomOrDefault(evaluator);
            evaluator.EvaluatePropertyValues(configs, sectionName, propertyKey, values);
        }
    }
}
