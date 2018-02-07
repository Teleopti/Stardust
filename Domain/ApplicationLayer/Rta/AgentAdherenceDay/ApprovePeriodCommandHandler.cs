using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public class ApprovePeriodCommandHandler
	{
		private readonly IApprovedPeriodsPersister _persister;
		private readonly IUserTimeZone _timeZone;

		public ApprovePeriodCommandHandler(IApprovedPeriodsPersister persister, IUserTimeZone timeZone)
		{
			_persister = persister;
			_timeZone = timeZone;
		}

		public void Handle(ApprovePeriodCommand command)
		{
			var startDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.StartDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			var endDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.EndDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			var dateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			_persister.Persist(new ApprovedPeriod
			{
				PersonId = command.PersonId,
				StartTime = dateTimePeriod.StartDateTime,
				EndTime = dateTimePeriod.EndDateTime
			});
		}
	}
}