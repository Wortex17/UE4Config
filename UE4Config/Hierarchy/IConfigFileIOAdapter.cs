using System.Collections.Generic;
using System.IO;

namespace UE4Config.Hierarchy
{
    public interface IConfigFileIOAdapter
    {
        /// <summary>
        /// Returns the paths of all subdirectories at path, excluding path itself.
        /// </summary>
        List<string> GetDirectories(string pivotPath);

        StreamReader OpenText(string filePath);
        
        StreamWriter OpenWrite(string filePath);
    }
}

