using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class PairTest
    {
        private Pair<int> target;
        private int first;
        private int second;

        [SetUp]
        public void Setup()
        {
            first = 37;
            second = 12;
            target = new Pair<int>(first, second);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(first, target.First);
            Assert.AreEqual(second, target.Second);
        }

        [Test]
        public void VerifyIEquitable()
        {
            Pair<string> target2 = new Pair<string>("hej", "råddjer");
            Assert.IsFalse(target2.Equals(target));
            Pair<int> target3 = new Pair<int>(-12, 12);
            Assert.IsFalse(target3.Equals(target));
            target3 = new Pair<int>(first, second);
            Assert.IsTrue(target3.Equals(target));
        }
    }
}
