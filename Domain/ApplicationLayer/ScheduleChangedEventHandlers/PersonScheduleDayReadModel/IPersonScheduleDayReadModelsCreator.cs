using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public interface IPersonScheduleDayReadModelsCreator
	{
		IEnumerable<PersonScheduleDayReadModel> MakeReadModels(ProjectionChangedEventBase schedule);
	}
}