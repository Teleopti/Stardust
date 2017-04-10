using System;
using System.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.WcfHost.Service
{
    public class CreateScheduleDistributionAsyncResult : AsyncResult
    {
        public DateOnlyPeriodDto DateOnlyPeriod { get; set;}
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public PersonDto[] PersonList { get; set; }
        public string TimeZone { get; set; }
        public IPrincipal Principal { get; set; }

        public CreateScheduleDistributionAsyncResult(
            AsyncCallback callback,
            object state)
            : base(callback, state)
        {
        }
    }
}