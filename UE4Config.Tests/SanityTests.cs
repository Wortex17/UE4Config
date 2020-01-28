using System;
using System.IO;
using NUnit.Framework;

namespace UE4Config.Tests
{
    [TestFixture]
    class SanityTests
    {
        public void When_CheckingTestSanity()
        {
            Assert.That("Sanity", Is.EqualTo("Sanity"));
            Assert.That(true, Is.True);
            Assert.That(false, Is.False);
        }
    }
}
