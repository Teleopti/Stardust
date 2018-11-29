using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ISkillIntervalDataAggregator
    {
        IList<ISkillIntervalData> AggregateSkillIntervalData(IList<IList<ISkillIntervalData>> multipleSkillIntervalDataList);
    }

    public class SkillIntervalDataAggregator : ISkillIntervalDataAggregator
    {
        public IList<ISkillIntervalData> AggregateSkillIntervalData(IList<IList<ISkillIntervalData>> multipleSkillIntervalDataList)
        {
            var tempPeriodToSkillIntervalData = new Dictionary<DateTimePeriod, ISkillIntervalData>();
            if (multipleSkillIntervalDataList != null)
                foreach(var skillIntervalList in multipleSkillIntervalDataList )
                {
                    foreach(var skillIntervalData in skillIntervalList )
                    {
                        if(!tempPeriodToSkillIntervalData.ContainsKey(skillIntervalData.Period ))
                            tempPeriodToSkillIntervalData.Add(skillIntervalData.Period, AggregateTwoIntervals(skillIntervalData,null));
                        else
                            tempPeriodToSkillIntervalData[skillIntervalData.Period] = AggregateTwoIntervals(tempPeriodToSkillIntervalData[skillIntervalData.Period], skillIntervalData);
                    
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
			            new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData1.Period.EndDateTime),
			            skillIntervalData1.ForecastedDemand,
			            skillIntervalData1.CurrentDemand,
			            skillIntervalData1.CurrentHeads, skillIntervalData1.MinimumHeads, skillIntervalData1.MaximumHeads)
		            {
			            MinMaxBoostFactor = skillIntervalData1.MinMaxBoostFactor,
			            MinMaxBoostFactorForStandardDeviation = skillIntervalData1.MinMaxBoostFactorForStandardDeviation
		            };

            }

            var ret = new SkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData2.Period.EndDateTime),
                                         skillIntervalData1.ForecastedDemand + skillIntervalData2.ForecastedDemand,
                                         skillIntervalData1.CurrentDemand + skillIntervalData2.CurrentDemand,
                                         skillIntervalData1.CurrentHeads + skillIntervalData2.CurrentHeads, null, null);
	        ret.MinMaxBoostFactor = skillIntervalData1.MinMaxBoostFactor + skillIntervalData2.MinMaxBoostFactor;
	        ret.MinMaxBoostFactorForStandardDeviation = skillIntervalData1.MinMaxBoostFactorForStandardDeviation +
	                                                    skillIntervalData2.MinMaxBoostFactorForStandardDeviation;

	        return ret;
        }
    }
}