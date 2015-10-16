using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Requests
{
	public class DontPersistIfNoPermissionTest : RequestPersisterBaseTest
	{
		protected override IPersonRequest Given()
		{
			PersonRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			PrincipalAuthorization = new PrincipalAuthorizationWithNoPermission();
			return new PersonRequest(Person, new TextRequest(new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
		}

		protected override IPersonRequest When(IPersonRequest currentRequest)
		{
			currentRequest.Subject = "sdfsdf";
			return currentRequest;
		}

		protected override void Then(IPersonRequest yourRequest)
		{
			PersonRequestRepository.AssertWasNotCalled(x => x.Add(yourRequest));
			ClearRefferedRequestsWasCalled.Should().Be.False();
		}
	}
}