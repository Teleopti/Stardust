using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ScheduleControllerPrerequisites : IScheduleControllerPrerequisites
	{
		private readonly IActivityRepository _activityRepository;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;

		public ScheduleControllerPrerequisites(IActivityRepository activityRepository, IDayOffTemplateRepository dayOffTemplateRepository, IPartTimePercentageRepository partTimePercentageRepository)
		{
			_activityRepository = activityRepository;
			_dayOffTemplateRepository = dayOffTemplateRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
		}

		public void MakeSureLoaded()
		{
			_activityRepository.LoadAll();
			_dayOffTemplateRepository.LoadAll();
			_partTimePercentageRepository.LoadAll();
		}
	}
}