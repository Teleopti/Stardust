using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
    public class TimeZonePeriod
    {
        public string TimeZoneCode { get; set; }
        public DateTimePeriod PeriodToLoad { get; set; }
    }
}
