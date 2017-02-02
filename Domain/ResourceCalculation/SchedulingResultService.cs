using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
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

		public ISkillResourceCalculationPeriodDictionary SchedulingResult(DateTimePeriod periodToRecalculate, IResourceCalculationData resourceCalculationData = null,  bool emptyCache = true)
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
			var personSkillService = new AffectedPersonSkillService(_allSkills);

			var rc = new ScheduleResourceOptimizer(_relevantProjections, _relevantSkillStaffPeriods, personSkillService,
			                                       emptyCache, new ActivityDivider());
			rc.Optimize(periodToRecalculate, resourceCalculationData);

			return _relevantSkillStaffPeriods;
		}

		private ResourceCalculationDataContainer createRelevantProjectionList(IScheduleDictionary scheduleDictionary)
		{
			if (!_allSkills.Any())
				return new ResourceCalculationDataContainer(_personSkillProvider, 60, false);

			var minutesSplit = _allSkills.Min(s => s.DefaultResolution);
			var resources = new ResourceCalculationDataContainer(_personSkillProvider, minutesSplit, false);

			foreach (var item in scheduleDictionary)
			{
				var range = item.Value;
				var period = range.TotalPeriod();
				if (!period.HasValue) continue;

				var scheduleDays =
					range.ScheduledDayCollection(period.Value.ToDateOnlyPeriod(item.Key.PermissionInformation.DefaultTimeZone()));
				foreach (var scheduleDay in scheduleDays)
				{
					resources.AddScheduleDayToContainer(scheduleDay, minutesSplit);
				}
			}

			return resources;
		}
	}

	public class SkillResourceCalculationPeriodWrapper : ISkillResourceCalculationPeriodDictionary
	{
		private readonly ISkillSkillStaffPeriodExtendedDictionary _relevantSkillStaffPeriods;

		public SkillResourceCalculationPeriodWrapper(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods)
		{
			_relevantSkillStaffPeriods = relevantSkillStaffPeriods;
		}

		public bool TryGetValue(ISkill skill, out IResourceCalculationPeriodDictionary resourceCalculationPeriods)
		{
			ISkillStaffPeriodDictionary wrappedDictionary;
			if (_relevantSkillStaffPeriods.TryGetValue(skill, out wrappedDictionary))
			{
				resourceCalculationPeriods = wrappedDictionary;
				return true;
			}
			resourceCalculationPeriods = null;
			return false;
		}

		public bool IsOpen(ISkill skill, DateTimePeriod periodToCalculate)
		{
			ISkillStaffPeriodDictionary dictionary;
			if (_relevantSkillStaffPeriods.TryGetValue(skill, out dictionary))
			{
				return dictionary.SkillOpenHoursCollection.Any(openHourPeriod => openHourPeriod.Intersect(periodToCalculate));
			}
			return false;
		}

		public IEnumerable<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>> Items()
		{
			return ((SkillSkillStaffPeriodExtendedDictionary)_relevantSkillStaffPeriods).Select(w => new KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>(w.Key, w.Value));
		}

		public DateTimePeriod? Period()
		{
			return _relevantSkillStaffPeriods.Period();
		}
	}
}