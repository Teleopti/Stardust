using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
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

        private ISkillIntervalData AggregateTwoIntervals(ISkillIntervalData skillIntervalData1, ISkillIntervalData skillIntervalData2)
        {
            if (skillIntervalData2 == null)
            {


                return new SkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData1.Period.EndDateTime),
                                             skillIntervalData1.ForecastedDemand,
                                             skillIntervalData1.CurrentDemand,
                                             skillIntervalData1.CurrentHeads, skillIntervalData1.MinimumHeads , skillIntervalData1.MaximumHeads );
            }
            int? minHead1 = skillIntervalData1.MinimumHeads.HasValue ? skillIntervalData1.MinimumHeads.Value : (int?)null;
            int? maxHead1 = skillIntervalData1.MaximumHeads.HasValue ? skillIntervalData1.MaximumHeads.Value : (int?)null;
            int? minHead2 = skillIntervalData2.MinimumHeads.HasValue ? skillIntervalData2.MinimumHeads.Value : (int?)null;
            int? maxHead2 = skillIntervalData2.MaximumHeads.HasValue ? skillIntervalData2.MaximumHeads.Value : (int?)null;

            int? aggMin = null;
            if (minHead1.HasValue)
                aggMin = minHead1;
            if (minHead2.HasValue)
                aggMin += minHead2;

            int? aggMax = null;
            if (maxHead1.HasValue)
                aggMax = minHead1;
            if (maxHead2.HasValue)
                aggMax += minHead2;

            return new SkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData2.Period.EndDateTime),
                                         skillIntervalData1.ForecastedDemand + skillIntervalData2.ForecastedDemand,
                                         skillIntervalData1.CurrentDemand + skillIntervalData2.CurrentDemand,
                                         skillIntervalData1.CurrentHeads + skillIntervalData2.CurrentHeads, aggMin, aggMax);
        }
    }
}