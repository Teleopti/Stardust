using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Historical.Adjustment
{
	public class AdjustPeriodToNeutralCommandHandler
	{
		private readonly IUserTimeZone _timeZone;
		private readonly Historical.Adjustment.Adjustment _adjuster;

		public AdjustPeriodToNeutralCommandHandler(IUserTimeZone timeZone, Historical.Adjustment.Adjustment adjuster)
		{
			_timeZone = timeZone;
			_adjuster = adjuster;
		}

		public void Handle(AdjustPeriodToNeutralCommand command)
		{
			_adjuster.AdjustToNeutral(new PeriodToAdjust
			{
				StartTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.StartDateTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), _timeZone.TimeZone()),
				EndTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.EndDateTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), _timeZone.TimeZone())
			});
		}
	}
}