namespace UE4Config.Parsing
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
        /// Adds another value to a key or sets its initial value.
        /// Subsequent adds of the same value will be ineffective
        /// </summary>
        /// <example>
        /// +Maps=SecondLevel.umap
        /// </example>
        Add,
        /// <summary>
        /// Adds another value to a key or sets its initial value.
        /// Will be evaluated as duplicate line, no matter how many times the value has been added before.
        /// </summary>
        /// <example>
        /// This is useful for the bindings (as seen in DefaultInput.ini), for instance,
        /// where the bottom-most binding takes effect:
        /// Bindings=(Name="Q",Command="Foo")
        /// .Bindings=(Name="Q",Command="Bar")
        /// .Bindings=(Name="Q",Command="Foo")
        /// </example>
        AddForce,
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
