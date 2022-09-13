namespace UE4Config.Hierarchy
{
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
