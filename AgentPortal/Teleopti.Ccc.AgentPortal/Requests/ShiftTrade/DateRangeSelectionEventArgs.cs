using System;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.Requests.ShiftTrade
{
    public class DateRangeSelectionEventArgs : EventArgs
    {
        public DateRangeSelectionEventArgs(DateTime start, DateTime end)
        {
            DateRange = new DateOnlyPeriodDto();
            DateRange.StartDate = new DateOnlyDto {DateTime = start.Date, DateTimeSpecified = true};
            DateRange.EndDate = new DateOnlyDto {DateTime = end.Date, DateTimeSpecified = true};
        }

        public DateOnlyPeriodDto DateRange { get; private set; }
    }
}