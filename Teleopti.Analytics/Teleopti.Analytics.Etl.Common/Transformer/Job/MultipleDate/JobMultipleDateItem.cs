using System;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate
{
	public class JobMultipleDateItem : IJobMultipleDateItem
	{
		private readonly DateTime _startDateLocal;
		private readonly DateTime _endDateLocal;
		private readonly TimeZoneInfo _timeZoneInfo;

		public JobMultipleDateItem(DateTimeKind dateTimeKind, DateTime startDate, DateTime endDate, TimeZoneInfo timeZone)
		{
			_timeZoneInfo = timeZone;
			if (dateTimeKind == DateTimeKind.Utc)
			{
				//UTC incoming
				_startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDate, _timeZoneInfo);
				_endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDate, _timeZoneInfo);
			}
			else
			{
				// Local incoming
				_startDateLocal = startDate;
				_endDateLocal = endDate;
			}
		}

		public DateTime StartDateLocal => _startDateLocal;

		public DateTime StartDateUtc => _timeZoneInfo.SafeConvertTimeToUtc(StartDateLocal);

		public DateTime EndDateUtc => _timeZoneInfo.SafeConvertTimeToUtc(EndDateLocal);

		public DateTime StartDateUtcFloor => StartDateUtc.Date;

		public DateTime EndDateUtcCeiling => EndDateUtc.Date.AddDays(1).AddMilliseconds(-1);

		public DateTime EndDateLocal => _endDateLocal;
	}
}