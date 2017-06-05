using System.Collections.Generic;
using System.Linq;

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
	        return dataList.Select(overtimeSkillIntervalData =>
									   new OvertimePeriodValue(overtimeSkillIntervalData.Period, -overtimeSkillIntervalData.CurrentDemand / overtimeSkillIntervalData.ForecastedDemand)).ToList();
        }
    }
}