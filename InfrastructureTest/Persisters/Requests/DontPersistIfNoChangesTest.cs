using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Requests
{
	public class DontPersistIfNoChangesTest : RequestPersisterBaseTest
	{
		protected override IPersonRequest Given()
		{
			PersonRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			return new PersonRequest(Person, new TextRequest(new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
		}

		protected override IPersonRequest When(IPersonRequest currentRequest)
		{
			return currentRequest;
		}

		protected override void Then(IPersonRequest yourRequest)
		{
			PersonRequestRepository.AssertWasNotCalled(x => x.Add(yourRequest));
			//don't understand why - but this is how it worked before...
			ClearRefferedRequestsWasCalled.Should().Be.True();
		}
	}
}