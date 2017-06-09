using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public class PerformBadgeCalculation : IPerformBadgeCalculation
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof (PerformBadgeCalculation));
		private readonly IBusinessUnitRepository _buRepository;
		private readonly INow _now;
		private readonly ILogObjectDateChecker _logObjectDateChecker;
		private readonly CalculateBadges _calculateBadges;

		public PerformBadgeCalculation(IBusinessUnitRepository buRepository, INow now,
			ILogObjectDateChecker logObjectDateChecker, CalculateBadges calculateBadges)
		{
			_buRepository = buRepository;
			_now = now;
			_logObjectDateChecker = logObjectDateChecker;
			_calculateBadges = calculateBadges;
		}

		public void Calculate(Guid businessUnitId)
		{
			var timeZoneList = _buRepository.LoadAllTimeZones().ToList();

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat($"Total {timeZoneList.Count} timezone loaded.");
			}

			foreach (var timeZoneInfo in timeZoneList)
			{
				calculateBadges(businessUnitId, timeZoneInfo.Id);
			}
		}

		private void calculateBadges(Guid businessUnitId, string timeZoneInfoId)
		{
			const int badgeCalculationDelayDays = -2;
			var today = _now.ServerDateTime_DontUse();
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId);
			var todayForGivenTimeZone = TimeZoneInfo.ConvertTime(today, TimeZoneInfo.Local, timeZone);
			var calculationDateForGivenTimeZone = todayForGivenTimeZone.AddDays(badgeCalculationDelayDays).Date;

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