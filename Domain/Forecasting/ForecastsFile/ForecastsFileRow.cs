using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;
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
        public int TaskTime { get; set; }
        public int AfterTaskTime { get; set; }
        public double? Agents { get; set; }

        public override bool Equals(object obj)
        {
            var row = obj as ForecastsFileRow;
            if (row == null)
                return false;
            return row.SkillName.Equals(SkillName) && row.LocalDateTimeFrom.Equals(LocalDateTimeFrom) &&
                   row.LocalDateTimeTo.Equals(LocalDateTimeTo) && row.UtcDateTimeFrom.Equals(row.UtcDateTimeTo)
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

    public class ForecastsFileRowCreator
    {
        public static ForecastsFileRow Create(IList<string> row, ICccTimeZoneInfo timeZone)
        {
            var forecastRow = new ForecastsFileRow
                                  {
                                      SkillName = row[0],
                                      LocalDateTimeFrom = DateTime.ParseExact(row[1], "yyyyMMdd HH:mm", null),
                                      LocalDateTimeTo = DateTime.ParseExact(row[2], "yyyyMMdd HH:mm", null),
                                      Tasks = int.Parse(row[3]),
                                      TaskTime = int.Parse(row[4]),
                                      AfterTaskTime = int.Parse(row[5]),
                                  };
            forecastRow.UtcDateTimeFrom = TimeZoneHelper.ConvertToUtc(forecastRow.LocalDateTimeFrom, timeZone);
            forecastRow.UtcDateTimeTo = TimeZoneHelper.ConvertToUtc(forecastRow.LocalDateTimeTo, timeZone);

            if (row.Count > 6)
                forecastRow.Agents = double.Parse(row[6], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            return forecastRow;
        }
    }
}
