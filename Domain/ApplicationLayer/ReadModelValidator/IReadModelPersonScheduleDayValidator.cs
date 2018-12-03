using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public interface IReadModelPersonScheduleDayValidator
	{
		bool Validate(IPerson person,IScheduleDay scheduleDay, ReadModelValidationMode mode);
		PersonScheduleDayReadModel FetchFromRepository(IPerson person,DateOnly date);
		PersonScheduleDayReadModel Build(Guid personId,DateOnly date);
		PersonScheduleDayReadModel Build(IPerson person,IScheduleDay scheduleDay);
		bool IsInitialized();
	}
}