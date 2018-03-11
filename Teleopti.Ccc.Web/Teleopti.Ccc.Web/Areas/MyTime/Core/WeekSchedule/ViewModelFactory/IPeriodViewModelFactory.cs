using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public interface IPeriodViewModelFactory
    {
		IEnumerable<PeriodViewModel> CreatePeriodViewModelsForWeek(IEnumerable<IVisualLayer> visualLayerCollection, TimePeriod minMaxTime, DateOnly localDate, TimeZoneInfo timeZone);
		IEnumerable<PeriodViewModel> CreatePeriodViewModelsForDay(IEnumerable<IVisualLayer> visualLayerCollection, TimePeriod minMaxTime, DateOnly localDate, TimeZoneInfo timeZone);
		IEnumerable<OvertimeAvailabilityPeriodViewModel> CreateOvertimeAvailabilityPeriodViewModels(IOvertimeAvailability overtimeAvailability, IOvertimeAvailability overtimeAvailabilityYesterday, TimePeriod minMaxTime);
    }
}