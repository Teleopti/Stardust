using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimePeriodValueMapper
	{
		IList<OvertimePeriodValue> Map(IEnumerable<IOvertimeSkillIntervalData> dataList);
	}

	public class OvertimePeriodValueMapper : IOvertimePeriodValueMapper
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