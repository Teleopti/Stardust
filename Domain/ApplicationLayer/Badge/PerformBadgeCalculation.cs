using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public class PerformBadgeCalculation : IPerformBadgeCalculation
	{
		private readonly IBusinessUnitRepository _buRepository;
		private readonly INow _now;
		private readonly CalculateBadges _calculateBadges;

		public PerformBadgeCalculation(IBusinessUnitRepository buRepository,  INow now,  CalculateBadges calculateBadges)
		{

			_buRepository = buRepository;
			_now = now;
			_calculateBadges = calculateBadges;
		}


		public void Calculate(Guid businessUnitId, bool fromEtl)
		{
			List<TimeZoneInfo> timeZoneList;
			timeZoneList = _buRepository.LoadAllTimeZones().ToList();

			foreach (var timeZoneInfo in timeZoneList)
			{
				calculateBadges(businessUnitId, timeZoneInfo.Id, fromEtl);
			}
		}

		private void calculateBadges(Guid businessUnitId, string timeZoneInfoId, bool fromEtl)
		{
			const int badgeCalculationDelayDays = -2;
			var today = _now.LocalDateTime();
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId);
			var todayForGivenTimeZone = TimeZoneInfo.ConvertTime(today, TimeZoneInfo.Local, timeZone);
			var calculationDateForGivenTimeZone = todayForGivenTimeZone.AddDays(badgeCalculationDelayDays).Date;

			_calculateBadges.Calculate(new CalculateBadgeMessage
			{
				LogOnBusinessUnitId = businessUnitId,
				CalculationDate = calculationDateForGivenTimeZone,
				TimeZoneCode = timeZoneInfoId
			}, fromEtl);

		}
	}
}