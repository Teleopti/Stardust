using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{

    public interface ISkillIntervalDataAggregator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
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
                            tempPeriodToSkillIntervalData.Add(skillIntervalData.Period, aggregateTwoIntervals(skillIntervalData,null));
                        else
                            tempPeriodToSkillIntervalData[skillIntervalData.Period] = aggregateTwoIntervals(tempPeriodToSkillIntervalData[skillIntervalData.Period], skillIntervalData);
                    
                    }
                
                }

            return tempPeriodToSkillIntervalData.Values.OrderBy(x => x.Period.StartDateTime).ToList();
        }

        private static ISkillIntervalData aggregateTwoIntervals(ISkillIntervalData skillIntervalData1, ISkillIntervalData skillIntervalData2)
        {
            if (skillIntervalData2 == null)
            {


                return new SkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData1.Period.EndDateTime),
                                             skillIntervalData1.ForecastedDemand,
                                             skillIntervalData1.CurrentDemand,
                                             skillIntervalData1.CurrentHeads, skillIntervalData1.MinimumHeads , skillIntervalData1.MaximumHeads );
            }
            double? minHead1 = skillIntervalData1.MinimumHeads.HasValue ? skillIntervalData1.MinimumHeads.Value : (double?)null;
            double? maxHead1 = skillIntervalData1.MaximumHeads.HasValue ? skillIntervalData1.MaximumHeads.Value : (double?)null;
            double? minHead2 = skillIntervalData2.MinimumHeads.HasValue ? skillIntervalData2.MinimumHeads.Value : (double?)null;
            double? maxHead2 = skillIntervalData2.MaximumHeads.HasValue ? skillIntervalData2.MaximumHeads.Value : (double?)null;

            double? aggMin = null;
            if (minHead1.HasValue)
                aggMin = minHead1;
            if (minHead2.HasValue)
                aggMin += minHead2;

            double? aggMax = null;
            if (maxHead1.HasValue)
                aggMax = maxHead1;
            if (maxHead2.HasValue)
                aggMax += maxHead2;

            return new SkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData2.Period.EndDateTime),
                                         skillIntervalData1.ForecastedDemand + skillIntervalData2.ForecastedDemand,
                                         skillIntervalData1.CurrentDemand + skillIntervalData2.CurrentDemand,
                                         skillIntervalData1.CurrentHeads + skillIntervalData2.CurrentHeads, aggMin, aggMax);
        }
    }
}