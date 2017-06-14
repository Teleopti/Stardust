﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
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

		public OvertimeWrapperModel Execute(IOvertimePreferences overtimePreferences, ISchedulingProgress schedulingProgress, IList<IScheduleDay> scheduleDays,
							DateTimePeriod requestedDateTimePeriod, IList<ISkill> skills, IList<ISkill> skillsToAddOvertime )
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
				var resourceCalculationPeriods = new List<SkillStaffingInterval>();
				var overtimeModels = new List<OverTimeModel>();
				foreach (var skill in skillsToAddOvertime)
				{
					overtimePreferences.SkillActivity = skill.Activity;
					overtimeModels.AddRange(_scheduleOvertimeWithoutStateHolder.Execute(overtimePreferences, schedulingProgress, scheduleDays,
						requestedDateTimePeriod, resCalcData, () => getContext(combinationResources, skills, true)));
					var xx = resCalcData.SkillResourceCalculationPeriodDictionary.Items().Where(x =>  x.Key==skill );
					
					foreach (var keyValuePair in xx)
					{
						var intervals = keyValuePair.Value.OnlyValues().Cast<SkillStaffingInterval>();
						resourceCalculationPeriods.AddRange(intervals);
					}
				}
				
				return new OvertimeWrapperModel(resourceCalculationPeriods, overtimeModels);
			}
		}


		private static IDisposable getContext(List<SkillCombinationResource> combinationResources, IList<ISkill> skills, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataConatainerFromSkillCombinations(combinationResources, skills, useAllSkills)));
		}

		
	}

	public class OvertimeWrapperModel
	{
		public List<SkillStaffingInterval> ResourceCalculationPeriods { get; }
		public IList<OverTimeModel> Models { get; }

		public OvertimeWrapperModel(List<SkillStaffingInterval> resourceCalculationPeriods, IList<OverTimeModel> models)
		{
			ResourceCalculationPeriods = resourceCalculationPeriods;
			Models = models;
		}
	}
}
