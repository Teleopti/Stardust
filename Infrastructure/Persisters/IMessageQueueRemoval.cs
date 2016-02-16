using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IMessageQueueRemoval
	{
		void Remove(IEventMessage eventMessage);
		void Remove(PersistConflict persistConflict);
	}
}