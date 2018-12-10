using System;
using System.Globalization;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsRow : IForecastsRow
    {
        private const string dateTimeFormat = "yyyyMMdd HH:mm";
        private const char separator = ',';
        private static readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;

	    public string SkillName { get; set; }
        public DateTime LocalDateTimeFrom { get; set; }
        public DateTime LocalDateTimeTo { get; set; }
        public DateTime UtcDateTimeFrom { get; set; }
        public DateTime UtcDateTimeTo { get; set; }
        public double Tasks { get; set; }
        public double TaskTime { get; set; }
        public double AfterTaskTime { get; set; }
        public double? Agents { get; set; }
        public double? Shrinkage { get; set; }

        public ForecastsRow()
        {
        }

        public ForecastsRow(string forecastsRow)
        {
            InParameter.NotNull(nameof(forecastsRow),forecastsRow);
            var parts = forecastsRow.Split(separator);
            SkillName = parts[0];
            LocalDateTimeFrom = DateTime.ParseExact(parts[1], dateTimeFormat, CultureInfo);
            LocalDateTimeTo = DateTime.ParseExact(parts[2], dateTimeFormat, CultureInfo);
            UtcDateTimeFrom = new DateTime(DateTime.ParseExact(parts[3], dateTimeFormat, CultureInfo).Ticks, DateTimeKind.Utc);
            UtcDateTimeTo = new DateTime(DateTime.ParseExact(parts[4], dateTimeFormat, CultureInfo).Ticks, DateTimeKind.Utc);
            Tasks = double.Parse(parts[5], NumberStyles.AllowDecimalPoint, CultureInfo);
            TaskTime = double.Parse(parts[6], NumberStyles.AllowDecimalPoint, CultureInfo);
            AfterTaskTime = double.Parse(parts[7], NumberStyles.AllowDecimalPoint, CultureInfo);
            if (parts[8].Equals(" "))
                Agents = null;
            else Agents = double.Parse(parts[8], NumberStyles.AllowDecimalPoint, CultureInfo);
            if (parts[9].Equals(" "))
                Shrinkage = null;
            else Shrinkage = double.Parse(parts[9], NumberStyles.AllowDecimalPoint, CultureInfo);
        }

        public override String ToString()
        {
            var str = new StringBuilder();
            str.Append(SkillName);str.Append(separator);
            str.Append(LocalDateTimeFrom.ToString(dateTimeFormat, CultureInfo)); str.Append(separator);
            str.Append(LocalDateTimeTo.ToString(dateTimeFormat, CultureInfo)); str.Append(separator);
            str.Append(UtcDateTimeFrom.ToString(dateTimeFormat, CultureInfo)); str.Append(separator);
            str.Append(UtcDateTimeTo.ToString(dateTimeFormat, CultureInfo)); str.Append(separator);
            str.Append(Tasks.ToString("F", CultureInfo)); str.Append(separator);
            str.Append(TaskTime.ToString("F", CultureInfo)); str.Append(separator);
            str.Append(AfterTaskTime.ToString("F", CultureInfo)); str.Append(separator);
            str.Append(Agents.HasValue ? Agents.Value.ToString("F", CultureInfo) : " ");
            str.Append(separator);
            str.Append(Shrinkage.HasValue ? Shrinkage.Value.ToString("F", CultureInfo) : " ");
            return str.ToString();
        }
    }
}
