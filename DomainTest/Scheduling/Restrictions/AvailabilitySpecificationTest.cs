using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class AvailabilitySpecificationTest
    {
        private ISpecification<IRestrictionBase> spec;

        [SetUp]
        public void Setup()
        {
            spec = new AvailabilitySpecification();
        }

        [Test]
        public void IsValid()
        {
            var item = new AvailabilityRestriction();
            Assert.IsTrue(spec.IsSatisfiedBy(item));
        }

        [Test]
        public void IsNonValid()
        {
            var item = new RotationRestriction();
            Assert.IsFalse(spec.IsSatisfiedBy(item));
        }

        [Test]
        public void VerifyNull()
        {
            Assert.IsFalse(spec.IsSatisfiedBy(null));
        }
    }
}
