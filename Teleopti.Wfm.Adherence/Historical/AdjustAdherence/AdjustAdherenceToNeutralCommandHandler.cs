using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Historical.AdjustAdherence
{
	public class AdjustAdherenceToNeutralCommandHandler
	{
		private readonly IUserTimeZone _timeZone;
		private readonly AdjustAdherenceToNeutral _adjuster;

		public AdjustAdherenceToNeutralCommandHandler(IUserTimeZone timeZone, AdjustAdherenceToNeutral adjuster)
		{
			_timeZone = timeZone;
			_adjuster = adjuster;
		}

		public void Handle(AdjustAdherenceToNeutralCommand command)
		{
			var startDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.StartDateTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			var endDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.EndDateTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			
			var adjustPeriod = new AdjustedPeriod
			{
				StartTime = startDateTime,
				EndTime = endDateTime
			};

			_adjuster.Adjust(adjustPeriod);
		}
	}
}