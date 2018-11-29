using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IForecastsAnalyzeQuery
    {
        IForecastsAnalyzeQueryResult Run(IEnumerable<IForecastsRow> forecastRows, ISkill skill);
    }

    public class ForecastsAnalyzeQuery : IForecastsAnalyzeQuery
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IForecastsAnalyzeQueryResult Run(IEnumerable<IForecastsRow> forecastRows, ISkill skill)
        {
            var result = new ForecastsAnalyzeQueryResult { ForecastFileContainer = new ForecastFileContainer() };
            var startDateTime = DateTime.MaxValue;
            var endDateTime = DateTime.MinValue;
            var workloadDayOpenHours = new WorkloadDayOpenHoursContainer();

            string previousSkillName = null;
            TimeSpan? previousIntervalLength = null;
            var rowNumber = 1;
            foreach (var forecastsRow in forecastRows)
            {
                if (!string.IsNullOrEmpty(previousSkillName) && !forecastsRow.SkillName.Equals(previousSkillName))
                {
                    result.ErrorMessage = string.Format(CultureInfo.InvariantCulture,
                                                        "Line {0}, Error:There exists multiple skill names in the file.",
                                                        rowNumber);
                    return result;
                }
                var intervalLength = forecastsRow.UtcDateTimeTo.Subtract(forecastsRow.UtcDateTimeFrom);
                if (intervalLength != TimeSpan.FromMinutes(skill.DefaultResolution))
                {
                    result.ErrorMessage = string.Format(CultureInfo.InvariantCulture,
                                                        "Line {0}, Error:Interval does not match the target skill: {1}.",
                                                        rowNumber, skill.Name);
                    return result;
                }
                if (previousIntervalLength.HasValue && intervalLength != previousIntervalLength)
                {
                    result.ErrorMessage = string.Format(CultureInfo.InvariantCulture,
                                                        "Line {0}, Error:Intervals do not have the same length.",
                                                        rowNumber);
                    return result;
                }
                if (forecastsRow.LocalDateTimeFrom < startDateTime)
                    startDateTime = forecastsRow.LocalDateTimeFrom;
                if (forecastsRow.LocalDateTimeTo > endDateTime)
                    endDateTime = forecastsRow.LocalDateTimeTo;

                var day = new DateOnly(forecastsRow.LocalDateTimeFrom.Subtract(skill.MidnightBreakOffset));
                workloadDayOpenHours.AddOpenHour(day,
                                         new TimePeriod(forecastsRow.LocalDateTimeFrom.Subtract(day.Date),
                                                        forecastsRow.LocalDateTimeTo.Subtract(day.Date)));

                result.ForecastFileContainer.AddForecastsRow(day, forecastsRow);

                previousSkillName = forecastsRow.SkillName;
                previousIntervalLength = intervalLength;
                rowNumber++;
            }
				if (endDateTime.TimeOfDay == TimeSpan.Zero)
		        endDateTime = endDateTime.AddDays(-1);

				result.Period = new DateOnlyPeriod(new DateOnly(startDateTime), new DateOnly(endDateTime));
            result.WorkloadDayOpenHours = workloadDayOpenHours;
            result.IntervalLength = previousIntervalLength.GetValueOrDefault();
            result.SkillName = previousSkillName;
            return result;
        }
    }
}