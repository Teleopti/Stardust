using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public interface IScheduleDayReadModelsCreator
	{
		ScheduleDayReadModel GetReadModel(ProjectionChangedEventScheduleDay schedule, IPerson person);
	}
}