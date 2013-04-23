using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal.Requests.ShiftTrade
{
    public class DateRangeSelectionEventArgs : EventArgs
    {
        public DateRangeSelectionEventArgs(DateTime start, DateTime end)
        {
            DateRange = new DateOnlyPeriodDto();
            DateRange.StartDate = new DateOnlyDto {DateTime = start.Date};
            DateRange.EndDate = new DateOnlyDto {DateTime = end.Date};
        }

        public DateOnlyPeriodDto DateRange { get; private set; }
    }
}