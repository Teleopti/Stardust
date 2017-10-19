using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class ScheduleStartOnWrongDateValidator : IScheduleValidator
	{
		public static string ErrorOutput = "Ändra denna sträng till resursnyckel sen {0}"; // = Resources.Blabla
		
		public void FillResult(ValidationResult validationResult, IScheduleDictionary schedules, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
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
					validationResult.Add(new PersonValidationError(scheduleDay.Person), GetType());
				}
			}
		}
	}
}