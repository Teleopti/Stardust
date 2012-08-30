using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class ScheduleDayReadModelCreator
	{
		public static ScheduleDayReadModel TurnScheduleToModel(IScheduleDay scheduleDay)
		{
			var ret = new ScheduleDayReadModel();
			var person = scheduleDay.Person;
			var tz = person.PermissionInformation.DefaultTimeZone();
			var proj = scheduleDay.ProjectionService().CreateProjection();

			ret.ContractTimeTicks = proj.ContractTime().Ticks;
			ret.WorkTimeTicks = proj.WorkTime().Ticks;
			ret.PersonId = person.Id.Value;
			ret.Date = scheduleDay.DateOnlyAsPeriod.DateOnly;

			var period = proj.Period();
			if (period != null)
			{
				ret.StartDateTime = period.Value.StartDateTimeLocal(tz);
				ret.EndDateTime = period.Value.EndDateTimeLocal(tz);
			}

			var significantPart = scheduleDay.SignificantPart();
			if (significantPart.Equals(SchedulePartView.MainShift))
			{
				var cat = scheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory;
				ret.Label = cat.Description.ShortName;
				ret.ColorCode = cat.DisplayColor.ToArgb();
				ret.WorkDay = true;
			}
			if (significantPart.Equals(SchedulePartView.Overtime))
			{
				ret.WorkDay = true;
			}
			if (significantPart.Equals(SchedulePartView.FullDayAbsence))
				ret.Label = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.Description.ShortName;
			if (significantPart.Equals(SchedulePartView.DayOff))
				ret.Label = scheduleDay.PersonDayOffCollection()[0].DayOff.Description.ShortName;


			return ret;
		}
	}
}