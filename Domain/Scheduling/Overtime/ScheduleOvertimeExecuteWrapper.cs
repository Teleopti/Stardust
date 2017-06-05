using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{

	public class ScheduleOvertimeExecuteWrapper
	{
		private readonly ScheduleOvertimeWithoutStateHolder _scheduleOvertimeWithoutStateHolder;
		private readonly IExtractSkillForecastIntervals _extractSkillForecastIntervals;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;

		public ScheduleOvertimeExecuteWrapper(ScheduleOvertimeWithoutStateHolder scheduleOvertimeWithoutStateHolder, IExtractSkillForecastIntervals extractSkillForecastIntervals, ISkillCombinationResourceRepository skillCombinationResourceRepository)
		{
			_scheduleOvertimeWithoutStateHolder = scheduleOvertimeWithoutStateHolder;
			_extractSkillForecastIntervals = extractSkillForecastIntervals;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
		}

		public HashSet<IPerson> Execute(IOvertimePreferences overtimePreferences, ISchedulingProgress schedulingProgress, IList<IScheduleDay> scheduleDays,
							DateTimePeriod requestedDateTimePeriod, IList<ISkill> skills)
		{
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(requestedDateTimePeriod).ToList();
			using (getContext(combinationResources, skills, false))
			{
				var skillStaffingIntervals = _extractSkillForecastIntervals.GetBySkills(skills, requestedDateTimePeriod, false).ToList(); //shrinkage = false
				skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

				var relevantSkillStaffPeriods =
					skillStaffingIntervals.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
						.ToDictionary(k => k.Key,
									  v =>
										  (IResourceCalculationPeriodDictionary)
										  new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
																								 s => (IResourceCalculationPeriod) s)));
				var resCalcData = new ResourceCalculationData(skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));

				return _scheduleOvertimeWithoutStateHolder.Execute(overtimePreferences, schedulingProgress, scheduleDays, requestedDateTimePeriod, resCalcData, () => getContext(combinationResources, skills, true));
			}
		}


		private static IDisposable getContext(List<SkillCombinationResource> combinationResources, IList<ISkill> skills, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataConatainerFromSkillCombinations(combinationResources, skills, useAllSkills)));
		}
	}
}
