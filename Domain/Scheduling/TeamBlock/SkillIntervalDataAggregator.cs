using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public class SkillIntervalDataAggregator
    {
        public IList<ISkillIntervalData> AggregateSkillIntervalData(IList<IList<ISkillIntervalData>> multipleSkillIntervalDataList)
		{
			if (multipleSkillIntervalDataList == null) return new ISkillIntervalData[0];
			var tempPeriodToSkillIntervalData = new Dictionary<DateTimePeriod, ISkillIntervalData>();

			foreach(var skillIntervalList in multipleSkillIntervalDataList )
			{
				foreach(var skillIntervalData in skillIntervalList )
				{
					if(!tempPeriodToSkillIntervalData.TryGetValue(skillIntervalData.Period, out var intervalData))
						tempPeriodToSkillIntervalData.Add(skillIntervalData.Period, AggregateTwoIntervals(skillIntervalData,null));
					else
						tempPeriodToSkillIntervalData[skillIntervalData.Period] = AggregateTwoIntervals(intervalData, skillIntervalData);
				}
			}

			return tempPeriodToSkillIntervalData.Values.ToList();
        }

        public ISkillIntervalData AggregateTwoIntervals(ISkillIntervalData skillIntervalData1, ISkillIntervalData skillIntervalData2)
        {
            if (skillIntervalData2 == null)
            {
	            return
		            new SkillIntervalData(
						skillIntervalData1.Period,
			            skillIntervalData1.ForecastedDemand,
			            skillIntervalData1.CurrentDemand,
			            skillIntervalData1.CurrentHeads, skillIntervalData1.MinimumHeads, skillIntervalData1.MaximumHeads)
		            {
			            MinMaxBoostFactor = skillIntervalData1.MinMaxBoostFactor,
			            MinMaxBoostFactorForStandardDeviation = skillIntervalData1.MinMaxBoostFactorForStandardDeviation
		            };

            }

			var ret = new SkillIntervalData(skillIntervalData1.Period,
				skillIntervalData1.ForecastedDemand + skillIntervalData2.ForecastedDemand,
				skillIntervalData1.CurrentDemand + skillIntervalData2.CurrentDemand,
				skillIntervalData1.CurrentHeads + skillIntervalData2.CurrentHeads, null, null)
			{
				MinMaxBoostFactor = skillIntervalData1.MinMaxBoostFactor + skillIntervalData2.MinMaxBoostFactor,
				MinMaxBoostFactorForStandardDeviation = skillIntervalData1.MinMaxBoostFactorForStandardDeviation +
														skillIntervalData2.MinMaxBoostFactorForStandardDeviation
			};

			return ret;
        }
    }
}