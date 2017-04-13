using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public class AnalyticsPersonPeriodDateFixerWithoutDateCreation : AnalyticsPersonPeriodDateFixerBase
	{
		public AnalyticsPersonPeriodDateFixerWithoutDateCreation(IAnalyticsDateRepository analyticsDateRepository, IAnalyticsIntervalRepository analyticsIntervalRepository) :
			base(analyticsDateRepository, analyticsIntervalRepository)
		{
		}

		// Old way of not creating dates with new date repositry.
		public override DateTime ValidToDate(DateTime personPeriodEndDate, TimeZoneInfo timeZoneInfo)
		{
			var validToDate = personPeriodEndDate.Equals(AnalyticsDate.Eternity.DateDate) || personPeriodEndDate > AnalyticsDateRepository.MaxDate().DateDate
				? AnalyticsDate.Eternity.DateDate
				: timeZoneInfo.SafeConvertTimeToUtc(personPeriodEndDate.AddDays(1));
			// Add one days because there is no end time in app database but it is in analytics and we do not want gap between person periods end and start date.
			return validToDate;
		}
	}
}