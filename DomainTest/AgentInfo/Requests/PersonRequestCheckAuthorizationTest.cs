using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class PersonRequestCheckAuthorizationTest
    {
        private IPersonRequestCheckAuthorization target;
        private PersonRequestFactory personRequestFactory;
        private IPersonRequest personRequest;

        [SetUp]
        public void Setup()
        {
            personRequestFactory = new PersonRequestFactory();
            personRequest = personRequestFactory.CreatePersonRequest();

            target = new PersonRequestCheckAuthorization();
        }

        [Test]
        public void ShouldHavePermissionToEdit()
        {
            target.HasEditRequestPermission(personRequest).Should().Be.True();
        }

        [Test]
        public void ShouldHavePermissionToView()
        {
            target.HasViewRequestPermission(personRequest).Should().Be.True();
        }

        [Test,ExpectedException(typeof(PermissionException))]
        public void ShouldThrowWhenNoPermissionWhenVerifying()
        {
            using(new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
            {
                target.VerifyEditRequestPermission(personRequest);
            }
        }
    }
}
