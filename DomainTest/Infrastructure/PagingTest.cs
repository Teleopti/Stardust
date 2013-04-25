using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

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

		[Test]
		public void ShouldReturnFalseIfEqualsNull()
		{
			Assert.That(_target.Equals(null), Is.False);
		}

		[Test]
		public void ShouldReturnFalseIfCheckingAgainstAnotherType()
		{
			var obj = (object)PersonFactory.CreatePerson("person");
			Assert.That(_target.Equals(obj), Is.False);
		}

		[Test]
		public void ShouldReturnTrueIfCheckingAgainstItself()
		{
			Assert.That(_target.Equals(_target), Is.True);
		}

		[Test]
		public void ShouldReturnTrueIfCheckingAgainstPaging()
		{
			var paging = new Paging {Skip = 10, Take = 100, TotalCount = 200};
			_target.TotalCount = 200;
			_target.Take = 100;
			_target.Skip = 10;

			Assert.That(_target, Is.EqualTo(paging));
		}
    }
}
