namespace UE4Config.Hierarchy
{
    /// <summary>
    /// Represents a range of <see cref="ConfigHierarchyLevel"/>s
    /// </summary>
    public struct ConfigHierarchyLevelRange
    {

        /// <summary>
        /// True if <see cref="From"/> represents a bound to be respected. False is there is no lower bound.
        /// </summary>
        public bool HasFrom => m_HasFrom;

        /// <summary>
        /// The lowest <see cref="ConfigHierarchyLevel"/> still included in the range
        /// </summary>
        public ConfigHierarchyLevel From { get; private set; }

        /// <summary>
        /// The topmost <see cref="ConfigHierarchyLevel"/> still included in the range
        /// </summary>
        public ConfigHierarchyLevel To { get; private set; }

        /// <summary>
        /// True if <see cref="To"/> represents a bound to be respected. False is there is no upper bound.
        /// </summary>
        public bool HasTo => m_HasTo;

        /// <summary>
        /// True if this range includes any level at all. If false, no level will match this range.
        /// </summary>
        public bool IncludesAnything => m_IncludesAnything;

        public ConfigHierarchyLevelRange(ConfigHierarchyLevel from, ConfigHierarchyLevel to)
        {
            m_IncludesAnything = true;
            m_HasFrom = true;
            m_HasTo = true;
            if (from <= to)
            {
                From = from;
                To = to;
            }
            else
            {
                To = from;
                From = to;
            }
        }

        public bool Includes(ConfigHierarchyLevel level)
        {
            if (!IncludesAnything)
                return false;
            if (HasFrom && level < From)
                return false;
            if (HasTo && level > To)
                return false;
            return true;
        }

        public static ConfigHierarchyLevelRange All()
        {
            var range = new ConfigHierarchyLevelRange();
            range.m_IncludesAnything = true;
            return range;
        }

        public static ConfigHierarchyLevelRange None()
        {
            return new ConfigHierarchyLevelRange();
        }

        public static ConfigHierarchyLevelRange AnyFrom(ConfigHierarchyLevel from)
        {
            var range = new ConfigHierarchyLevelRange(from, from);
            range.m_HasTo = false;
            return range;
        }

        public static ConfigHierarchyLevelRange AnyTo(ConfigHierarchyLevel to)
        {
            var range = new ConfigHierarchyLevelRange(to, to);
            range.m_HasFrom = false;
            return range;
        }

        public static ConfigHierarchyLevelRange FromTo(ConfigHierarchyLevel from, ConfigHierarchyLevel to)
        {
            return new ConfigHierarchyLevelRange(from, to);
        }

        public static ConfigHierarchyLevelRange Exact(ConfigHierarchyLevel level)
        {
            return new ConfigHierarchyLevelRange(level, level);
        }

        private bool m_IncludesAnything;
        private bool m_HasFrom;
        private bool m_HasTo;
    }
}
