using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class ScheduleControllerPrerequisites : IScheduleControllerPrerequisites
	{
		private readonly IActivityRepository _activityRepository;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;

		public ScheduleControllerPrerequisites(IActivityRepository activityRepository, IDayOffTemplateRepository dayOffTemplateRepository)
		{
			_activityRepository = activityRepository;
			_dayOffTemplateRepository = dayOffTemplateRepository;
		}

		public void MakeSureLoaded()
		{
			_activityRepository.LoadAll();
			_dayOffTemplateRepository.LoadAll();
		}
	}
}