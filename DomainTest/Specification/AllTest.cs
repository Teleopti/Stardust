using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.DomainTest.Specification
{
    [TestFixture]
    public class AllTest
    {
        private ISpecification<object> target;

        [SetUp]
        public void Setup()
        {
            target = new All<object>();
        }

        [Test]
        public void VerifySpecifiedBy()
        {
            Assert.IsTrue(target.IsSatisfiedBy(null));
            Assert.IsTrue(target.IsSatisfiedBy(new object()));
        }

    }
}
