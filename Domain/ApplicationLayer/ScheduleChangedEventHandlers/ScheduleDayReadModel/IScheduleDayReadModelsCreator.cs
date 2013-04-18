using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public interface IScheduleDayReadModelsCreator
	{
		ScheduleDayReadModel GetReadModel(ProjectionChangedEventScheduleDay schedule, IPerson person);
	}
}