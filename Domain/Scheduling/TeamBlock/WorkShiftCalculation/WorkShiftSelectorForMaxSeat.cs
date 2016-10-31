using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public class WorkShiftSelectorForMaxSeat : WorkShiftSelector
	{
		public WorkShiftSelectorForMaxSeat(IWorkShiftValueCalculator workShiftValueCalculator, IEqualWorkShiftValueDecider equalWorkShiftValueDecider) : base(workShiftValueCalculator, equalWorkShiftValueDecider)
		{
		}

		protected override double? ValueForShift(IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> skillIntervalDataLocalDictionary, IShiftProjectionCache shiftProjectionCache,
			PeriodValueCalculationParameters parameters, TimeZoneInfo timeZoneInfo)
		{
			var intervalLength = 1440/parameters.MaxSeatInfoPerInterval.Count;
			foreach (var layer in shiftProjectionCache.MainShiftProjection)
			{
				foreach (var dateTimePeriod in layer.Period.Intervals(TimeSpan.FromMinutes(intervalLength)))
				{
					IntervalLevelMaxSeatInfo intervalLevelMaxSeatInfo;
					if (parameters.MaxSeatInfoPerInterval.TryGetValue(dateTimePeriod.StartDateTime, out intervalLevelMaxSeatInfo))
					{
						if (intervalLevelMaxSeatInfo.IsMaxSeatReached)
							return null;
					}
				}	
			}
			
			return 1;
		}
	}
}