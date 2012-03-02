using System;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsFileRow
    {
        public string SkillName { get; set; }
        public DateTime DateTimeFrom { get; set; }
        public DateTime DateTimeTo { get; set; }
        public int Tasks { get; set; }
        public int TaskTime { get; set; }
        public int AfterTaskTime { get; set; }
        public double? Agents { get; set; }

        public static ForecastsFileRow CreateFrom(IFileRow row)
        {
            var forecastRow = new ForecastsFileRow
                                  {
                                      SkillName = row[0],
                                      DateTimeFrom = DateTime.ParseExact(row[1], "yyyyMMdd HH:mm", null),
                                      DateTimeTo = DateTime.ParseExact(row[2], "yyyyMMdd HH:mm", null),
                                      Tasks = int.Parse(row[3]),
                                      TaskTime = int.Parse(row[4]),
                                      AfterTaskTime = int.Parse(row[5])
                                  };
            if (row.Count > 6)
                forecastRow.Agents = double.Parse(row[6]);
            return forecastRow;
        }
    }
}
