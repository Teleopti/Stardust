using NUnit.Framework;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Auditing
{
    [TestFixture]
    public class RevisionTest
    {
        private Revision _target;

        [SetUp]
        public void Setup()
        {
            _target = new Revision { Id = 100 };
        }

        [Test]
        public void ShouldSetRevisionData()
        {
            var person = PersonFactory.CreatePerson("person1");
            _target.SetRevisionData(person);

            Assert.That(person, Is.EqualTo(_target.ModifiedBy));
        }

        [Test]
        public void ShouldGetHashCode()
        {
            Assert.That(_target.GetHashCode(), Is.EqualTo(100.GetHashCode()));
        }

        [Test]
        public void ShouldCheckEquality()
        {
            var revision = new Revision { Id = 100 };
            Assert.That(_target.Equals(revision), Is.True);

	        var obj = (object)PersonFactory.CreatePerson("person");
			Assert.That(_target.Equals(obj), Is.False);

	        Assert.That(_target.Equals(null), Is.False);
        }
    }
}
