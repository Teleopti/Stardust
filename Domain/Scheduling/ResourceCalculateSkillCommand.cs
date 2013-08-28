using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IResourceCalculateSkillCommand
	{
		void Execute(IScenario scenario, DateTimePeriod period, ISkill skill, ResourceCalculationDataContainerFromStorage resourceCalculationDataContainer);
	}

	public class ResourceCalculateSkillCommand : IResourceCalculateSkillCommand
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly ISkillLoaderDecider _skillLoaderDecider;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly ISkillRepository _skillRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IScheduledResourcesReadModelReader _storage;

		public ResourceCalculateSkillCommand(ISkillRepository skillRepository, IWorkloadRepository workloadRepository, IScheduledResourcesReadModelReader storage, ISchedulingResultStateHolder schedulingResultStateHolder, ISkillLoaderDecider skillLoaderDecider, ISkillDayLoadHelper skillDayLoadHelper)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_skillLoaderDecider = skillLoaderDecider;
			_skillDayLoadHelper = skillDayLoadHelper;
			_skillRepository = skillRepository;
			_workloadRepository = workloadRepository;
			_storage = storage;
		}

		public void Execute(IScenario scenario, DateTimePeriod period, ISkill skill, ResourceCalculationDataContainerFromStorage resourceCalculationDataContainer)
		{
			var dateOnlyPeriod = period.ToDateOnlyPeriod(skill.TimeZone);

			var skills = _skillRepository.FindAllWithSkillDays(dateOnlyPeriod);
			var allSkills = skills.ToList();
			_workloadRepository.LoadAll();

			_skillLoaderDecider.Execute(scenario, period, skill);
			_skillLoaderDecider.FilterSkills(skills);

			if (skills != null)
				skills.ForEach(_schedulingResultStateHolder.Skills.Add);

			var result = _storage.ForPeriod(period, allSkills);
			resourceCalculationDataContainer.AddResources(result, _skillRepository.MinimumResolution());

		    _schedulingResultStateHolder.SkillDays =
		        _skillDayLoadHelper.LoadSchedulerSkillDays(dateOnlyPeriod, skills, scenario);
		}
	}
}
