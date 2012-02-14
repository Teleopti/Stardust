using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation.Cache;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Foundation.Cache
{
    [TestFixture]
    public class NoCacheTest
    {
        private ICustomDataCache<int> target;

        [SetUp]
        public void Setup()
        {
            target = new NoCache<int>();
        }

        [Test]
        public void VerifyPutDoesNotDoAnything()
        {
            target.Put("a",2);
            Assert.AreEqual(0, target.Get("a"));
        }

        [Test]
        public void VerifyDeleteDoesNotDoAnything()
        {
            //cant really test - just dont crash
            target.Delete("går ej att testa");
        }

    }
}
