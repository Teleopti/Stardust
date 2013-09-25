using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public interface IPeriodViewModelFactory
    {
		IEnumerable<PeriodViewModel> CreatePeriodViewModels(IEnumerable<IVisualLayer> visualLayerCollection, TimePeriod minMaxTime, DateTime localDate, TimeZoneInfo timeZone);
	    IEnumerable<OvertimeAvailabilityPeriodViewModel> CreateOvertimeAvailabilityPeriodViewModels(IOvertimeAvailability overtimeAvailability, IOvertimeAvailability overtimeAvailabilityYesterday, TimePeriod minMaxTime);
    }
}