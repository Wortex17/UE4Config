using System.Collections.Generic;

namespace UE4Config.Hierarchy
{
    public interface IConfigFileIOAdapter
    {
        /// <summary>
        /// Returns the paths of all subdirectories at path, excluding path itself.
        /// </summary>
        List<string> GetDirectories(string pivotPath);
    }
}

