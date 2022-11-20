using System.Collections.Generic;

namespace UE4Config.Hierarchy
{
    /// <summary>
    /// Platforms are optional additional layers in the hierarchy that only apply to
    /// specific target platforms.
    /// Platforms support additional subhierarchies by inheritance e.g. "PS4" and "PS5" inheriting config from "Sony"
    /// </summary>
    public interface IConfigPlatform
    {
        string Identifier { get; }
        IConfigPlatform ParentPlatform { get; }
    }

    public static class IConfigPlatformExtensions
    {
        public static void ResolvePlatformInheritance(this IConfigPlatform platform, ref List<IConfigPlatform> outPlatforms)
        {
            var platforms = new List<IConfigPlatform>();
            var pivotPlatform = platform;
            while (pivotPlatform != null)
            {
                platforms.Add(pivotPlatform);
                pivotPlatform = pivotPlatform.ParentPlatform;
            };
            platforms.Reverse();
            outPlatforms.AddRange(platforms);
        }
    }
}
