using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class ScheduleDayReadModelsCreator : IScheduleDayReadModelsCreator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public ScheduleDayReadModel GetReadModel(ProjectionChangedEventScheduleDay schedule, IPerson person)
		{
			var ret = new ScheduleDayReadModel();
			var tz = person.PermissionInformation.DefaultTimeZone();

			ret.ContractTimeTicks = schedule.ContractTime.Ticks;
			ret.WorkTimeTicks = schedule.WorkTime.Ticks;
			ret.PersonId = person.Id.GetValueOrDefault();
			ret.Date = new DateOnly(schedule.Date);

			if (schedule.StartDateTime.HasValue && schedule.EndDateTime.HasValue)
			{
				ret.StartDateTime = TimeZoneInfo.ConvertTimeFromUtc(schedule.StartDateTime.Value, tz);
				ret.EndDateTime = TimeZoneInfo.ConvertTimeFromUtc(schedule.EndDateTime.Value, tz);
			}

			ret.Label = schedule.ShortName;
			ret.ColorCode = schedule.DisplayColor;
			ret.Workday = schedule.IsWorkday;
			ret.NotScheduled = schedule.NotScheduled;

			return ret;
		}
	}
}