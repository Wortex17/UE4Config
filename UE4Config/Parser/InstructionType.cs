namespace UE4Config.Parser
{
    public enum InstructionType
    {
        /// <summary>
        /// Creates or overrides a key with a value
        /// </summary>
        /// <example>
        /// Name=Player
        /// </example>
        Set,
        /// <summary>
        /// Adds another value to a key or sets its initial value
        /// </summary>
        /// <example>
        /// +Maps=SecondLevel.umap
        /// </example>
        Add,
        /// <summary>
        /// Adds another value to a key or sets its initial value
        /// (Honestly don't see the implementation difference between . and + yet; Docs are weird about it. Effectively does not seem to work like described)
        /// </summary>
        /// <example>
        /// Bindings=(Name="Q",Command="Foo")
        /// .Bindings=(Name="Q",Command="Bar")
        /// .Bindings=(Name="Q",Command="Foo")
        /// </example>
        AddOverride,
        /// <summary>
        /// Removes a specific value from a key
        /// </summary>
        /// <example>
        /// -Maps=SecondLevel.umap
        /// </example>
        Remove,
        /// <summary>
        /// Removes all values from a key
        /// </summary>
        /// <example>
        /// !Maps
        /// </example>
        RemoveAll
    }
}
