using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public interface IReadModelScheduleDayValidator
	{
		bool Validate(IPerson person,IScheduleDay scheduleDay,ReadModelValidationMode mode);
		ScheduleDayReadModel FetchFromRepository(IPerson person,DateOnly date);
		ScheduleDayReadModel Build(IPerson person,IScheduleDay scheduleDay);
		ScheduleDayReadModel Build(Guid personId,DateOnly date);
		bool IsInitialized();
	}
}