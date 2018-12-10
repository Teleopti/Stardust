using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IOvertimeSkillIntervalDataAggregator
    {
        IList<IOvertimeSkillIntervalData> AggregateOvertimeSkillIntervalData(IList<IList<IOvertimeSkillIntervalData>> multipleSkillIntervalDataList);
        IOvertimeSkillIntervalData AggregateTwoIntervals(IOvertimeSkillIntervalData skillAIntervalData1, IOvertimeSkillIntervalData skillAIntervalData2);
    }

    public class OvertimeSkillIntervalDataAggregator : IOvertimeSkillIntervalDataAggregator
    {
        public IList<IOvertimeSkillIntervalData> AggregateOvertimeSkillIntervalData(IList<IList<IOvertimeSkillIntervalData>> multipleSkillIntervalDataList)
        {
            var tempPeriodToSkillIntervalData = new Dictionary<DateTimePeriod, IOvertimeSkillIntervalData>();
            if (multipleSkillIntervalDataList != null)
                foreach (var skillIntervalList in multipleSkillIntervalDataList)
                {
                    foreach (var skillIntervalData in skillIntervalList)
                    {
                        if (!tempPeriodToSkillIntervalData.ContainsKey(skillIntervalData.Period))
                            tempPeriodToSkillIntervalData.Add(skillIntervalData.Period, AggregateTwoIntervals(skillIntervalData, null));
                        else
                            tempPeriodToSkillIntervalData[skillIntervalData.Period] = AggregateTwoIntervals(tempPeriodToSkillIntervalData[skillIntervalData.Period], skillIntervalData);

                    }

                }

            return tempPeriodToSkillIntervalData.Values.ToList();
        }

        public IOvertimeSkillIntervalData AggregateTwoIntervals(IOvertimeSkillIntervalData skillIntervalData1,
                                                                IOvertimeSkillIntervalData skillIntervalData2)
        {
            if (skillIntervalData2 == null)
            {


                return new OvertimeSkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData1.Period.EndDateTime),
                                                     skillIntervalData1.ForecastedDemand,
                                                     skillIntervalData1.CurrentDemand);
            }

            var ret = new OvertimeSkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData2.Period.EndDateTime),
                                                    skillIntervalData1.ForecastedDemand + skillIntervalData2.ForecastedDemand,
                                                    skillIntervalData1.CurrentDemand + skillIntervalData2.CurrentDemand);

            return ret;
        }
    }
}