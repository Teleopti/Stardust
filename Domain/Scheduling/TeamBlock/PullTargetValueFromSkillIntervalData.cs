using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class PullTargetValueFromSkillIntervalData
	{
		public double GetTargetValue(IDictionary<DateTime, ISkillIntervalData> skillIntervalDataDic, TargetValueOptions targetValueCalculation)
		{
			if (targetValueCalculation == TargetValueOptions.RootMeanSquare)
			{
				var aggregatedValues = skillIntervalDataDic.Select(interval => interval.Value.AbsoluteDifference);
				return Calculation.Variances.RMS(aggregatedValues);
			}
			else
			{
				var aggregatedValues = skillIntervalDataDic.Select(interval => interval.Value.RelativeDifferenceBoosted());
				return targetValueCalculation == TargetValueOptions.StandardDeviation ? 
					Calculation.Variances.StandardDeviation(aggregatedValues) : 
					Calculation.Variances.Teleopti(aggregatedValues);
			}
		}
	}
}