using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.DomainTest.Infrastructure
{
    [TestFixture]
    public class PagingDetailTest
    {
        private PagingDetail _target;

        [SetUp]
        public void Setup()
        {
            _target = new PagingDetail();
        }

        [Test]
        public void ShoudHaveDefaultPagingValue()
        {
            Assert.That(_target.Take, Is.EqualTo(20));
        }

        [Test]
        public void ShouldAssignValue()
        {
            _target.TotalNumberOfResults = 100;
            _target.Take = 10;
            _target.Skip = 50;

            Assert.That(_target.Skip, Is.EqualTo(50));
            Assert.That(_target.Take, Is.EqualTo(10));
            Assert.That(_target.TotalNumberOfResults, Is.EqualTo(100));
        }
    }
}
