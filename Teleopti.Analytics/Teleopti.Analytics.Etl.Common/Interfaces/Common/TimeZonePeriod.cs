

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
    public class TimeZonePeriod
    {
        public string TimeZoneCode { get; set; }
        public DateTimePeriod PeriodToLoad { get; set; }
    }
}
