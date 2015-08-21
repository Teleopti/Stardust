using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class PullTargetValueFromSkillIntervalData
	{
		private const int punishingNumber = 1000;

		public double GetTargetValue(IDictionary<DateTime, ISkillIntervalData> skillIntervalDataDic,
			TargetValueOptions targetValueCalculation, IDictionary<DateTime, IntervalLevelMaxSeatInfo> aggregatedMaxSeatIntervals)
		{
			if (targetValueCalculation == TargetValueOptions.RootMeanSquare)
			{
				var aggregatedValues = new List<Double>();
				foreach (var interval in skillIntervalDataDic)
				{
					IntervalLevelMaxSeatInfo value;
					if (aggregatedMaxSeatIntervals.TryGetValue(interval.Key, out value))
					{
						if (value.IsMaxSeatReached)
						{
							aggregatedValues.Add(interval.Value.AbsoluteDifference +
							                     (punishingNumber*value.MaxSeatBoostingFactor));
							continue;
						}
					}
					aggregatedValues.Add(interval.Value.AbsoluteDifference);
				}
				return Calculation.Variances.RMS(aggregatedValues);
			}
			else
			{
				var aggregatedValues = new List<Double>();
				foreach (var interval in skillIntervalDataDic)
				{
					IntervalLevelMaxSeatInfo value;
					if (aggregatedMaxSeatIntervals.TryGetValue(interval.Key, out value))
					{
						if (value.IsMaxSeatReached)
						{
							aggregatedValues.Add(interval.Value.RelativeDifferenceBoosted() +
							                     (punishingNumber*value.MaxSeatBoostingFactor));
							continue;
						}
					}
					aggregatedValues.Add(interval.Value.RelativeDifferenceBoosted());
				}
				if (targetValueCalculation == TargetValueOptions.StandardDeviation)
					return Calculation.Variances.StandardDeviation(aggregatedValues);
				return Calculation.Variances.Teleopti(aggregatedValues);
			}

		}


	}
}