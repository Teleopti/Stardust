using NUnit.Framework;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security
{
    [TestFixture]
    public class IsPasswordEncryptedSpecificationTest
    {
        private ISpecification<string> target;

        [SetUp]
        public void Setup()
        {
            target = new IsPasswordEncryptedSpecification();
        }

        [Test]
        public void VerifyWithEncryptedPassword()
        {
            Assert.IsTrue(target.IsSatisfiedBy("###02EFAE5ECEC5F5E87D4F93449CF3B13DE2985C69###"));
        }

        [Test]
        public void VerifyWithNonEncryptedPassword()
        {
            Assert.IsFalse(target.IsSatisfiedBy("abc123"));
        }

        [Test]
        public void VerifyWithNull()
        {
            Assert.IsFalse(target.IsSatisfiedBy(null));
        }

        [Test]
        public void VerifyWithCloseOne()
        {
            Assert.IsFalse(target.IsSatisfiedBy("##                           ###"));
        }
    }
}
