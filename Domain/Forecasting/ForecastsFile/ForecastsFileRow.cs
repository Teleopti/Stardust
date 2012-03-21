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

        public override bool Equals(object obj)
        {
            var row = obj as ForecastsFileRow;
            if (row == null)
                return false;
            return row.SkillName.Equals(SkillName) && row.LocalDateTimeFrom.Equals(LocalDateTimeFrom) &&
                   row.LocalDateTimeTo.Equals(LocalDateTimeTo) && row.UtcDateTimeFrom.Equals(UtcDateTimeFrom) &&
                   row.UtcDateTimeTo.Equals(UtcDateTimeTo)
                   && row.Tasks.Equals(Tasks) && row.TaskTime.Equals(TaskTime) &&
                   row.AfterTaskTime.Equals(AfterTaskTime) && (row.Agents == Agents);
        }

        public override int GetHashCode()
        {
            return SkillName.GetHashCode() ^ LocalDateTimeFrom.GetHashCode() ^ LocalDateTimeTo.GetHashCode() ^
                   UtcDateTimeFrom.GetHashCode() ^ UtcDateTimeTo.GetHashCode() ^ Tasks.GetHashCode() ^
                   TaskTime.GetHashCode() ^ AfterTaskTime.GetHashCode() ^ (Agents == null ? 1 : Agents.GetHashCode());
        }
    }
}
