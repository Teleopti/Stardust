using NUnit.Framework;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    public class SystemUserPasswordSpecificationTest
    {
        private ISpecification<string> target;

        [SetUp]
        public void Setup()
        {
            target = new SystemUserPasswordSpecification();
        }

        [Test]
        public void VerifySystemUserPassword()
        {
            Assert.IsTrue(target.IsSatisfiedBy(SuperUser.Password));
        }

        [Test]
        public void VerifyNonSystemUserPassword()
        {
            Assert.IsFalse(target.IsSatisfiedBy("KalleK"));
        }

        [Test]
        public void VerifyNullUserPassword()
        {
            Assert.IsFalse(target.IsSatisfiedBy(null));
        }
    }
}
