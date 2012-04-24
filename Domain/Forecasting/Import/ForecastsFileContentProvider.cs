using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IForecastsFileContentProvider
    {
        ICollection<IForecastsRow> LoadContent(byte[] fileContent, ICccTimeZoneInfo timeZone);
    }

    public class ForecastsFileContentProvider : IForecastsFileContentProvider
    {
        private readonly IForecastsRowExtractor _rowExtractor;

        public ForecastsFileContentProvider(IForecastsRowExtractor rowExtractor)
        {
            _rowExtractor = rowExtractor;
        }

        public ICollection<IForecastsRow> LoadContent(byte[] fileContent, ICccTimeZoneInfo timeZone)
        {
            var rowNumber = 1;
            var result = new List<IForecastsRow>();
            try
            {
                foreach (var line in Encoding.UTF8.GetString(fileContent).Split(new[] { Environment.NewLine },
                                                                                StringSplitOptions.RemoveEmptyEntries))
                {
                    var forecastsRow = _rowExtractor.Extract(line, timeZone);
                    if (timeZone.IsAmbiguousTime(forecastsRow.LocalDateTimeFrom) && timeZone.IsAmbiguousTime(forecastsRow.LocalDateTimeTo))
                    {
                        var missingRow = new ForecastsRow
                                             {
                                                 AfterTaskTime = forecastsRow.AfterTaskTime,
                                                 Agents = forecastsRow.Agents,
                                                 LocalDateTimeFrom = forecastsRow.LocalDateTimeFrom,
                                                 LocalDateTimeTo = forecastsRow.LocalDateTimeTo,
                                                 SkillName = forecastsRow.SkillName,
                                                 Tasks = forecastsRow.Tasks,
                                                 TaskTime = forecastsRow.TaskTime,
                                                 UtcDateTimeFrom = adjustMissingUtcTime(forecastsRow.UtcDateTimeFrom, (TimeZoneInfo)timeZone.TimeZoneInfoObject),
                                                 UtcDateTimeTo = adjustMissingUtcTime(forecastsRow.UtcDateTimeTo,(TimeZoneInfo)timeZone.TimeZoneInfoObject)
                                             };
                        result.Add(missingRow);
                    }
                    else if (!timeZone.IsAmbiguousTime(forecastsRow.LocalDateTimeFrom) && timeZone.IsAmbiguousTime(forecastsRow.LocalDateTimeTo))
                    {
                        forecastsRow.UtcDateTimeTo = adjustMissingUtcTime(forecastsRow.UtcDateTimeTo,
                                                                       (TimeZoneInfo) timeZone.TimeZoneInfoObject);
                    }
                    result.Add(forecastsRow);
                    rowNumber++;
                }
            }
            catch (ValidationException exception)
            {
                throw new ValidationException(string.Format(CultureInfo.InvariantCulture,"Line {0}, Error:{1}", rowNumber, exception.Message));
            }
            return result;
        }
        
        private static DateTime adjustMissingUtcTime(DateTime utcTime, TimeZoneInfo timeZone)
        {
            var rules = timeZone.GetAdjustmentRules();
            var rul = rules.FirstOrDefault(r => r.DateStart < new DateTime(utcTime.Year, 1, 1) && r.DateEnd > new DateTime(utcTime.Year, 1, 1));
            return rul == null ? utcTime : utcTime.Add(-rul.DaylightDelta);
        }
    }
}