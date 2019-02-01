using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IWorkShiftSelectorForIntraInterval
	{
		//legacy - was part of IWorkShiftSelector before
		IList<IWorkShiftCalculationResultHolder> SelectAllShiftProjectionCaches(IList<ShiftProjectionCache> shiftList,
			IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> skillIntervalDataLocalDictionary,
			PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo);
	}

	public interface IWorkShiftSelector
	{
		ShiftProjectionCache SelectShiftProjectionCache(IGroupPersonSkillAggregator groupPersonSkillAggregator, DateOnly datePointer, IList<ShiftProjectionCache> shifts,
			IEnumerable<ISkillDay> allSkillDays, ITeamBlockInfo teamBlockInfo,
			SchedulingOptions schedulingOptions, TimeZoneInfo timeZoneInfo, bool forRoleModel, IPerson person);
	}

	public class WorkShiftSelector : IWorkShiftSelector, IWorkShiftSelectorForIntraInterval
	{
		private readonly WorkShiftValueCalculator _workShiftValueCalculator;
		private readonly IEqualWorkShiftValueDecider _equalWorkShiftValueDecider;
		private readonly ActivityIntervalDataCreator _activityIntervalDataCreator;

		public WorkShiftSelector(WorkShiftValueCalculator workShiftValueCalculator,
												IEqualWorkShiftValueDecider equalWorkShiftValueDecider,
												ActivityIntervalDataCreator activityIntervalDataCreator)
		{
			_workShiftValueCalculator = workShiftValueCalculator;
			_equalWorkShiftValueDecider = equalWorkShiftValueDecider;
			_activityIntervalDataCreator = activityIntervalDataCreator;
		}


		public ShiftProjectionCache SelectShiftProjectionCache(IGroupPersonSkillAggregator groupPersonSkillAggregator, DateOnly datePointer, IList<ShiftProjectionCache> shifts, IEnumerable<ISkillDay> allSkillDays,
			 ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions, TimeZoneInfo timeZoneInfo, bool forRoleModel, IPerson person)
		{
			var activityInternalData = _activityIntervalDataCreator.CreateForAgent(groupPersonSkillAggregator, teamBlockInfo, datePointer, allSkillDays, forRoleModel, person.PermissionInformation.DefaultTimeZone());
			var parameters = new PeriodValueCalculationParameters(schedulingOptions.WorkShiftLengthHintOption, schedulingOptions.UseMinimumStaffing, schedulingOptions.UseMaximumStaffing);

			double? bestShiftValue = null;
			ShiftProjectionCache bestShift = null;

			var shiftsWithValue =
				shifts
					.Select(s => new { s, value = valueForShift(activityInternalData, s, parameters, person.PermissionInformation.DefaultTimeZone()) })
					.Where(s => s.value.HasValue);

			if (schedulingOptions.SkipNegativeShiftValues)
			{
				shiftsWithValue = shiftsWithValue.Where(x => x.value > 0);
			}

			foreach (var item in shiftsWithValue)
			{
				if (!bestShiftValue.HasValue)
				{
					bestShiftValue = item.value.Value;
					bestShift = item.s;
				}
				else
				{
					if (item.value.Value == bestShiftValue)
					{
						bestShiftValue = item.value.Value;
						bestShift = _equalWorkShiftValueDecider.Decide(bestShift, item.s);
					}

					if (item.value.Value > bestShiftValue)
					{
						bestShiftValue = item.value.Value;
						bestShift = item.s;
					}
				}
			}

			return bestShift;
		}

		public IList<IWorkShiftCalculationResultHolder> SelectAllShiftProjectionCaches(IList<ShiftProjectionCache> shiftList,
			IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> skillIntervalDataLocalDictionary,
			PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo)
		{
			IComparer<IWorkShiftCalculationResultHolder> comparer = new WorkShiftCalculationResultComparer();
			var sortedList =
				shiftList.AsParallel()
					.Select(s => new { s, value = valueForShift(skillIntervalDataLocalDictionary, s, parameters, timeZoneInfo) })
					.Where(i => i.value.HasValue)
					.Select(
						shiftWithValue =>
							(IWorkShiftCalculationResultHolder)new WorkShiftCalculationResult { ShiftProjection = shiftWithValue.s, Value = shiftWithValue.value.Value }).ToList();

			sortedList.Sort(comparer);
			return sortedList;
		}

		private double? valueForShift(IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> skillIntervalDataLocalDictionary, ShiftProjectionCache shiftProjectionCache, PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo)
		{
			var activityValueSum = 0d;
			var mainShiftProjection = shiftProjectionCache.MainShiftProjection();
			foreach (var skillInterval in skillIntervalDataLocalDictionary)
			{
				var activityValue =_workShiftValueCalculator.CalculateShiftValue(mainShiftProjection,
					skillInterval.Key, skillInterval.Value, parameters, timeZoneInfo);
				if (!activityValue.HasValue)
					return null;
				activityValueSum += activityValue.Value;
			}
			return activityValueSum;
		}
	}
}