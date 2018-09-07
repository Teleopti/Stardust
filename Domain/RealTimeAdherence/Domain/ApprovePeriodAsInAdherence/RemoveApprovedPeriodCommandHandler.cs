using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence
{
	public class RemoveApprovedPeriodCommandHandler
	{
		private readonly RemoveApprovedPeriod _remove;
		private readonly IUserTimeZone _timeZone;

		public RemoveApprovedPeriodCommandHandler(RemoveApprovedPeriod remove, IUserTimeZone timeZone)
		{
			_remove = remove;
			_timeZone = timeZone;
		}

		public void Handle(RemoveApprovedPeriodCommand command)
		{
			var startDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.StartDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			var endDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.EndDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			_remove.Remove(new RemovedPeriod
			{
				PersonId = command.PersonId,
				StartTime = startDateTime,
				EndTime = endDateTime
			});
		}
	}
}