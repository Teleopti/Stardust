using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IEventContextPopulator
	{
		void SetMessageDetail(IEvent @event);
		void SetMessageDetail(IRaptorDomainMessageInfo @event);
	}
}