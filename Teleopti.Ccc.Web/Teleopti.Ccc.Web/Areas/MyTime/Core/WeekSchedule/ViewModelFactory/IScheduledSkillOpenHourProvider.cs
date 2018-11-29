using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface IScheduledSkillOpenHourProvider
	{
		TimePeriod? GetSkillOpenHourPeriod(IScheduleDay scheduleDay);
		TimePeriod? GetMergedSkillOpenHourPeriod(IList<IScheduleDay> scheduleDays);
	}
}