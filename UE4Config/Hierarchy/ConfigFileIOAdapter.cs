using System.Collections.Generic;
using System.IO;

namespace UE4Config.Hierarchy
{
    /// <summary>
    /// Default implementation of the <see cref="IConfigFileIOAdapter"/>
    /// </summary>
    public class ConfigFileIOAdapter : IConfigFileIOAdapter
    {
        public List<string> GetDirectories(string pivotPath)
        {
            return new List<string>(Directory.GetDirectories(pivotPath));
        }
    }
}

