using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Requests
{
	public class UpdateNoConflictTest : RequestPersisterBaseTest
	{
		protected override IPersonRequest Given()
		{
			return new PersonRequest(Person, new TextRequest(new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
		}

		protected override IPersonRequest When(IPersonRequest currentRequest)
		{
			currentRequest.TrySetMessage("yes");
			return currentRequest;
		}

		protected override void Then(IPersonRequest yourRequest)
		{
			yourRequest.GetMessage(new NoFormatting()).Should().Be.EqualTo("yes");
			ClearRefferedRequestsWasCalled.Should().Be.True();
			yourRequest.Changed.Should().Be.False();
		}
	}
}