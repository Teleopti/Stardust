using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SchedulingResultService : ISchedulingResultService
	{
		private readonly ISkillSkillStaffPeriodExtendedDictionary _relevantSkillStaffPeriods;
		private readonly IList<ISkill> _allSkills;
		private readonly IResourceCalculationDataContainer _relevantProjections;
		private readonly bool _useOccupancyAdjustment;
		private readonly IPersonSkillProvider _personSkillProvider;

		public SchedulingResultService(ISchedulingResultStateHolder stateHolder,
			IList<ISkill> allSkills,
			bool useOccupancyAdjustment,
			IPersonSkillProvider personSkillProvider)
		{
			_useOccupancyAdjustment = useOccupancyAdjustment;
			_relevantSkillStaffPeriods = stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary;
			_allSkills = allSkills;
			_personSkillProvider = personSkillProvider;
			_relevantProjections = createRelevantProjectionList(stateHolder.Schedules);
		}


		public SchedulingResultService(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods,
			IList<ISkill> allSkills,
			IResourceCalculationDataContainer relevantProjections,
			bool useOccupancyAdjustment,
			IPersonSkillProvider personSkillProvider)
		{
			_relevantSkillStaffPeriods = relevantSkillStaffPeriods;
			_allSkills = allSkills;
			_relevantProjections = relevantProjections;
			_useOccupancyAdjustment = useOccupancyAdjustment;
			_personSkillProvider = personSkillProvider;
		}

		//only used by ETL
		public ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodWithSchedules)
		{
			DateTimePeriod? relevantPeriod = _relevantSkillStaffPeriods.Period();
			if (!relevantPeriod.HasValue)
				return _relevantSkillStaffPeriods;

			var intersectingPeriod = relevantPeriod.Value.Intersection(periodWithSchedules);

			if (!intersectingPeriod.HasValue)
				return _relevantSkillStaffPeriods;

			return SchedulingResult(intersectingPeriod.Value,true);
		}

		public ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodToRecalculate, bool emptyCache)
		{
			if (_allSkills.Count == 0)
				return _relevantSkillStaffPeriods;

			var period = periodToRecalculate.ToDateOnlyPeriod(TimeZoneGuard.Instance.TimeZone);
			var datePeriod = new DateOnlyPeriod(period.StartDate.AddDays(-1), period.EndDate.AddDays(1));
			IAffectedPersonSkillService personSkillService = new AffectedPersonSkillService(datePeriod, _allSkills);

			var rc = new ScheduleResourceOptimizer(_relevantProjections, _relevantSkillStaffPeriods, personSkillService,
			                                       emptyCache, new ActivityDivider());
			rc.Optimize(periodToRecalculate, _useOccupancyAdjustment);

			return _relevantSkillStaffPeriods;
		}

		private ResourceCalculationDataContainer createRelevantProjectionList(IScheduleDictionary scheduleDictionary)
		{
			if (_allSkills.Count.Equals(0))
				return new ResourceCalculationDataContainer(_personSkillProvider, 60);

			int minutesSplit = _allSkills.Min(s => s.DefaultResolution);
			var resources = new ResourceCalculationDataContainer(_personSkillProvider, minutesSplit);

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