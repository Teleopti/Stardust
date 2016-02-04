using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.TestCommon
{
	public class DummyInfrastructureInfoPopulator : IEventInfrastructureInfoPopulator
	{
		public void PopulateEventContext(params object[] events)
		{
		}
	}
}