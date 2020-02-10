using System.IO;
using NUnit.Framework;

namespace UE4Config.Tests
{
    public class TestUtils
    {
        public static string GetTestDataPath()
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
        }

        public static string GetTestDataPath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", path);
        }
    }
}
