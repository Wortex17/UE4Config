using System;

namespace UE4Config.Hierarchy
{
    /// <summary>
    /// Represents a single config file that may or may not exist yet.
    /// Needs to be interpreted and provided by a ConfigFileProvider.
    /// </summary>
    public struct ConfigFileReference
    {
        public static ConfigFileReference None => new ConfigFileReference(); 
        public ConfigDomain Domain { get; private set; }
        public IConfigPlatform Platform { get; private set; }
        public string Type { get; private set; }

        public bool IsPlatformConfig => Platform != null;

        public ConfigFileReference(ConfigDomain domain, IConfigPlatform platform, string type)
        {
            if (type != null && string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException(nameof(type), "Argument cannot be an empty or whitespace string");
            } else if (type?.ToLowerInvariant() == "default")
            {
                throw new ArgumentException(nameof(type), "Argument cannot be \"Default\"");
            } else if (type?.ToLowerInvariant() == "base")
            {
                throw new ArgumentException(nameof(type), "Argument cannot be \"Base\"");
            }
            Domain = domain;
            Platform = platform;
            Type = type;
        }

        public override string ToString()
        {
            var result = nameof(ConfigFileReference);
            result += ":"+Domain;
            if (Platform != null)
            {
                result += "@" + Platform.Identifier;
            }

            if (Type != null)
            {
                result += ":" + Type;
            }
            
            return result;
        }
    }
}

