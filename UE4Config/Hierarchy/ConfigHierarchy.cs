﻿using System.Collections.Generic;
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
        /// The default platform identifier - used when no specific platform is targeted
        /// </summary>
        public const string DefaultPlatform = "Default";

        /// <summary>
        /// Returns if there is a "platform extension" folder structure for the given platform in the engine directory.
        /// This structure is being introduced since 4.24
        /// If true, will use different paths for engine platform config files.
        /// </summary>
        public abstract bool CheckEngineHasPlatformExtension(string platform);

        /// <summary>
        /// Returns if there is a "platform extension" folder structure for the given platform in the engine directory.
        /// This structure is being introduced since 4.24
        /// If true, will use different paths for project platform config files.
        /// </summary>
        public abstract bool CheckProjectHasPlatformExtension(string platform);

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
            return GetConfig(DefaultPlatform, category, level);
        }

        /// <summary>
        /// Gets the configs for the given platform & category in order of ascending levels (<see cref="ConfigHierarchyLevel.Base"/> being the first)
        /// </summary>
        public void GetConfigs(string platform, string category, ConfigHierarchyLevelRange range, IList<ConfigIni> configs)
        {
            var levels = ConfigHierarchyLevelExtensions.GetLevelsAscending();
            foreach (var level in levels)
            {
                if (range.Includes(level))
                {
                    ConfigIni config = GetConfig(platform, category, level);
                    if (config != null)
                    {
                        configs.Add(config);
                    }
                }
            }
        }

        /// <inheritdoc cref="GetConfigs(string,string,UE4Config.Hierarchy.ConfigHierarchyLevelRange,System.Collections.Generic.IList{UE4Config.Parsing.ConfigIni})"/>
        public void GetConfigs(string platform, string category, IList<ConfigIni> configs)
        {
            GetConfigs(platform, category, ConfigHierarchyLevelRange.All(), configs);
        }

        /// <summary>
        /// Gets the default configs for the category in order of ascending levels (<see cref="ConfigHierarchyLevel.Base"/> being the first)
        /// </summary>
        public void GetConfigs(string category, ConfigHierarchyLevelRange range, IList<ConfigIni> configs)
        {
            GetConfigs(DefaultPlatform, category, range, configs);
        }

        public void GetConfigs(string category, IList<ConfigIni> configs)
        {
            GetConfigs(category, ConfigHierarchyLevelRange.All(), configs);
        }

        /// <summary>
        /// Evaluates a properties values over this hierarchy of configs.
        /// </summary>
        public virtual void EvaluatePropertyValues(string platform, string category, string sectionName, string propertyKey, ConfigHierarchyLevelRange range,
            PropertyEvaluator evaluator, IList<string> values)
        {
            List<ConfigIni> configs = new List<ConfigIni>();
            GetConfigs(platform, category, range, configs);
            evaluator = PropertyEvaluator.CustomOrDefault(evaluator);
            evaluator.EvaluatePropertyValues(configs, sectionName, propertyKey, values);
        }

        /// <inheritdoc cref="EvaluatePropertyValues(string,string,string,string,ConfigHierarchyLevelRange,UE4Config.Evaluation.PropertyEvaluator,System.Collections.Generic.IList{string})"/>
        public void EvaluatePropertyValues(string platform, string category, string sectionName, string propertyKey, PropertyEvaluator evaluator, IList<string> values)
        {
            EvaluatePropertyValues(platform, category, sectionName, propertyKey, ConfigHierarchyLevelRange.All(),
                evaluator, values);
        }

        /// <inheritdoc cref="EvaluatePropertyValues(string,string,string,string,ConfigHierarchyLevelRange,UE4Config.Evaluation.PropertyEvaluator,System.Collections.Generic.IList{string})"/>
        /// <remarks>
        /// Uses <see cref="PropertyEvaluator.Default"/> as evaluator
        /// </remarks>
        public void EvaluatePropertyValues(string platform, string category, string sectionName, string propertyKey, ConfigHierarchyLevelRange range, IList<string> values)
        {
            EvaluatePropertyValues(platform, category, sectionName, propertyKey, range, PropertyEvaluator.Default, values);
        }

        /// <inheritdoc cref="EvaluatePropertyValues(string,string,string,string,UE4Config.Evaluation.PropertyEvaluator,System.Collections.Generic.IList{string})"/>
        /// <remarks>
        /// Uses <see cref="PropertyEvaluator.Default"/> as evaluator
        /// </remarks>
        public void EvaluatePropertyValues(string platform, string category, string sectionName, string propertyKey, IList<string> values)
        {
            EvaluatePropertyValues(platform, category, sectionName, propertyKey, ConfigHierarchyLevelRange.All(), values);
        }

        /// <inheritdoc cref="EvaluatePropertyValues(string,string,string,string,ConfigHierarchyLevelRange,UE4Config.Evaluation.PropertyEvaluator,System.Collections.Generic.IList{string})"/>
        /// <remarks>
        /// Uses "Default" platform
        /// Uses <see cref="PropertyEvaluator.Default"/> as evaluator
        /// </remarks>
        public void EvaluatePropertyValues(string category, string sectionName, string propertyKey, ConfigHierarchyLevelRange range, IList<string> values)
        {
            EvaluatePropertyValues(DefaultPlatform, category, sectionName, propertyKey, range, PropertyEvaluator.Default, values);
        }

        /// <inheritdoc cref="EvaluatePropertyValues(string,string,string,string,UE4Config.Evaluation.PropertyEvaluator,System.Collections.Generic.IList{string})"/>
        /// <remarks>
        /// Uses "Default" platform
        /// Uses <see cref="PropertyEvaluator.Default"/> as evaluator
        /// </remarks>
        public void EvaluatePropertyValues(string category, string sectionName, string propertyKey, IList<string> values)
        {
            EvaluatePropertyValues(category, sectionName, propertyKey, ConfigHierarchyLevelRange.All(), values);
        }

        /// <inheritdoc cref="EvaluatePropertyValues(string,string,string,string,ConfigHierarchyLevelRange,UE4Config.Evaluation.PropertyEvaluator,System.Collections.Generic.IList{string})"/>
        /// <remarks>
        /// Uses "Default" platform
        /// </remarks>
        public void EvaluatePropertyValues(string category, string sectionName, string propertyKey, ConfigHierarchyLevelRange range, PropertyEvaluator evaluator, IList<string> values)
        {
            EvaluatePropertyValues(DefaultPlatform, category, sectionName, propertyKey, range, evaluator, values);
        }

        /// <inheritdoc cref="EvaluatePropertyValues(string,string,string,string,UE4Config.Evaluation.PropertyEvaluator,System.Collections.Generic.IList{string})"/>
        /// <remarks>
        /// Uses "Default" platform
        /// </remarks>
        public void EvaluatePropertyValues(string category, string sectionName, string propertyKey, PropertyEvaluator evaluator, IList<string> values)
        {
            EvaluatePropertyValues(category, sectionName, propertyKey, ConfigHierarchyLevelRange.All(), evaluator, values);
        }
    }
}
