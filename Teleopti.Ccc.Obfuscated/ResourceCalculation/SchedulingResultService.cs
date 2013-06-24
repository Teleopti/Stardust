using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
	public class SchedulingResultService : ISchedulingResultService
	{
		private readonly ISkillSkillStaffPeriodExtendedDictionary _relevantSkillStaffPeriods;
		private readonly IList<ISkill> _allSkills;
		private readonly IResourceCalculationDataContainer _relevantProjections;
		private readonly ISingleSkillCalculator _singleSkillCalculator;
		private readonly bool _useOccupancyAdjustment;
		private readonly IPersonSkillProvider _personSkillProvider;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public SchedulingResultService(ISchedulingResultStateHolder stateHolder,
			IList<ISkill> allSkills,
			ISingleSkillCalculator singleSkillCalculator,
			bool useOccupancyAdjustment,
			IPersonSkillProvider personSkillProvider)
		{
			_useOccupancyAdjustment = useOccupancyAdjustment;
			_relevantSkillStaffPeriods = stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary;
			_relevantProjections = createRelevantProjectionList(stateHolder.Schedules);
			_allSkills = allSkills;
			_singleSkillCalculator = singleSkillCalculator;
			_personSkillProvider = personSkillProvider;
		}


		public SchedulingResultService(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods,
			IList<ISkill> allSkills,
			IResourceCalculationDataContainer relevantProjections,
			ISingleSkillCalculator singleSkillCalculator,
			bool useOccupancyAdjustment,
			IPersonSkillProvider personSkillProvider)
		{
			_relevantSkillStaffPeriods = relevantSkillStaffPeriods;
			_allSkills = allSkills;
			_relevantProjections = relevantProjections;
			_singleSkillCalculator = singleSkillCalculator;
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

			return SchedulingResult(intersectingPeriod.Value, new List<IScheduleDay>(), new List<IScheduleDay>());
		}

		//used from everwhere exept ETL
		public ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodToRecalculate, IList<IScheduleDay> toRemove, IList<IScheduleDay> toAdd)
		{
			var toRemoveContainer = new ResourceCalculationDataContainer(_personSkillProvider);
			var toAddContainer = new ResourceCalculationDataContainer(_personSkillProvider);

			var minResolution = 15;
			if (_allSkills.Any())
			{
				minResolution = _allSkills.Min(s => s.DefaultResolution);
			}
			foreach (var scheduleDay in toRemove)
			{
				toRemoveContainer.AddScheduleDayToContainer(scheduleDay,minResolution);
			}
			foreach (var scheduleDay in toAdd)
			{
				toAddContainer.AddScheduleDayToContainer(scheduleDay,minResolution);
			}

			return schedulingResult(periodToRecalculate, true, toRemoveContainer, toAddContainer);
		}

		private ISkillSkillStaffPeriodExtendedDictionary schedulingResult(DateTimePeriod periodToRecalculate, bool emptyCache, IResourceCalculationDataContainer toRemove, IResourceCalculationDataContainer toAdd)
		{
			if (_allSkills.Count == 0)
				return _relevantSkillStaffPeriods;

			var period = periodToRecalculate.ToDateOnlyPeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
			var datePeriod = new DateOnlyPeriod(period.StartDate.AddDays(-1), period.EndDate.AddDays(1));
			IAffectedPersonSkillService personSkillService = new AffectedPersonSkillService(datePeriod, _allSkills);

			var useSingleSkillCalculations = UseSingleSkillCalculations(toRemove, toAdd);

			if (useSingleSkillCalculations)
			{
				_singleSkillCalculator.Calculate(_relevantProjections, _relevantSkillStaffPeriods, toRemove, toAdd);
			}
			else
			{
				var rc = new ScheduleResourceOptimizer(_relevantProjections, _relevantSkillStaffPeriods, personSkillService, emptyCache, new ActivityDivider());
				rc.Optimize(periodToRecalculate, _useOccupancyAdjustment);
			}

			return _relevantSkillStaffPeriods;
		}

		private ResourceCalculationDataContainer createRelevantProjectionList(IScheduleDictionary scheduleDictionary)
		{
			var resources = new ResourceCalculationDataContainer(_personSkillProvider);
			int minutesSplit = _allSkills.Min(s => s.DefaultResolution);
					
			foreach (var person in scheduleDictionary.Keys)
			{
				var range = scheduleDictionary[person];
				var period = range.TotalPeriod();
				if (!period.HasValue) continue;

				var scheduleDays =
					range.ScheduledDayCollection(period.Value.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()));
				foreach (var scheduleDay in scheduleDays)
				{
					resources.AddScheduleDayToContainer(scheduleDay, minutesSplit);
				}
			}
			return resources;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool UseSingleSkillCalculations(IResourceCalculationDataContainer toRemove, IResourceCalculationDataContainer toAdd)
		{
			var useSingleSkillCalculations = toRemove.HasItems() || toAdd.HasItems();

			if (useSingleSkillCalculations)
				useSingleSkillCalculations = AllIsSingleSkill(toRemove);

			if (useSingleSkillCalculations)
				useSingleSkillCalculations = AllIsSingleSkill(toAdd);

			return useSingleSkillCalculations;
		}

		private bool AllIsSingleSkill(IResourceCalculationDataContainer visualLayerCollections)
		{
			return visualLayerCollections.AllIsSingleSkill();
		}
	}
}