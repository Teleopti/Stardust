﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SchedulingResultService : ISchedulingResultService
	{
		private readonly ISkillSkillStaffPeriodExtendedDictionary _relevantSkillStaffPeriods;
		private readonly IEnumerable<ISkill> _allSkills;
		private readonly IResourceCalculationDataContainer _relevantProjections;
		private readonly IPersonSkillProvider _personSkillProvider;

		public SchedulingResultService(ISchedulingResultStateHolder stateHolder,
			IEnumerable<ISkill> allSkills,
			IPersonSkillProvider personSkillProvider)
		{
			_relevantSkillStaffPeriods = stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary;
			_allSkills = allSkills;
			_personSkillProvider = personSkillProvider;
			_relevantProjections = createRelevantProjectionList(stateHolder.Schedules);
		}


		public SchedulingResultService(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods,
			IEnumerable<ISkill> allSkills,
			IResourceCalculationDataContainer relevantProjections,
			IPersonSkillProvider personSkillProvider)
		{
			_relevantSkillStaffPeriods = relevantSkillStaffPeriods;
			_allSkills = allSkills;
			_relevantProjections = relevantProjections;
			_personSkillProvider = personSkillProvider;
		}

		//public ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodWithSchedules, IResourceCalculationData resourceCalculationData = null )
		//{
		//	DateTimePeriod? relevantPeriod = _relevantSkillStaffPeriods.Period();
		//	if (!relevantPeriod.HasValue)
		//		return _relevantSkillStaffPeriods;

		//	var intersectingPeriod = relevantPeriod.Value.Intersection(periodWithSchedules);

		//	if (!intersectingPeriod.HasValue)
		//		return _relevantSkillStaffPeriods;

		//	return SchedulingResult(intersectingPeriod.Value,resourceCalculationData);
		//}

		public ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodToRecalculate, IResourceCalculationData resourceCalculationData = null,  bool emptyCache = true)
		{
			if (!emptyCache)
			{
				DateTimePeriod? relevantPeriod = _relevantSkillStaffPeriods.Period();
				if (!relevantPeriod.HasValue)
					return _relevantSkillStaffPeriods;

				var intersectingPeriod = relevantPeriod.Value.Intersection(periodToRecalculate);

				if (!intersectingPeriod.HasValue)
					return _relevantSkillStaffPeriods;

			}

			if (!_allSkills.Any())
				return _relevantSkillStaffPeriods;
			IAffectedPersonSkillService personSkillService = new AffectedPersonSkillService(_allSkills);

			var rc = new ScheduleResourceOptimizer(_relevantProjections, _relevantSkillStaffPeriods, personSkillService,
			                                       emptyCache, new ActivityDivider());
			rc.Optimize(periodToRecalculate, resourceCalculationData);

			return _relevantSkillStaffPeriods;
		}

		private ResourceCalculationDataContainer createRelevantProjectionList(IScheduleDictionary scheduleDictionary)
		{
			if (!_allSkills.Any())
				return new ResourceCalculationDataContainer(_personSkillProvider, 60, false);

			int minutesSplit = _allSkills.Min(s => s.DefaultResolution);
			var resources = new ResourceCalculationDataContainer(_personSkillProvider, minutesSplit, false);

			Parallel.ForEach(scheduleDictionary.Keys, person =>
			{
				var range = scheduleDictionary[person];
				var period = range.TotalPeriod();
				if (!period.HasValue) return;

				var scheduleDays =
					range.ScheduledDayCollection(period.Value.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()));
				Parallel.ForEach(scheduleDays, scheduleDay => resources.AddScheduleDayToContainer(scheduleDay, minutesSplit));
			});

			return resources;
		}
	}
}