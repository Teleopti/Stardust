﻿using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface IScheduleMinMaxTimeSiteOpenHourCalculator
	{
		void AdjustScheduleMinMaxTime(WeekScheduleDomainData weekDomainData);

		void AdjustScheduleMinMaxTime(DayScheduleDomainData dayDomainData);
	}
}