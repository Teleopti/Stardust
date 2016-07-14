using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public interface IReadModelScheduleDayValidator
	{
		bool Validate(IPerson person,IScheduleDay scheduleDay, bool directFix);
		ScheduleDayReadModel FetchFromRepository(IPerson person,DateOnly date);
		ScheduleDayReadModel Build(IPerson person,IScheduleDay scheduleDay);
		ScheduleDayReadModel Build(Guid personId,DateOnly date);
	}
}