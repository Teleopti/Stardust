using System;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public interface IAnalyticsPersonPeriodDateFixer
	{
		DateTime ValidFromDate(DateTime personPeriodStartDate, TimeZoneInfo timeZoneInfo);
		int ValidFromIntervalId(DateTime validFromDate, int intervalsPerDay);
		DateTime ValidToDate(DateTime personPeriodEndDate, TimeZoneInfo timeZoneInfo);
		int ValidToIntervalId(DateTime validToDate, int intervalsPerDay);

		DateTime GetPeriodIntervalEndDate(DateTime endDate, int intervalsPerDay);
		int GetValidToDateIdMaxDate(DateTime validToDate, int validToDateId);
		int GetValidToIntervalIdMaxDate(int validToIntervalId, int validToDateId);

		int ValidToDateIdLocal(int dateId);
		DateTime ValidToDateLocal(DateTime personPeriodEndDate);
		DateTime ValidFromDateLocal(DateTime personPeriodStartDate);

		int MapDateId(DateTime date);
	}
}