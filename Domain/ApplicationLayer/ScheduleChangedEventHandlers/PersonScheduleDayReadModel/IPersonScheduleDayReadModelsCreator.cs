using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public interface IPersonScheduleDayReadModelsCreator
	{
		IEnumerable<PersonScheduleDayReadModel> MakeReadModels(ProjectionChangedEventBase schedule);

		PersonScheduleDayReadModel MakePersonScheduleDayReadModel(IPerson person,
			ProjectionChangedEventScheduleDay scheduleDay);
	}
}