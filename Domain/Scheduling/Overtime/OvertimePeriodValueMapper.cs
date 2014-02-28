using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public class OvertimePeriodValueMapper
    {
        public IList<OvertimePeriodValue> Map(IEnumerable<IOvertimeSkillIntervalData> dataList)
        {
            var listOfOvertimePeriodValue = new List<OvertimePeriodValue>();
            foreach (var overtimeSkillIntervalData in dataList)
            {
                listOfOvertimePeriodValue.Add(new OvertimePeriodValue(overtimeSkillIntervalData.Period,
                                           overtimeSkillIntervalData.RelativeDifference()));
            }
            return listOfOvertimePeriodValue;
        }
    }
}