using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public interface IForecastsAnalyzeCommand
    {
        ForecastsAnalyzeCommandResult Execute();
    }

    public class ForecastsAnalyzeCommand : IForecastsAnalyzeCommand
    {
        private readonly IEnumerable<IForecastsFileRow> _forecasts;

        public ForecastsAnalyzeCommand(IEnumerable<IForecastsFileRow> forecasts)
        {
            _forecasts = forecasts;
        }

        public ForecastsAnalyzeCommandResult Execute()
        {
            var result = new ForecastsAnalyzeCommandResult { ForecastFileDictionary = new ForecastFileDictionary() };
            var firstRow = _forecasts.First();
            var intervalLengthTicks = firstRow.LocalDateTimeTo.Subtract(firstRow.LocalDateTimeFrom).Ticks;
            var skillName = firstRow.SkillName;
            var startDateTime = DateTime.MaxValue;
            var endDateTime = DateTime.MinValue;
            var workloadDayOpenHours = new WorkloadDayOpenHoursDictionary();
            foreach (var forecastsRow in _forecasts)
            {
                if (!forecastsRow.SkillName.Equals(skillName))
                {
                    result.ErrorMessage = "There exists multiple skill names in the file.";
                    break;
                }
                if (forecastsRow.LocalDateTimeTo.Subtract(forecastsRow.LocalDateTimeFrom).Ticks != intervalLengthTicks)
                {
                    result.ErrorMessage = "Intervals do not have the same length.";
                    break;
                }
                if (forecastsRow.LocalDateTimeFrom < startDateTime)
                    startDateTime = forecastsRow.LocalDateTimeFrom;
                if (forecastsRow.LocalDateTimeTo > endDateTime)
                    endDateTime = forecastsRow.LocalDateTimeTo;

                workloadDayOpenHours.Add(new DateOnly(forecastsRow.LocalDateTimeFrom),
                                         new TimePeriod(forecastsRow.LocalDateTimeFrom.TimeOfDay,
                                                        forecastsRow.LocalDateTimeTo.TimeOfDay));

                result.ForecastFileDictionary.Add(new DateOnly(forecastsRow.LocalDateTimeFrom), forecastsRow);
            }
            result.Period = new DateOnlyPeriod(new DateOnly(startDateTime), new DateOnly(endDateTime));
            result.WorkloadDayOpenHours = workloadDayOpenHours;
            result.IntervalLengthTicks = intervalLengthTicks;
            result.SkillName = firstRow.SkillName;
            return result;
        }
    }
}