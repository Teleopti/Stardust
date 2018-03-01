using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public class PerformAllBadgeCalculation :IPerformBadgeCalculation
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(PerformBadgeCalculation));
		private readonly IBusinessUnitRepository _buRepository;
		private readonly ILogObjectDateChecker _logObjectDateChecker;
		private readonly CalculateBadges _calculateBadges;

		public PerformAllBadgeCalculation(IBusinessUnitRepository buRepository, ILogObjectDateChecker logObjectDateChecker, CalculateBadges calculateBadges)
		{
			_buRepository = buRepository;
			_logObjectDateChecker = logObjectDateChecker;
			_calculateBadges = calculateBadges;
		}

		public void Calculate(Guid businessUnitId, DateTime date)
		{
			var timeZoneList = _buRepository.LoadAllTimeZones().ToList();

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat($"Total {timeZoneList.Count} timezone loaded.");
			}

			foreach (var timeZoneInfo in timeZoneList)
			{
				calculateSystemBadges(businessUnitId, timeZoneInfo.Id, date);
			}

			_calculateBadges.CalculateExternalBadge(new CalculateBadgeMessage
			{
				LogOnBusinessUnitId = businessUnitId,
				CalculationDate = date
			});
		}

		private void calculateSystemBadges(Guid businessUnitId, string timeZoneInfoId, DateTime date)
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId);
			var dateForGivenTimeZone = TimeZoneHelper.ConvertFromUtc(date, timeZone);
			var calculationDateForGivenTimeZone = dateForGivenTimeZone.Date;

			if (logger.IsDebugEnabled)
			{
				logger.Debug($"Calculate badge of {calculationDateForGivenTimeZone:yyyy-MM-dd} for business "
							 + $"unit (Id=\"{businessUnitId}\" and timezone $(Id=\"{timeZoneInfoId}\"))");
			}

			if (_logObjectDateChecker.HistoricalDataIsEarlierThan(new DateOnly(calculationDateForGivenTimeZone)))
			{
				logger.Warn("Badge calculation result may be incorrect since one or more historical data detail "
							+ $"comes late than latest interval of \"{calculationDateForGivenTimeZone:yyyy-MM-dd}\"");
			}

			_calculateBadges.Calculate(new CalculateBadgeMessage
			{
				LogOnBusinessUnitId = businessUnitId,
				CalculationDate = calculationDateForGivenTimeZone,
				TimeZoneCode = timeZoneInfoId
			});
		}
	}
}
