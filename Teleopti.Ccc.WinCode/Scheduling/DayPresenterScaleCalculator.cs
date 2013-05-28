using System;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IDayPresenterScaleCalculator
    {
        DateTimePeriod CalculateScalePeriod(ISchedulerStateHolder schedulerState, DateOnly selectedDate);
    }
    public class DayPresenterScaleCalculator : IDayPresenterScaleCalculator
    {
	    private readonly IEditableShiftMapper _editableShiftMapper;

	    public DayPresenterScaleCalculator(IEditableShiftMapper editableShiftMapper)
		{
			_editableShiftMapper = editableShiftMapper;
		}

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public DateTimePeriod CalculateScalePeriod(ISchedulerStateHolder schedulerState, DateOnly selectedDate)
        {
            DateTime min = DateTime.MaxValue;
            DateTime max = DateTime.MinValue;
            TimeZoneInfo timeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
            foreach (var person in schedulerState.FilteredPersonDictionary.Values)
            {
                IScheduleRange range = schedulerState.Schedules[person];
				
                if (min.TimeOfDay != TimeSpan.Zero)
                {
                    IScheduleDay yesterDay = range.ScheduledDay(selectedDate.AddDays(-1));
                	timeZone = yesterDay.TimeZone;
                    foreach (var personAssignment in yesterDay.PersonAssignmentCollection())
                    {
	                    var shift = _editableShiftMapper.CreateEditorShift(personAssignment);
                        if (shift != null && shift.LayerCollection.Period().Value.EndDateTimeLocal(timeZone) > selectedDate.Date)
                        {
                            DateTime maxTemp =
                                shift.LayerCollection.Period().Value.EndDateTimeLocal(timeZone);
                            min = selectedDate.Date;
                            if(maxTemp > max)
                                max = maxTemp;
                        }
                            
                    }
                }

                IScheduleDay today = range.ScheduledDay(selectedDate);
                foreach (var personAssignment in today.PersonAssignmentCollection())
                {
					var shift = _editableShiftMapper.CreateEditorShift(personAssignment);
					if (shift != null)
                    {
						if (max < shift.LayerCollection.Period().Value.EndDateTimeLocal(timeZone))
							max = shift.LayerCollection.Period().Value.EndDateTimeLocal(timeZone);
						if (min > shift.LayerCollection.Period().Value.StartDateTimeLocal(timeZone))
							min = shift.LayerCollection.Period().Value.StartDateTimeLocal(timeZone);
                    }
                }
            }

            if (min == DateTime.MaxValue && max == DateTime.MinValue)
            {
                DateTime baseDate = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, 0, 0, 0, 0,
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