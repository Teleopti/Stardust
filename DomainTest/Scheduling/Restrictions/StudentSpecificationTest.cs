using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class StudentSpecificationTest
    {
        private ISpecification<IRestrictionBase> spec;

        [SetUp]
        public void Setup()
        {
            spec = new StudentSpecification();
        }

        [Test]
        public void IsNonValid()
        {
            var item = new AvailabilityRestriction();
            Assert.IsFalse(spec.IsSatisfiedBy(item));
        }

        [Test]
        public void IsValid()
        {
            var item = new StudentAvailabilityRestriction();
            Assert.IsTrue(spec.IsSatisfiedBy(item));
        }

        [Test]
        public void VerifyNull()
        {
            Assert.IsFalse(spec.IsSatisfiedBy(null));
        }
    }
}
