using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
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
            var modDiffInMin = (skillIntervalDataList[0].Period.EndDateTime.Minute -
                                skillIntervalDataList[0].Period.StartDateTime.Minute) % resolution;

            int totallDiffInMin = (skillIntervalDataList[0].Period.EndDateTime.Minute -
                                   skillIntervalDataList[0].Period.StartDateTime.Minute)/resolution;
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
                        aggregatedList.Add(AggregateTwoIntervals(sortedSkillList[j], sortedSkillList[j + 1]));
                        j++;
                    }else
                        aggregatedList.Add(AggregateTwoIntervals(sortedSkillList[j], null));
                    
                }
                skillIntervalDataList = aggregatedList;
                totallDiffInMin = (skillIntervalDataList[0].Period.EndDateTime.Minute -
                                   skillIntervalDataList[0].Period.StartDateTime.Minute) / resolution;
            }
            foreach(var skillIntervalItem in skillIntervalDataList )
            {
                var startPeriod = skillIntervalItem.Period.StartDateTime ;
                for (int j = 0; j < totallDiffInMin; j++)
                {
                    int? minHead = null;
                    int? maxHead = null;
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


            return resultingskillIntervalDataList;
        }

        private ISkillIntervalData AggregateTwoIntervals(ISkillIntervalData skillIntervalData1,ISkillIntervalData skillIntervalData2  )
        {
            if(skillIntervalData2 == null )
            {
                

                return new SkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData1.Period.EndDateTime.AddMinutes(15)),
                                             skillIntervalData1.ForecastedDemand ,
                                             skillIntervalData1.CurrentDemand ,
                                             skillIntervalData1.CurrentHeads , null , null );
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