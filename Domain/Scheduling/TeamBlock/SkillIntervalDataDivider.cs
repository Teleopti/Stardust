using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ISkillIntervalDataDivider
    {
        IList<ISkillIntervalData> SplitSkillIntervalData(IList<ISkillIntervalData> skillIntervalDataList, int resolution);
    }

    public class SkillIntervalDataDivider : ISkillIntervalDataDivider
    {
        public IList<ISkillIntervalData> SplitSkillIntervalData(IList<ISkillIntervalData> skillIntervalDataList, int resolution)
        {
            var resultingskillIntervalDataList = new List<ISkillIntervalData>();
            if (skillIntervalDataList != null)
            {
                
                var modDiffInMin = getElapsedTime(skillIntervalDataList[0].Period.EndDateTime.Ticks,
                                                        skillIntervalDataList[0].Period.StartDateTime.Ticks) % resolution ;

                int totallDiffInMin = (int) (getElapsedTime(skillIntervalDataList[0].Period.EndDateTime.Ticks,
                                                                  skillIntervalDataList[0].Period.StartDateTime.Ticks) / resolution);
                if (modDiffInMin!= 0)
                {
                    //split the interval into to 30 min as the interval length is 15 and the resolution is 10
                    var sortedSkillList =
                        skillIntervalDataList.OrderBy(s => s.Period.StartDateTime).ThenBy(s => s.Period.EndDateTime).ToList() ;
                    var aggregatedList = new List<ISkillIntervalData>();
                    for (var j = 0; j < sortedSkillList.Count( ); j++)
                    {
                        if(j+1<skillIntervalDataList.Count)
                        {
                            aggregatedList.Add(aggregateTwoIntervals(sortedSkillList[j], sortedSkillList[j + 1]));
                            j++;
                        }else
                            aggregatedList.Add(aggregateTwoIntervals(sortedSkillList[j], null));
                    
                    }
                    skillIntervalDataList = aggregatedList;
                    totallDiffInMin = (int) (getElapsedTime( skillIntervalDataList[0].Period.EndDateTime.Ticks  ,
                                                                   skillIntervalDataList[0].Period.StartDateTime.Ticks ) / resolution);
                }
                foreach(var skillIntervalItem in skillIntervalDataList )
                {
                    var startPeriod = skillIntervalItem.Period.StartDateTime ;
                    for (int j = 0; j < totallDiffInMin; j++)
                    {
                        double? minHead = null;
                        double? maxHead = null;
                        if (skillIntervalItem.MinimumHeads.HasValue)
                            minHead = skillIntervalItem.MinimumHeads.Value;
                        if (skillIntervalItem.MaximumHeads.HasValue)
                            maxHead = skillIntervalItem.MaximumHeads.Value;

                        resultingskillIntervalDataList.Add(
                            new SkillIntervalData(new DateTimePeriod(startPeriod, startPeriod.AddMinutes(resolution)),
                                                  skillIntervalItem.ForecastedDemand, skillIntervalItem.CurrentDemand,
                                                  skillIntervalItem.CurrentHeads, minHead, maxHead));
                        startPeriod = startPeriod.AddMinutes(resolution);
                    }
                }
            }


            return resultingskillIntervalDataList;
        }

        private double getElapsedTime(long ticksEnd, long ticksStart)
        {
            long elapsedTicks = ticksEnd - ticksStart;
            var elapsedSpan = new TimeSpan(elapsedTicks);
            return elapsedSpan.TotalMinutes;
        }


        private static ISkillIntervalData aggregateTwoIntervals(ISkillIntervalData skillIntervalData1,ISkillIntervalData skillIntervalData2  )
        {
            if(skillIntervalData2 == null )
            {
                

                return new SkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData1.Period.EndDateTime.AddMinutes(15)),
                                             skillIntervalData1.ForecastedDemand ,
                                             skillIntervalData1.CurrentDemand ,
                                             skillIntervalData1.CurrentHeads , skillIntervalData1.MinimumHeads  , skillIntervalData1.MaximumHeads );
            }
            double? minHead1 = skillIntervalData1.MinimumHeads.HasValue ? skillIntervalData1.MinimumHeads.Value : (double?)null;
            double? maxHead1 = skillIntervalData1.MaximumHeads.HasValue ? skillIntervalData1.MaximumHeads.Value : (double?)null;
            double? minHead2 = skillIntervalData2.MinimumHeads.HasValue ? skillIntervalData2.MinimumHeads.Value : (double?)null;
            double? maxHead2 = skillIntervalData2.MaximumHeads.HasValue ? skillIntervalData2.MaximumHeads.Value : (double?)null;

            double? aggMin = null;
            if (minHead1.HasValue && minHead2.HasValue)
                aggMin = (minHead1 + minHead2)/2;
            else if (minHead1.HasValue)
                aggMin = minHead1;
            else if (minHead2.HasValue)
                aggMin = minHead2;

            double? aggMax = null;
            if (maxHead1.HasValue && maxHead2.HasValue)
                aggMax = (maxHead1 + maxHead2)/2;
            else if (maxHead1.HasValue)
                aggMax = maxHead1;
            else if (maxHead2.HasValue)
                aggMax = maxHead2;

            return new SkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData2.Period.EndDateTime),
                                         (skillIntervalData1.ForecastedDemand + skillIntervalData2.ForecastedDemand)/2,
                                         (skillIntervalData1.CurrentDemand + skillIntervalData2.CurrentDemand)/2,
                                         (skillIntervalData1.CurrentHeads + skillIntervalData2.CurrentHeads)/2, aggMin, aggMax);
        }
    }
}