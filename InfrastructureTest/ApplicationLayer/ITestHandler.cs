using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	public interface ITestHandler : IHandleEvent<TestEvent>, IHandleEvent<TestEventTwo>
	{
	}
}