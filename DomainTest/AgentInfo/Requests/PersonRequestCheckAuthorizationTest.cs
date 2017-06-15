using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
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

        [Test]
        public void ShouldThrowWhenNoPermissionWhenVerifying()
        {
            using(CurrentAuthorization.ThreadlyUse(new NoPermission()))
            {
                Assert.Throws<PermissionException>(() => target.VerifyEditRequestPermission(personRequest));
            }
        }
    }
}
