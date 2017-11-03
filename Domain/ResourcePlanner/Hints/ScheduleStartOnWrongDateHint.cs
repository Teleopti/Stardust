using System.Linq;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class ScheduleStartOnWrongDateHint : IScheduleHint
	{
		public static string ErrorOutput = Resources.ShiftStartsDayBeforeOrAfter;
		
		public void FillResult(HintResult hintResult, HintInput input)
		{
			var agents = input.People;
			var period = input.Period;
			var schedules = input.Schedules;
			if (schedules == null)
				return;
			
			var scheduleDays = schedules.SchedulesForPeriod(period, agents.ToArray());
			foreach (var scheduleDay in scheduleDays)
			{
				var startLocal = scheduleDay.PersonAssignment(true).Period
					.StartDateTimeLocal(scheduleDay.Person.PermissionInformation.DefaultTimeZone());
				var dateOfScheduleDay = scheduleDay.DateOnlyAsPeriod.DateOnly.Date; 
				if (startLocal < dateOfScheduleDay || startLocal >= dateOfScheduleDay.AddDays(1))
				{
					hintResult.Add(new PersonHintError(scheduleDay.Person)
					{
						ValidationError = string.Format(ErrorOutput, dateOfScheduleDay)
					}, GetType());
				}
			}
		}
	}
}