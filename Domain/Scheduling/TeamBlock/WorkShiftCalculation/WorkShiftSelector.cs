using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IWorkShiftSelector
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IShiftProjectionCache SelectShiftProjectionCache(IList<IShiftProjectionCache> shiftList, IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> skillIntervalDataDictionary, PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo);
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
			if (shiftList != null)
				foreach (var shiftProjectionCache in shiftList)
				{
					double? valueForShift = this.valueForShift(skillIntervalDataLocalDictionary, shiftProjectionCache, parameters, timeZoneInfo);

					if (valueForShift.HasValue)
					{
						if (!bestShiftValue.HasValue)
						{
							bestShiftValue = valueForShift.Value;
							bestShift = shiftProjectionCache;

						}
						else
						{
							if (valueForShift.Value == bestShiftValue)
							{
								bestShiftValue = valueForShift.Value;
								bestShift = _equalWorkShiftValueDecider.Decide(bestShift, shiftProjectionCache);
							}

							if (valueForShift.Value > bestShiftValue)
							{
								bestShiftValue = valueForShift.Value;
								bestShift = shiftProjectionCache;
							}

						}
					}
				}

			return bestShift;
		}

		private double? valueForActivity(IActivity activity, IDictionary<DateTime, ISkillIntervalData> skillIntervalDataDic, IShiftProjectionCache shiftProjectionCache, PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo)
		{
			double? value = _workShiftValueCalculator.CalculateShiftValue(shiftProjectionCache.MainShiftProjection,
																		  activity, skillIntervalDataDic, parameters , timeZoneInfo, new Dictionary<DateTime, bool>());
			return value;
		}

		private double? valueForShift(IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> skillIntervalDataLocalDictionary, IShiftProjectionCache shiftProjectionCache, PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo)
		{
			double? totalForAllActivitesValue = null;
			foreach (var activity in skillIntervalDataLocalDictionary.Keys)
			{
				double? skillValue = valueForActivity(activity, skillIntervalDataLocalDictionary[activity], shiftProjectionCache, parameters, timeZoneInfo);
				if (!skillValue.HasValue) return null;
				
				if (totalForAllActivitesValue.HasValue)
				{
					totalForAllActivitesValue = totalForAllActivitesValue + skillValue.Value;
				}
				else
				{
					totalForAllActivitesValue = skillValue.Value;
				}

			}

			return totalForAllActivitesValue;
		}
	}
}