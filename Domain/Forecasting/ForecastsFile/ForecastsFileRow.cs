using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    [Serializable]
    public class ForecastsFileRow : IForecastsFileRow
    {
        public string SkillName { get; set; }
        public DateTime LocalDateTimeFrom { get; set; }
        public DateTime LocalDateTimeTo { get; set; }
        public DateTime UtcDateTimeFrom { get; set; }
        public DateTime UtcDateTimeTo { get; set; }
        public DateTimePeriod Period { get; set; }
        public int Tasks { get; set; }
        public int TaskTime { get; set; }
        public int AfterTaskTime { get; set; }
        public double? Agents { get; set; }

        public ForecastsFileRow(IList<string> row, ICccTimeZoneInfo timeZone)
        {
            SkillName = row[0];
            LocalDateTimeFrom = DateTime.ParseExact(row[1], "yyyyMMdd HH:mm", null);
            LocalDateTimeTo = DateTime.ParseExact(row[2], "yyyyMMdd HH:mm", null);
            UtcDateTimeFrom = TimeZoneHelper.ConvertToUtc(LocalDateTimeFrom, timeZone);
            UtcDateTimeTo = TimeZoneHelper.ConvertToUtc(LocalDateTimeTo, timeZone);
            Period = new DateTimePeriod(UtcDateTimeFrom, UtcDateTimeTo);
            Tasks = int.Parse(row[3]);
            TaskTime = int.Parse(row[4]);
            AfterTaskTime = int.Parse(row[5]);

            if (row.Count > 6)
                Agents = double.Parse(row[6]);
        }

        public override bool Equals(object obj)
        {
            var row = obj as ForecastsFileRow;
            if (row == null)
                return false;
            return row.SkillName.Equals(SkillName) && row.LocalDateTimeFrom.Equals(LocalDateTimeFrom) &&
                   row.LocalDateTimeTo.Equals(LocalDateTimeTo) && row.UtcDateTimeFrom.Equals(row.UtcDateTimeTo) &&
                   row.Period.Equals(Period)
                   && row.Tasks.Equals(Tasks) && row.TaskTime.Equals(TaskTime) &&
                   row.AfterTaskTime.Equals(AfterTaskTime) && (row.Agents == Agents);
        }

        public override int GetHashCode()
        {
            return SkillName.GetHashCode() ^ LocalDateTimeFrom.GetHashCode() ^ LocalDateTimeTo.GetHashCode() ^
                   UtcDateTimeFrom.GetHashCode() ^ UtcDateTimeTo.GetHashCode() ^ Tasks.GetHashCode() ^
                   Period.GetHashCode() ^
                   TaskTime.GetHashCode() ^ AfterTaskTime.GetHashCode() ^ (Agents == null ? 1 : Agents.GetHashCode());
        }
    }
}
