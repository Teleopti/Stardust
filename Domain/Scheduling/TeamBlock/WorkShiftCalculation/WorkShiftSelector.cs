using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IWorkShiftSelector
	{
        IShiftProjectionCache SelectShiftProjectionCache(IList<IShiftProjectionCache> shiftList, IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> skillIntervalDataDictionary, PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo);

		IList<IWorkShiftCalculationResultHolder> SelectAllShiftProjectionCaches(IList<IShiftProjectionCache> shiftList,
			IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> skillIntervalDataLocalDictionary,
			PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo);
	}

	public class WorkShiftSelector : IWorkShiftSelector
	{
		private readonly IWorkShiftValueCalculator _workShiftValueCalculator;
		private readonly IEqualWorkShiftValueDecider _equalWorkShiftValueDecider;

		public WorkShiftSelector(IWorkShiftValueCalculator workShiftValueCalculator, IEqualWorkShiftValueDecider equalWorkShiftValueDecider)
		{
			_workShiftValueCalculator = workShiftValueCalculator;
			_equalWorkShiftValueDecider = equalWorkShiftValueDecider;
		}

		public IShiftProjectionCache SelectShiftProjectionCache(IList<IShiftProjectionCache> shiftList,
			IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> skillIntervalDataLocalDictionary,
			PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo)
		{
			double? bestShiftValue = null;
			IShiftProjectionCache bestShift = null;

			var shiftsWithValue =
				shiftList.AsParallel()
					.Select(s => new {s, value = valueForShift(skillIntervalDataLocalDictionary, s, parameters, timeZoneInfo)});

			foreach (var item in shiftsWithValue)
			{
				if (!item.value.HasValue) continue;

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

		public IList<IWorkShiftCalculationResultHolder> SelectAllShiftProjectionCaches(IList<IShiftProjectionCache> shiftList,
			IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> skillIntervalDataLocalDictionary,
			PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo)
		{
			IComparer<IWorkShiftCalculationResultHolder> comparer = new WorkShiftCalculationResultComparer();
			var sortedList =
				shiftList.AsParallel()
					.Select(s => new {s, value = valueForShift(skillIntervalDataLocalDictionary, s, parameters, timeZoneInfo)})
					.Where(i => i.value.HasValue)
					.Select(
						shiftWithValue =>
							(IWorkShiftCalculationResultHolder)new WorkShiftCalculationResult { ShiftProjection = shiftWithValue.s, Value = shiftWithValue.value.Value }).ToList();
			
			sortedList.Sort(comparer);
			return sortedList;
		}

		private double? valueForActivity(IActivity activity, IDictionary<DateTime, ISkillIntervalData> skillIntervalDataDic, IShiftProjectionCache shiftProjectionCache, PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo)
		{
			double? value = _workShiftValueCalculator.CalculateShiftValue(shiftProjectionCache.MainShiftProjection,
																		  activity, skillIntervalDataDic, parameters , timeZoneInfo);
			return value;
		}

		private double? valueForShift(IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> skillIntervalDataLocalDictionary, IShiftProjectionCache shiftProjectionCache, PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo)
		{
			double? totalForAllActivitesValue = null;
			var actvityValue =
				skillIntervalDataLocalDictionary.AsParallel()
					.Select(i => valueForActivity(i.Key, i.Value, shiftProjectionCache, parameters, timeZoneInfo));
			foreach (var value in actvityValue)
			{
				if (!value.HasValue) return null;  
				
				if (totalForAllActivitesValue.HasValue)
				{
					totalForAllActivitesValue = totalForAllActivitesValue + value.Value;
				}
				else
				{
					totalForAllActivitesValue = value.Value;
				}
			}

			return totalForAllActivitesValue;
		}
	}
}