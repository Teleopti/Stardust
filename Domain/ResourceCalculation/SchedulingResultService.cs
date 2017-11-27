using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

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

		public ISkillResourceCalculationPeriodDictionary SchedulingResult(DateTimePeriod periodToRecalculate, ResourceCalculationData resourceCalculationData = null,  bool emptyCache = true)
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
				return new ResourceCalculationDataContainer(Enumerable.Empty<BpoResource>(), _personSkillProvider, 60, false);

			var minutesSplit = _allSkills.Min(s => s.DefaultResolution);
			var resources = new ResourceCalculationDataContainer(Enumerable.Empty<BpoResource>(), _personSkillProvider, minutesSplit, false);

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
		private readonly IEnumerable<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>> _items;
		private readonly Dictionary<ISkill, IResourceCalculationPeriodDictionary> _wrappedDictionary = new	Dictionary<ISkill, IResourceCalculationPeriodDictionary>();
		public SkillResourceCalculationPeriodWrapper(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods)
		{
			_items = ((SkillSkillStaffPeriodExtendedDictionary)relevantSkillStaffPeriods).Select(w => new KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>(w.Key, w.Value));
			foreach (var relevantSkillStaffPeriod in relevantSkillStaffPeriods)
			{
				_wrappedDictionary.Add(relevantSkillStaffPeriod.Key, relevantSkillStaffPeriod.Value);
			}
		}

		public SkillResourceCalculationPeriodWrapper(List<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>> result)
		{
			_items = result;
			foreach (var relevantSkillStaffPeriod in result)
			{
				_wrappedDictionary.Add(relevantSkillStaffPeriod.Key, relevantSkillStaffPeriod.Value);
			}
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

			//var wrappedDictionary = _items.FirstOrDefault(x => x.Key.Equals(skill)).Value;

			//if (wrappedDictionary != null)
			//{
			//	resourceCalculationPeriods = wrappedDictionary;
			//	return true;
			//}
			//resourceCalculationPeriods = null;
			//return false;
		}

		public bool IsOpen(ISkill skill, DateTimePeriod periodToCalculate)
		{
			IResourceCalculationPeriodDictionary thisDic;
			if (!_wrappedDictionary.TryGetValue(skill, out thisDic))
				return false;

			IResourceCalculationPeriod items;

			return thisDic.TryGetValue(periodToCalculate, out items);

			//var pair = _items.Where(x => x.Key.Equals(skill)).ToList();
			//if (!pair.Any()) return false;

			//if (pair.First().Value == null) return false;
			//IResourceCalculationPeriodDictionary resources = pair.First().Value;
			
			//IResourceCalculationPeriod items;
			
			//return resources.TryGetValue(periodToCalculate, out items);
		}

		public IEnumerable<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>> Items()
		{
			return _items;
		}

		public bool TryGetDataForInterval(ISkill skill, DateTimePeriod period, out IShovelResourceDataForInterval dataForInterval)
		{
			IResourceCalculationPeriodDictionary thisDic;
			if (_wrappedDictionary.TryGetValue(skill, out thisDic))
			{
				IResourceCalculationPeriod skillStaffPeriod;
				if (thisDic.TryGetValue(period, out skillStaffPeriod))
				{
					dataForInterval = (IShovelResourceDataForInterval)skillStaffPeriod;
					return true;
				}
			}

			dataForInterval = null;
			return false;

			//var wrappedDictionary = _items.FirstOrDefault(x => x.Key.Equals(skill)).Value;
			//if (wrappedDictionary != null)
			//{
			//	IResourceCalculationPeriod skillStaffPeriod;
			//	if (wrappedDictionary.TryGetValue(period, out skillStaffPeriod))
			//	{
			//		dataForInterval = (IShovelResourceDataForInterval) skillStaffPeriod;
			//		return true;
			//	}
			//}
			//dataForInterval = null;
			//return false;
		}
	}
}