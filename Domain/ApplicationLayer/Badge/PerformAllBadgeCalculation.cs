using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public class PerformAllBadgeCalculation :IPerformBadgeCalculation
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(PerformAllBadgeCalculation));
		private readonly IBusinessUnitRepository _buRepository;
		private readonly CalculateBadges _calculateBadges;

		public PerformAllBadgeCalculation(IBusinessUnitRepository buRepository, CalculateBadges calculateBadges)
		{
			_buRepository = buRepository;
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

			_calculateBadges.Calculate(new CalculateBadgeMessage
			{
				LogOnBusinessUnitId = businessUnitId,
				CalculationDate = calculationDateForGivenTimeZone,
				TimeZoneCode = timeZoneInfoId
			});
		}
	}
}
