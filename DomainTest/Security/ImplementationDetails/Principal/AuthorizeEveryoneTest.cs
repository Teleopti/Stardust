using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.Principal
{
    [TestFixture]
    public class AuthorizeEveryoneTest
    {
        private IAuthorizeAvailableData target;

        [SetUp]
        public void Setup()
        {
            target = new AuthorizeEveryone();
        }

        [Test]
        public void ShouldAuthorizePerson()
        {
            target.Check(null,DateOnly.Today,(IPerson)null).Should().Be.True();
        }

        [Test]
        public void ShouldAuthorizeTeam()
        {
            target.Check(null, DateOnly.Today, (ITeam)null).Should().Be.True();
        }

        [Test]
        public void ShouldAuthorizeSite()
        {
            target.Check(null, DateOnly.Today, (ISite)null).Should().Be.True();
        }

        [Test]
        public void ShouldAuthorizeBusinessUnit()
        {
            target.Check(null, DateOnly.Today, (IBusinessUnit)null).Should().Be.True();
        }

        [Test]
        public void ShouldAuthorizeOrganisation()
        {
            target.Check(null, DateOnly.Today, (IPersonAuthorization)null).Should().Be.True();
        }
    }
}
