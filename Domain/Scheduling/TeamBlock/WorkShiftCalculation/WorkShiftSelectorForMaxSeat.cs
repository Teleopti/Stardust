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
			foreach (var layer in shiftProjectionCache.MainShiftProjection)
			{
				IntervalLevelMaxSeatInfo intervalLevelMaxSeatInfo;
				if (parameters.MaxSeatInfoPerInterval.TryGetValue(layer.Period.StartDateTime, out intervalLevelMaxSeatInfo))
				{
					if (intervalLevelMaxSeatInfo.IsMaxSeatReached)
						return 1000;
				}
			}
			return 1;
		}
	}
}