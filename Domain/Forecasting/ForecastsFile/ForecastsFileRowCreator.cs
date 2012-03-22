using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsFileRowCreator
    {
        public static ForecastsFileRow Create(IFileRow row, ICccTimeZoneInfo timeZone)
        {
            var forecastRow = new ForecastsFileRow
                                  {
                                      SkillName = row.Content[0],
                                      LocalDateTimeFrom = DateTime.ParseExact(row.Content[1], "yyyyMMdd HH:mm", null),
                                      LocalDateTimeTo = DateTime.ParseExact(row.Content[2], "yyyyMMdd HH:mm", null),
                                      Tasks = int.Parse(row.Content[3]),
                                      TaskTime = double.Parse(row.Content[4], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                                      AfterTaskTime = double.Parse(row.Content[5], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)
                                  };
            forecastRow.UtcDateTimeFrom = TimeZoneHelper.ConvertToUtc(forecastRow.LocalDateTimeFrom, timeZone);
            forecastRow.UtcDateTimeTo = TimeZoneHelper.ConvertToUtc(forecastRow.LocalDateTimeTo, timeZone);

            if (row.Count > FileColumnsWithoutAgent)
                forecastRow.Agents = double.Parse(row.Content[FileColumnsWithoutAgent], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            return forecastRow;
        }

        public static int FileColumnsWithoutAgent { get { return 6; } }
        public static int FileColumnsWithAgent { get { return 7; } }

        public static bool IsFileColumnValid(IFileRow row)
        {
            return row.Count == FileColumnsWithoutAgent || row.Count == FileColumnsWithAgent;
        }
    }
}