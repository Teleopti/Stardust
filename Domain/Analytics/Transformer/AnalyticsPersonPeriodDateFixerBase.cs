using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public abstract class AnalyticsPersonPeriodDateFixerBase : IAnalyticsPersonPeriodDateFixer
	{
		protected readonly IAnalyticsDateRepository AnalyticsDateRepository;
		protected readonly IAnalyticsIntervalRepository AnalyticsIntervalRepository;

		protected AnalyticsPersonPeriodDateFixerBase(IAnalyticsDateRepository analyticsDateRepository, IAnalyticsIntervalRepository analyticsIntervalRepository)
		{
			AnalyticsDateRepository = analyticsDateRepository;
			AnalyticsIntervalRepository = analyticsIntervalRepository;
		}

		public int GetValidToIntervalIdMaxDate(int validToIntervalId, int validToDateId)
		{
			// Samma som ValidToIntervalId om inte validToDateId är eterntity då ska det vara sista interval i dim_interval
			return validToDateId != AnalyticsDate.Eternity.DateId ? validToIntervalId : AnalyticsIntervalRepository.MaxInterval().IntervalId;
		}

		public DateTime ValidToDateLocal(DateTime personPeriodEndDate)
		{
			return personPeriodEndDate.Equals(AnalyticsDate.Eternity.DateDate) ? AnalyticsDateRepository.MaxDate().DateDate : personPeriodEndDate;
		}

		public DateTime ValidFromDateLocal(DateTime personPeriodStartDate)
		{
			var minDate = AnalyticsDateRepository.MinDate();
			return personPeriodStartDate < minDate.DateDate ? minDate.DateDate : personPeriodStartDate;
		}

		public int ValidToDateIdLocal(int dateId)
		{
			return dateId == AnalyticsDate.Eternity.DateId ? AnalyticsDateRepository.MaxDate().DateId : dateId;
		}

		public int ValidToIntervalId(DateTime validToDate, int intervalsPerDay)
		{
			return new IntervalBase(GetPeriodIntervalEndDate(validToDate, intervalsPerDay), intervalsPerDay).Id;
		}

		public int ValidFromIntervalId(DateTime validFromDate, int intervalsPerDay)
		{
			return new IntervalBase(validFromDate, intervalsPerDay).Id;
		}

		public DateTime GetPeriodIntervalEndDate(DateTime endDate, int intervalsPerDay)
		{
			if (endDate.Equals(AnalyticsDate.Eternity.DateDate))
			{
				return endDate;
			}

			var minutesPerInterval = 1440 / intervalsPerDay;
			return endDate.AddMinutes(-minutesPerInterval);
		}

		public int GetValidToDateIdMaxDate(DateTime validToDate, int validToDateId)
		{
			// Samma som ValidToDateId om inte eternity då ska vara näst sista dagen i dim_date
			return validToDate.Equals(AnalyticsDate.Eternity.DateDate)
				? AnalyticsDateRepository.MaxDate().DateId - 1
				: validToDateId;
		}

		public virtual DateTime ValidToDate(DateTime personPeriodEndDate, TimeZoneInfo timeZoneInfo)
		{
			var validToDate = personPeriodEndDate.Equals(AnalyticsDate.Eternity.DateDate)
				? AnalyticsDate.Eternity.DateDate
				: timeZoneInfo.SafeConvertTimeToUtc(personPeriodEndDate.AddDays(1));
			// Add one days because there is no end time in app database but it is in analytics and we do not want gap between person periods end and start date.
			return validToDate;
		}

		public DateTime ValidFromDate(DateTime personPeriodStartDate, TimeZoneInfo timeZoneInfo)
		{
			var minDate = AnalyticsDateRepository.MinDate().DateDate;
			if (personPeriodStartDate < minDate)
				return minDate;
			var validFromDate = timeZoneInfo.SafeConvertTimeToUtc(personPeriodStartDate);
			if (validFromDate >= AnalyticsDate.Eternity.DateDate)
				validFromDate = AnalyticsDate.Eternity.DateDate;
			return validFromDate;
		}

		public int MapDateId(DateTime date)
		{
			var analyticsDate = AnalyticsDateRepository.Date(date);
			return analyticsDate?.DateId ?? AnalyticsDate.NotDefined.DateId;
		}
	}
}