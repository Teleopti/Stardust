using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
			_persister.Persist(new ApprovedPeriod
			{
				PersonId = command.PersonId,
				StartTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.StartDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone()),
				EndTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.EndDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone())
			});
		}
	}
}