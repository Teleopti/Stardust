using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeRequestDefaultStartTimeProvider : IOvertimeRequestDefaultStartTimeProvider
	{
		private readonly IShiftEndTimeProvider _shiftEndTimeProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly INow _now;
		private static readonly int _overtimeRequestStartTimeFlexibilityInMinutes = 5;
		private static readonly int _overtimeRequestStartTimeRoundMinutes = 30;
		private static readonly int _defaultWorkDayStartTimeInHour = 8;

		public OvertimeRequestDefaultStartTimeProvider(IShiftEndTimeProvider shiftEndTimeProvider, ILoggedOnUser loggedOnUser, INow now)
		{
			_shiftEndTimeProvider = shiftEndTimeProvider;
			_loggedOnUser = loggedOnUser;
			_now = now;
		}

		public DateTime GetDefaultStartTime(DateOnly date)
		{
			var possibleShiftEndTimes = getPossibleShiftEndTimes(date);
			var requestDate = TimeZoneHelper.ConvertToUtc(date.Date, _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());

			var validEndTimeList = possibleShiftEndTimes
				.Where(e => _now.UtcDateTime().CompareTo(e) < 0 && e.CompareTo(requestDate) >= 0).ToList();

			if (validEndTimeList.Any())
			{
				return TimeZoneHelper.ConvertFromUtc(validEndTimeList.Min(), _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());
			}
			return getNowRoundUpHour();
		}

		private DateTime[] getPossibleShiftEndTimes(DateOnly date)
		{
			var todayShiftEndTime = _shiftEndTimeProvider.GetShiftEndTimeForPerson(_loggedOnUser.CurrentUser(), date);
			var yesterdayShiftEndTime = _shiftEndTimeProvider.GetShiftEndTimeForPerson(_loggedOnUser.CurrentUser(), date.AddDays(-1));
			return new[] { getDefaultEndTime(date.AddDays(-1), yesterdayShiftEndTime), getDefaultEndTime(date, todayShiftEndTime) };
		}

		private DateTime getDefaultEndTime(DateOnly date, DateTime? shiftEndTime)
		{
			if (shiftEndTime.HasValue)
				return shiftEndTime.Value;

			return TimeZoneHelper.ConvertToUtc(date.Date.AddHours(_defaultWorkDayStartTimeInHour),
				_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());
		}

		private DateTime getNowRoundUpHour()
		{
			var localNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());
			var newLocalNow = localNow.AddMinutes(OvertimeMinimumApprovalThresholdInMinutes.MinimumApprovalThresholdTimeInMinutes + _overtimeRequestStartTimeFlexibilityInMinutes);

			if (newLocalNow.Minute > _overtimeRequestStartTimeRoundMinutes)
			{
				return newLocalNow.AddHours(1).AddMinutes(-newLocalNow.Minute);
			}
			return newLocalNow.AddMinutes(-newLocalNow.Minute + _overtimeRequestStartTimeRoundMinutes);
		}
	}
}
