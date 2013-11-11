using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class ScheduleDayReadModelsCreator : IScheduleDayReadModelsCreator
	{
		public ScheduleDayReadModel GetReadModel(ProjectionChangedEventScheduleDay schedule, IPerson person)
		{
			var ret = new ScheduleDayReadModel();
			var tz = person.PermissionInformation.DefaultTimeZone();

			ret.ContractTimeTicks = schedule.ContractTime.Ticks;
			ret.WorkTimeTicks = schedule.WorkTime.Ticks;
			ret.PersonId = person.Id.GetValueOrDefault();
			ret.Date = new DateOnly(schedule.Date);

			if (schedule.Shift != null)
			{
				ret.StartDateTime = TimeZoneInfo.ConvertTimeFromUtc(schedule.Shift.StartDateTime, tz);
				ret.EndDateTime = TimeZoneInfo.ConvertTimeFromUtc(schedule.Shift.EndDateTime, tz);
			}

			ret.Label = schedule.ShortName;
			ret.ColorCode = schedule.DisplayColor;
			ret.Workday = schedule.IsWorkday;
			ret.NotScheduled = schedule.NotScheduled;

			return ret;
		}
	}
}