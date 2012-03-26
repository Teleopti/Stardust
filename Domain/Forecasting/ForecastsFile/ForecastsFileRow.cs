using System;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsFileRow : IForecastsFileRow
    {
        public string SkillName { get; set; }
        public DateTime LocalDateTimeFrom { get; set; }
        public DateTime LocalDateTimeTo { get; set; }
        public DateTime UtcDateTimeFrom { get; set; }
        public DateTime UtcDateTimeTo { get; set; }
        public int Tasks { get; set; }
        public double TaskTime { get; set; }
        public double AfterTaskTime { get; set; }
        public double? Agents { get; set; }
    }
}
