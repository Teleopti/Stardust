using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public interface IScheduleDayReadModelCreator
	{
		ScheduleDayReadModel TurnScheduleToModel(IScheduleDay scheduleDay);
	}
}