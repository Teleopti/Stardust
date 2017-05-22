﻿using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface ISiteOpenHourProvider
	{
		TimePeriod? GetMergedSiteOpenHourPeriod(DateOnlyPeriod weekPeriod);
		TimePeriod? GetSiteOpenHourPeriod(DateOnly date);
	}
}