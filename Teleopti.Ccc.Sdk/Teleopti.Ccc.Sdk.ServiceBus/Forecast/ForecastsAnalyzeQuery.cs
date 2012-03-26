using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public interface IForecastsAnalyzeQuery
    {
        IForecastsAnalyzeQueryResult Run(IEnumerable<IForecastsFileRow> forecastRows, TimeSpan midnightBreakOffSet);
    }

    public class ForecastsAnalyzeQuery : IForecastsAnalyzeQuery
    {
        public IForecastsAnalyzeQueryResult Run(IEnumerable<IForecastsFileRow> forecastRows, TimeSpan midnightBreakOffSet)
        {
            var result = new ForecastsAnalyzeQueryResult { ForecastFileContainer = new ForecastFileContainer() };
            var startDateTime = DateTime.MaxValue;
            var endDateTime = DateTime.MinValue;
            var workloadDayOpenHours = new WorkloadDayOpenHoursContainer();

            string previousSkillName = null;
            TimeSpan? previousIntervalLength = null;

            foreach (var forecastsRow in forecastRows)
            {
                if (!string.IsNullOrEmpty(previousSkillName) && !forecastsRow.SkillName.Equals(previousSkillName))
                {
                    result.ErrorMessage = "There exists multiple skill names in the file.";
                    break;
                }

                var intervalLength = forecastsRow.LocalDateTimeTo.Subtract(forecastsRow.LocalDateTimeFrom);
                if (previousIntervalLength.HasValue && intervalLength != previousIntervalLength)
                {
                    result.ErrorMessage = "Intervals do not have the same length.";
                    break;
                }
                if (forecastsRow.LocalDateTimeFrom < startDateTime)
                    startDateTime = forecastsRow.LocalDateTimeFrom;
                if (forecastsRow.LocalDateTimeTo > endDateTime)
                    endDateTime = forecastsRow.LocalDateTimeTo;

                var day = new DateOnly(forecastsRow.LocalDateTimeFrom.Subtract(midnightBreakOffSet));
                workloadDayOpenHours.AddOpenHour(day,
                                         new TimePeriod(forecastsRow.LocalDateTimeFrom.Subtract(day),
                                                        forecastsRow.LocalDateTimeTo.Subtract(day)));

                result.ForecastFileContainer.AddForecastsRow(day, forecastsRow);

                previousSkillName = forecastsRow.SkillName;
                previousIntervalLength = intervalLength;
            }
            result.Period = new DateOnlyPeriod(new DateOnly(startDateTime), new DateOnly(endDateTime));
            result.WorkloadDayOpenHours = workloadDayOpenHours;
            result.IntervalLength = previousIntervalLength.GetValueOrDefault();
            result.SkillName = previousSkillName;
            return result;
        }
    }
}