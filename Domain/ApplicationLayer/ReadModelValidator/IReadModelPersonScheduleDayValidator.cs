using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public interface IReadModelPersonScheduleDayValidator
	{
		bool Validate(IPerson person,DateOnly date,IScheduleDay scheduleDay);
		PersonScheduleDayReadModel FetchFromRepository(IPerson person,DateOnly date);
		PersonScheduleDayReadModel Build(Guid personId,DateOnly date);
		PersonScheduleDayReadModel Build(IPerson person,IScheduleDay scheduleDay);
	}
}