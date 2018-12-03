using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Requests
{
	public class DontPersistIfNoPermissionTest : RequestPersisterBaseTest
	{
		protected override IPersonRequest Given()
		{
			PersonRequestRepository = new FakePersonRequestRepository();
			Authorization = new NoPermission();
			return new PersonRequest(Person, new TextRequest(new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
		}

		protected override IPersonRequest When(IPersonRequest currentRequest)
		{
			currentRequest.Subject = "sdfsdf";
			return currentRequest;
		}

		protected override void Then(IPersonRequest yourRequest)
		{
			PersonRequestRepository.LoadAll().Should().Not.Contain(yourRequest);
			ClearRefferedRequestsWasCalled.Should().Be.False();
		}
	}
}