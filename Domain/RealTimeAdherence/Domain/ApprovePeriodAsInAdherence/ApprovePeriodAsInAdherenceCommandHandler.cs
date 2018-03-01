using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence
{
	public class ApprovePeriodAsInAdherenceCommandHandler
	{
		private readonly ApprovePeriodAsInAdherence _approve;
		private readonly IUserTimeZone _timeZone;

		public ApprovePeriodAsInAdherenceCommandHandler(ApprovePeriodAsInAdherence approve, IUserTimeZone timeZone)
		{
			_approve = approve;
			_timeZone = timeZone;
		}

		public void Handle(ApprovePeriodAsInAdherenceCommand command)
		{
			var startDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.StartDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			var endDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.EndDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			_approve.Approve(new ApprovedPeriod
			{
				PersonId = command.PersonId,
				StartTime = startDateTime,
				EndTime = endDateTime
			});
		}
	}
}