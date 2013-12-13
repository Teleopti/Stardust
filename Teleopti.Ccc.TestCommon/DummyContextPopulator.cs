using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.TestCommon
{
	public class DummyContextPopulator : IEventContextPopulator
	{
		public void SetMessageDetail(IEvent @event)
		{
		}

		public void SetMessageDetail(IRaptorDomainMessageInfo @event)
		{
		}
	}
}