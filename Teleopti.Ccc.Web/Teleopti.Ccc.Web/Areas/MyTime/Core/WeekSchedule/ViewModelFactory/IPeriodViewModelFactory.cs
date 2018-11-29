using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public interface IPeriodViewModelFactory
    {
		IEnumerable<PeriodViewModel> CreatePeriodViewModelsForWeek(IEnumerable<IVisualLayer> visualLayerCollection, TimePeriod minMaxTime, DateOnly localDate, TimeZoneInfo timeZone, IPerson person);
		IEnumerable<PeriodViewModel> CreatePeriodViewModelsForDay(IEnumerable<IVisualLayer> visualLayerCollection, TimePeriod minMaxTime, DateOnly localDate, TimeZoneInfo timeZone, IPerson person, bool allowCrossNight = false);
		IEnumerable<OvertimeAvailabilityPeriodViewModel> CreateOvertimeAvailabilityPeriodViewModels(IOvertimeAvailability overtimeAvailability, IOvertimeAvailability overtimeAvailabilityYesterday, TimePeriod minMaxTime);
    }
}