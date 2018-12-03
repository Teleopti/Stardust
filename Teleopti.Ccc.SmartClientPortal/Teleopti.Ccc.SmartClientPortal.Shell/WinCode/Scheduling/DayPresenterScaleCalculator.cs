using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public interface IDayPresenterScaleCalculator
    {
        DateTimePeriod CalculateScalePeriod(ISchedulerStateHolder schedulerState, DateOnly selectedDate);
    }
    public class DayPresenterScaleCalculator : IDayPresenterScaleCalculator
    {
	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public DateTimePeriod CalculateScalePeriod(ISchedulerStateHolder schedulerState, DateOnly selectedDate)
        {
            var min = DateTime.MaxValue;
            var max = DateTime.MinValue;
            var timeZone = TimeZoneGuard.Instance.TimeZone;
            foreach (var person in schedulerState.FilteredCombinedAgentsDictionary.Values)
            {
                var range = schedulerState.Schedules[person];
	           
                if (min.TimeOfDay != TimeSpan.Zero)
                {
                    var yesterDay = range.ScheduledDay(selectedDate.AddDays(-1));
	                var projectionYesterday = yesterDay.ProjectionService().CreateProjection();

	                if (projectionYesterday.HasLayers)
	                {
		                var periodYesterday = projectionYesterday.Period();
		                if (periodYesterday != null)
		                {
			                if (periodYesterday.Value.EndDateTimeLocal(timeZone) > selectedDate.Date)
			                {
								var maxTemp = periodYesterday.Value.EndDateTimeLocal(timeZone);
								min = selectedDate.Date;
								if (maxTemp > max) max = maxTemp;   
			                }
		                }
	                }

                }

				var today = range.ScheduledDay(selectedDate);
	            var projectionToday = today.ProjectionService().CreateProjection();

				if (!projectionToday.HasLayers) continue;
				var periodToday = projectionToday.Period();
	            if (periodToday == null) continue;
	            if (max < periodToday.Value.EndDateTimeLocal(timeZone)) max = periodToday.Value.EndDateTimeLocal(timeZone);
	            if (min > periodToday.Value.StartDateTimeLocal(timeZone)) min = periodToday.Value.StartDateTimeLocal(timeZone);
            }

            if (min == DateTime.MaxValue && max == DateTime.MinValue)
            {
                var baseDate = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, 0, 0, 0, 0,
                                                 DateTimeKind.Utc);
                return new DateTimePeriod(baseDate.AddHours(7), baseDate.AddHours(17));
            }

            min = min.Date.Add(TimeHelper.FitToDefaultResolutionRoundDown(min.TimeOfDay, 60));
            max = max.Date.Add(TimeHelper.FitToDefaultResolution(max.TimeOfDay, 60));
            if (min.TimeOfDay >= TimeSpan.FromHours(1))
                min = min.AddHours(-1);
            max = max.AddHours(1);
            if (min.TimeOfDay > TimeSpan.FromHours(8))
                min = min.Date.AddHours(8);
            if (max.Date == min.Date)
            {
                if (max.TimeOfDay < TimeSpan.FromHours(17))
                    max = max.Date.AddHours(17);
            }



            return new DateTimePeriod(new DateTime(min.Year, min.Month, min.Day, min.Hour, min.Minute, 0, 0, DateTimeKind.Utc), new DateTime(max.Year, max.Month, max.Day, max.Hour, max.Minute, 0, 0, DateTimeKind.Utc));

        }
    }
}