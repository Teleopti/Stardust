using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.DomainTest.Infrastructure
{
    [TestFixture]
    public class PagingTest
    {
        private Paging _target;

        [SetUp]
        public void Setup()
        {
            _target = new Paging();
        }

        [Test]
        public void ShouldAssignValue()
        {
            _target.TotalCount = 100;
            _target.Take = 10;
            _target.Skip = 50;

            Assert.That(_target.Skip, Is.EqualTo(50));
            Assert.That(_target.Take, Is.EqualTo(10));
            Assert.That(_target.TotalCount, Is.EqualTo(100));
        }

        [Test]
        public void ShouldGetHashCode()
        {
            _target.TotalCount = 100;
            _target.Take = 10;
            _target.Skip = 50;

            Assert.That(_target.GetHashCode(), Is.EqualTo((10*397) ^ 50 ^ 100));
        }

        [Test]
        public void ShouldCheckEquality()
        {
            var paging = new Paging();
            Assert.That(_target.Equals(paging), Is.True);
        }
    }
}
