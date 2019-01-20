using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SchedulingResultService : ISchedulingResultService
	{
		private readonly ISkillResourceCalculationPeriodDictionary _relevantSkillStaffPeriods;
		private readonly IEnumerable<ISkill> _allSkills;
		private readonly IResourceCalculationDataContainer _relevantProjections;
		private readonly IPersonSkillProvider _personSkillProvider;

		public SchedulingResultService(ISchedulingResultStateHolder stateHolder,
			IEnumerable<ISkill> allSkills,
			IPersonSkillProvider personSkillProvider)
		{
			_relevantSkillStaffPeriods = new SkillResourceCalculationPeriodWrapper(stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary);
			_allSkills = allSkills;
			_personSkillProvider = personSkillProvider;
			_relevantProjections = createRelevantProjectionList(stateHolder.Schedules);
		}


		public SchedulingResultService(ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods,
			IEnumerable<ISkill> allSkills,
			IResourceCalculationDataContainer relevantProjections,
			IPersonSkillProvider personSkillProvider)
		{
			_relevantSkillStaffPeriods = relevantSkillStaffPeriods;
			_allSkills = allSkills;
			_relevantProjections = relevantProjections;
			_personSkillProvider = personSkillProvider;
		}

		public ISkillResourceCalculationPeriodDictionary SchedulingResult(DateTimePeriod periodToRecalculate, ResourceCalculationData resourceCalculationData = null, bool emptyCache = true)
		{
			if (!emptyCache)
			{
				DateTimePeriod? relevantPeriod = Period(_relevantSkillStaffPeriods.Items());
				if (!relevantPeriod.HasValue)
					return _relevantSkillStaffPeriods;

				var intersectingPeriod = relevantPeriod.Value.Intersection(periodToRecalculate);

				if (!intersectingPeriod.HasValue)
					return _relevantSkillStaffPeriods;

			}

			if (!_allSkills.Any())
				return _relevantSkillStaffPeriods;
			var personSkillService = new AffectedPersonSkillService(_allSkills);

			var rc = new ScheduleResourceOptimizer(_relevantProjections, _relevantSkillStaffPeriods, personSkillService,
												   emptyCache, new ActivityDivider());
			rc.Optimize(periodToRecalculate, resourceCalculationData);

			return _relevantSkillStaffPeriods;
		}

		private ResourceCalculationDataContainer createRelevantProjectionList(IScheduleDictionary scheduleDictionary)
		{
			if (!_allSkills.Any())
				return new ResourceCalculationDataContainer(Enumerable.Empty<ExternalStaff>(), _personSkillProvider, 60, false, new ActivityDivider());

			var minutesSplit = _allSkills.Min(s => s.DefaultResolution);
			var resources = new ResourceCalculationDataContainer(Enumerable.Empty<ExternalStaff>(), _personSkillProvider, minutesSplit, false, new ActivityDivider());

			foreach (var item in scheduleDictionary)
			{
				var range = item.Value;
				var period = range.TotalPeriod();
				if (!period.HasValue) continue;

				var scheduleDays =
					range.ScheduledDayCollection(period.Value.ToDateOnlyPeriod(item.Key.PermissionInformation.DefaultTimeZone()));
				foreach (var scheduleDay in scheduleDays)
				{
					resources.AddScheduleDayToContainer(scheduleDay);
				}
			}

			return resources;
		}

		public DateTimePeriod? Period(IEnumerable<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>> items)
		{
			DateTime? minTime = null, maxTime = null;

			foreach (var periodCollection in items.Select(x => x.Value))
			{
				if (periodCollection.Items().Any())
				{
					var currentMinTime = periodCollection.Items().Min(p => p.Key.StartDateTime);
					var currentMaxTime = periodCollection.Items().Max(p => p.Key.EndDateTime);
					if (!maxTime.HasValue || currentMaxTime > maxTime.Value)
					{
						maxTime = currentMaxTime;
					}

					if (!minTime.HasValue || currentMinTime < minTime.Value)
					{
						minTime = currentMinTime;
					}
				}
			}

			DateTimePeriod? returnValue = (!minTime.HasValue || !maxTime.HasValue)
														 ? (DateTimePeriod?)null
														 : new DateTimePeriod(minTime.Value, maxTime.Value);
			return returnValue;
		}
	}

	public class SkillResourceCalculationPeriodWrapper : ISkillResourceCalculationPeriodDictionary
	{
		private readonly Dictionary<ISkill, IResourceCalculationPeriodDictionary> _wrappedDictionary = new Dictionary<ISkill, IResourceCalculationPeriodDictionary>();

		public SkillResourceCalculationPeriodWrapper(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods)
		{
			_wrappedDictionary =
				((SkillSkillStaffPeriodExtendedDictionary)relevantSkillStaffPeriods).ToDictionary(k => k.Key, v => (IResourceCalculationPeriodDictionary)v.Value);
		}

		public SkillResourceCalculationPeriodWrapper(List<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>> result)
		{
			_wrappedDictionary = result.ToDictionary(k => k.Key, v => v.Value);
		}

		public bool TryGetValue(ISkill skill, out IResourceCalculationPeriodDictionary resourceCalculationPeriods)
		{
			IResourceCalculationPeriodDictionary thisDic;
			if (_wrappedDictionary.TryGetValue(skill, out thisDic))
			{
				resourceCalculationPeriods = thisDic;
				return true;
			}
			resourceCalculationPeriods = null;
			return false;
		}

		public bool IsOpen(ISkill skill, DateTimePeriod periodToCalculate)
		{
			if (!_wrappedDictionary.TryGetValue(skill, out var thisDic))
				return false;

			return thisDic.TryGetValue(periodToCalculate, out _);
		}

		public IEnumerable<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>> Items()
		{
			return _wrappedDictionary;
		}

		public bool TryGetDataForInterval(ISkill skill, DateTimePeriod period, out IShovelResourceDataForInterval dataForInterval)
		{
			if (_wrappedDictionary.TryGetValue(skill, out var thisDic))
			{
				if (thisDic.TryGetValue(period, out var skillStaffPeriod))
				{
					dataForInterval = (IShovelResourceDataForInterval)skillStaffPeriod;
					return true;
				}
			}

			dataForInterval = null;
			return false;
		}
	}
}