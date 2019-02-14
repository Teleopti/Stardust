using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer
{
	public interface IIntradayApplicationStaffingService
	{
		ScheduledStaffingViewModel GenerateStaffingViewModel(Guid[] skillIdList, int dayOffset);

		ScheduledStaffingViewModel GenerateStaffingViewModel(Guid[] skillIdList, DateOnly? dateInLocalTime = null,
			bool useShrinkage = false);
	}

	public class IntradayAppStaffingService : IIntradayApplicationStaffingService
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;

		public IntradayAppStaffingService(INow now, IUserTimeZone timeZone, IStaffingViewModelCreator staffingViewModelCreator)
		{
			_now = now;
			_timeZone = timeZone;
			_staffingViewModelCreator = staffingViewModelCreator;
		}

		public ScheduledStaffingViewModel GenerateStaffingViewModel(Guid[] skillIdList, int dayOffset)
		{
			var localTimeWithOffset = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).AddDays(dayOffset);
			return GenerateStaffingViewModel(skillIdList, new DateOnly(localTimeWithOffset));
		}

		public ScheduledStaffingViewModel GenerateStaffingViewModel(
			Guid[] skillIdList,
			DateOnly? dateInLocalTime = null,
			bool useShrinkage = false)
		{
			return _staffingViewModelCreator.Load(skillIdList, dateInLocalTime, useShrinkage);
		}
	}
}
