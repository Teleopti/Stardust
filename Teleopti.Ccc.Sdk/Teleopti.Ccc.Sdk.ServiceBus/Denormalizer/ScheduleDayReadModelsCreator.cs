using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class ScheduleDayReadModelsCreator : IScheduleDayReadModelsCreator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public ScheduleDayReadModel GetReadModel(DenormalizedScheduleDay schedule, IPerson person)
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

			ret.Label = schedule.Label;
			ret.ColorCode = schedule.DisplayColor;
			ret.Workday = schedule.IsWorkday;

			return ret;
		}
	}
}