using NUnit.Framework;

namespace UE4Config.Tests
{
    [TestFixture]
    class SanityTests
    {
        [Test]
        public void When_CheckingTestSanity()
        {
            Assert.That("Sanity", Is.EqualTo("Sanity"));
            Assert.That(true, Is.True);
            Assert.That(false, Is.False);

            Assert.That(System.Convert.ToInt32(false), Is.EqualTo(0));
            Assert.That(System.Convert.ToInt32(true), Is.EqualTo(1));
            Assert.That(System.Convert.ToBoolean(0), Is.False);
            Assert.That(System.Convert.ToBoolean(1), Is.True);
            Assert.That(!!false, Is.False);
            Assert.That(!!true, Is.True);
        }
    }
}
