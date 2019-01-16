using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public interface ITeamScheduleWeekViewChangeChecker
	{
		bool IsRelevantChange(DateOnly date, IPerson person, ScheduleDayReadModel newReadModel);
	}

	public class TeamScheduleWeekViewChangeChecker : ITeamScheduleWeekViewChangeChecker
	{
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		public TeamScheduleWeekViewChangeChecker(IScheduleDayReadModelRepository scheduleDayReadModelRepository) {
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}
		public bool IsRelevantChange(DateOnly date, IPerson person, ScheduleDayReadModel newReadModel)
		{
			var existingReadModel = _scheduleDayReadModelRepository.ForPerson(date, person.Id.GetValueOrDefault());

			if (existingReadModel?.Workday != newReadModel?.Workday
				|| existingReadModel?.StartDateTime != newReadModel?.StartDateTime
				|| existingReadModel?.EndDateTime != newReadModel?.EndDateTime
				|| existingReadModel?.Label != newReadModel?.Label) {
				return true;
			}
	
			return false;
		}
	}
}
