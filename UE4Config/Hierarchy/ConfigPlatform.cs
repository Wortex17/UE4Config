namespace UE4Config.Hierarchy
{
    /// <summary>
    /// Default implementation of an <see cref="IConfigPlatform"/>
    /// </summary>
    public class ConfigPlatform : IConfigPlatform
    {
        public string Identifier { get; }
        public IConfigPlatform ParentPlatform { get; }
        public ConfigPlatform(string identifier, IConfigPlatform parent = null)
        {
            Identifier = identifier;
            ParentPlatform = parent;
        }
    }
}
