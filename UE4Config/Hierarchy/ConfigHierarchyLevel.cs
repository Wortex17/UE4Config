using System;

namespace UE4Config.Hierarchy
{
    /// <summary>
    /// Represents a level of the Unreal Engine 4 configuration hierarchy
    /// </summary>
    public enum ConfigHierarchyLevel
    {
        /// <example>
        /// Engine/Config/Base.ini 
        /// </example>
        Base,
        /// <example>
        /// Engine/Config/Base[Category].ini 
        /// Engine/Config/BaseEngine.ini 
        /// </example>
        BaseCategory,
        /// <example>
        /// Engine/Config/[Platform]/[Platform][Category].ini
        /// Engine/Config/PS4/PS4Engine.ini
        /// </example>
        BasePlatformCategory,
        /// <example>
        /// [ProjectDirectory]/Config/Default[Category].ini
        /// [ProjectDirectory]/Config/DefaultEngine.ini
        /// </example>
        ProjectCategory,
        /// <example>
        /// [ProjectDirectory]/Config/[Platform]/[Platform][Category].ini 
        /// [ProjectDirectory]/Config/PS4/PS4Engine.ini 
        /// </example>
        ProjectPlatformCategory
    }

    public static class ConfigHierarchyLevelExtensions
    {
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

        private static ConfigHierarchyLevel[] m_LevelsAscending = null;
    }
}
