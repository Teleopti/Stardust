using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	[CLSCompliant(false)]
    public interface ISkillIntervalDataAggregator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IList<ISkillIntervalData> AggregateSkillIntervalData(IList<IList<ISkillIntervalData>> multipleSkillIntervalDataList);

		[CLSCompliant(false)]
	    ISkillIntervalData AggregateTwoIntervals(ISkillIntervalData skillIntervalData1,
	                                             ISkillIntervalData skillIntervalData2);
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

		[CLSCompliant(false)]
        public ISkillIntervalData AggregateTwoIntervals(ISkillIntervalData skillIntervalData1, ISkillIntervalData skillIntervalData2)
        {
            if (skillIntervalData2 == null)
            {


                return new SkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData1.Period.EndDateTime),
                                             skillIntervalData1.ForecastedDemand,
                                             skillIntervalData1.CurrentDemand,
                                             skillIntervalData1.CurrentHeads, skillIntervalData1.MinimumHeads , skillIntervalData1.MaximumHeads );
            }

            var ret = new SkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData2.Period.EndDateTime),
                                         skillIntervalData1.ForecastedDemand + skillIntervalData2.ForecastedDemand,
                                         skillIntervalData1.CurrentDemand + skillIntervalData2.CurrentDemand,
                                         skillIntervalData1.CurrentHeads + skillIntervalData2.CurrentHeads, null, null);
	        ret.MinMaxBoostFactor = skillIntervalData1.MinMaxBoostFactor + skillIntervalData2.MinMaxBoostFactor;

	        return ret;
        }
    }
}