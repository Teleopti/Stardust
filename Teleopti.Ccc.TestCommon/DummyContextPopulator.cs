using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.TestCommon
{
	public class DummyContextPopulator : IEventContextPopulator
	{
		public void PopulateEventContext(params object[] events)
		{
		}
	}
}