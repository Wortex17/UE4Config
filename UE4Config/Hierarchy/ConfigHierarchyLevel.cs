using System;

namespace UE4Config.Hierarchy
{
    /// <summary>
    /// Represents a level of the Unreal Engine 4 configuration hierarchy
    /// </summary>
    [Obsolete]
    public enum ConfigHierarchyLevel
    {
        /// <example>
        /// Engine/Config/Base.ini
        /// </example>
        Base,
        /// <example>
        /// Engine/Config/Base[Category].ini
        /// </example>
        /// <example>
        /// Engine/Config/BaseEngine.ini
        /// </example>
        BaseCategory,
        /// <example>
        /// Engine/Config/[Platform]/[Platform][Category].ini
        /// </example>
        /// <example>
        /// Engine/Config/PS4/PS4Engine.ini
        /// </example>
        BasePlatformCategory,
        /// <example>
        /// [ProjectDirectory]/Config/Default[Category].ini
        /// </example>
        /// <example>
        /// [ProjectDirectory]/Config/DefaultEngine.ini
        /// </example>
        ProjectCategory,
        /// <example>
        /// [ProjectDirectory]/Config/[Platform]/[Platform][Category].ini
        /// </example>
        /// <example>
        /// [ProjectDirectory]/Config/PS4/PS4Engine.ini
        /// </example>
        ProjectPlatformCategory
    }

    public static class ConfigHierarchyLevelExtensions
    {
        /// <summary>
        /// Returns a <see cref="ConfigHierarchyLevelRange"/> from this level to a specific other level
        /// <see cref="ConfigHierarchyLevelRange.FromTo"/>
        /// </summary>
        public static ConfigHierarchyLevelRange To(this ConfigHierarchyLevel from, ConfigHierarchyLevel to)
        {
            return ConfigHierarchyLevelRange.FromTo(from, to);
        }

        /// <summary>
        /// Returns a <see cref="ConfigHierarchyLevelRange"/> from this level to any lower
        /// <see cref="ConfigHierarchyLevelRange.AnyTo"/>
        /// </summary>
        public static ConfigHierarchyLevelRange AndLower(this ConfigHierarchyLevel level)
        {
            return ConfigHierarchyLevelRange.AnyTo(level);
        }

        /// <summary>
        /// Returns a <see cref="ConfigHierarchyLevelRange"/> from this level to any higher
        /// <see cref="ConfigHierarchyLevelRange.AnyFrom"/>
        /// </summary>
        public static ConfigHierarchyLevelRange AndHigher(this ConfigHierarchyLevel level)
        {
            return ConfigHierarchyLevelRange.AnyFrom(level);
        }

        /// <summary>
        /// Returns a <see cref="ConfigHierarchyLevelRange"/> exactly representing this level.
        /// <see cref="ConfigHierarchyLevelRange.Exact"/>
        /// </summary>
        public static ConfigHierarchyLevelRange Exact(this ConfigHierarchyLevel level)
        {
            return ConfigHierarchyLevelRange.Exact(level);
        }

        /// <summary>
        /// Returns the levels in ascending order (<see cref="ConfigHierarchyLevel.Base"/> being the first)
        /// </summary>
        public static ConfigHierarchyLevel[] GetLevelsAscending()
        {
            if (m_LevelsAscending == null)
            {
                m_LevelsAscending = (ConfigHierarchyLevel[])Enum.GetValues(typeof(ConfigHierarchyLevel));
            }

            return m_LevelsAscending;
        }

        private static ConfigHierarchyLevel[] m_LevelsAscending;
    }
}
