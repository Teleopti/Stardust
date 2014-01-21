using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.TestCommon
{
	public class DummyContextPopulator : IEventContextPopulator
	{
		public void PopulateEventContext(IEvent @event)
		{
		}

		public void PopulateEventContext(IRaptorDomainMessageInfo @event)
		{
		}

		public void PopulateEventContextWithoutInitiator(IRaptorDomainMessageInfo message)
		{
		}
	}
}