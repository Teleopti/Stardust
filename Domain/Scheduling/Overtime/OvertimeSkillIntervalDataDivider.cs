using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IOvertimeSkillIntervalDataDivider
    {
        IList<IOvertimeSkillIntervalData> SplitSkillIntervalData(IList<IOvertimeSkillIntervalData> skillIntervalDataList, int resolution);
    }
    public class OvertimeSkillIntervalDataDivider : IOvertimeSkillIntervalDataDivider
    {
        public IList<IOvertimeSkillIntervalData> SplitSkillIntervalData(IList<IOvertimeSkillIntervalData> skillIntervalDataList, int resolution)
        {
            var resultingskillIntervalDataList = new List<IOvertimeSkillIntervalData>();
            if (skillIntervalDataList != null && skillIntervalDataList.Count > 0)
            {
                var modDiffInMin = (skillIntervalDataList[0].Period.EndDateTime.Minute -
                                    skillIntervalDataList[0].Period.StartDateTime.Minute) % resolution;

                int totallDiffInMin = (skillIntervalDataList[0].Period.EndDateTime.Minute -
                                       skillIntervalDataList[0].Period.StartDateTime.Minute) / resolution;
                if (modDiffInMin != 0)
                {
                    //split the interval into to 30 min as the interval length is 15 and the resolution is 10
                    var sortedSkillList =
                        skillIntervalDataList.OrderBy(s => s.Period.StartDateTime).ThenBy(s => s.Period.EndDateTime).ToList();
                    var aggregatedList = new List<IOvertimeSkillIntervalData>();
                    for (var j = 0; j < sortedSkillList.Count(); j++)
                    {
                        if (j + 1 < skillIntervalDataList.Count)
                        {
                            aggregatedList.Add(aggregateTwoIntervals(sortedSkillList[j], sortedSkillList[j + 1]));
                            j++;
                        }
                        else
                            aggregatedList.Add(aggregateTwoIntervals(sortedSkillList[j], null));

                    }
                    skillIntervalDataList = aggregatedList;
                    totallDiffInMin = (skillIntervalDataList[0].Period.EndDateTime.Minute -
                                       skillIntervalDataList[0].Period.StartDateTime.Minute) / resolution;
                }
                foreach (var skillIntervalItem in skillIntervalDataList)
                {
                    var startPeriod = skillIntervalItem.Period.StartDateTime;
                    for (int j = 0; j < totallDiffInMin; j++)
                    {
                        resultingskillIntervalDataList.Add(
                            new OvertimeSkillIntervalData(new DateTimePeriod(startPeriod, startPeriod.AddMinutes(resolution)),
                                                  skillIntervalItem.ForecastedDemand, skillIntervalItem.CurrentDemand));
                        startPeriod = startPeriod.AddMinutes(resolution);
                    }
                }
            }


            return resultingskillIntervalDataList;
        }

        private static IOvertimeSkillIntervalData aggregateTwoIntervals(IOvertimeSkillIntervalData overtimeSkillIntervalData1, IOvertimeSkillIntervalData overtimeSkillIntervalData2)
        {
            if (overtimeSkillIntervalData2 == null)
            {
                return new OvertimeSkillIntervalData(new DateTimePeriod(overtimeSkillIntervalData1.Period.StartDateTime, overtimeSkillIntervalData1.Period.EndDateTime.AddMinutes(15)),
                                             overtimeSkillIntervalData1.ForecastedDemand, overtimeSkillIntervalData1.CurrentDemand);
            }

            return new OvertimeSkillIntervalData(new DateTimePeriod(overtimeSkillIntervalData1.Period.StartDateTime, overtimeSkillIntervalData2.Period.EndDateTime),
                                         (overtimeSkillIntervalData1.ForecastedDemand + overtimeSkillIntervalData2.ForecastedDemand) / 2, (overtimeSkillIntervalData1.CurrentDemand + overtimeSkillIntervalData2.CurrentDemand) / 2);
        }
    }
}