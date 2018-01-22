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
		private readonly IShiftStartTimeProvider _shiftStartTimeProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly INow _now;
		private static readonly int _overtimeRequestStartTimeFlexibilityInMinutes = 5;
		private static readonly int _overtimeRequestStartTimeRoundMinutes = 30;
		private static readonly int _defaultWorkDayStartTimeInHour = 8;

		public OvertimeRequestDefaultStartTimeProvider(ILoggedOnUser loggedOnUser, INow now, IShiftStartTimeProvider shiftStartTimeProvider, IShiftEndTimeProvider shiftEndTimeProvider)
		{
			_loggedOnUser = loggedOnUser;
			_now = now;
			_shiftEndTimeProvider = shiftEndTimeProvider;
			_shiftStartTimeProvider = shiftStartTimeProvider;
		}

		public OvertimeDefaultStartTimeResult GetDefaultStartTime(DateOnly date)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var currentUserTimeZone = currentUser.PermissionInformation.DefaultTimeZone();

			var yesterdayAndTodaysPossibleShiftEndTimes = getPossibleShiftEndTimes(date);
			var requestDate = TimeZoneHelper.ConvertToUtc(date.Date, currentUserTimeZone);

			var shiftStartTime = _shiftStartTimeProvider.GetShiftStartTimeForPerson(currentUser, date);
			var validEndTimeList = yesterdayAndTodaysPossibleShiftEndTimes
				.Where(e => _now.UtcDateTime().CompareTo(e) < 0 && e.CompareTo(requestDate) >= 0).ToList();

			var utcNowPlusGap = _now.UtcDateTime().AddMinutes(OvertimeMinimumApprovalThresholdInMinutes.MinimumApprovalThresholdTimeInMinutes +
														   _overtimeRequestStartTimeFlexibilityInMinutes);

			if (shiftStartTime.HasValue && utcNowPlusGap.CompareTo(shiftStartTime) <= 0 &&
				yesterdayAndTodaysPossibleShiftEndTimes.Last().Date == date.AddDays(1).Date)
			{
				return buildDefaultStartTimeResult(TimeZoneHelper.ConvertFromUtc(shiftStartTime.GetValueOrDefault(), currentUserTimeZone), true, false);
			}

			if (validEndTimeList.Any(e => utcNowPlusGap.CompareTo(e) <= 0))
			{
				if (utcNowPlusGap.CompareTo(validEndTimeList.Min()) <= 0)
				{
					return buildDefaultStartTimeResult(TimeZoneHelper.ConvertFromUtc(validEndTimeList.Min(), currentUserTimeZone), false, true);
				}
				return buildDefaultStartTimeResult(getLocalRoundUpHour(utcNowPlusGap), false, false);
			}

			return buildDefaultStartTimeResult(getLocalRoundUpHour(_now.UtcDateTime()), false, false);
		}

		private OvertimeDefaultStartTimeResult buildDefaultStartTimeResult(DateTime datetime, bool isShiftStartTime, bool isShiftEndTime)
		{
			var result = new OvertimeDefaultStartTimeResult
			{
				IsShiftStartTime = isShiftStartTime,
				IsShiftEndTime = isShiftEndTime,
				DefaultStartTime = datetime
			};

			return result;
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

		private DateTime getLocalRoundUpHour(DateTime utcNow)
		{
			var localNow = TimeZoneHelper.ConvertFromUtc(utcNow, _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());
			var newLocalNow = localNow.AddMinutes(OvertimeMinimumApprovalThresholdInMinutes.MinimumApprovalThresholdTimeInMinutes + _overtimeRequestStartTimeFlexibilityInMinutes);

			if (newLocalNow.Minute > _overtimeRequestStartTimeRoundMinutes)
			{
				return newLocalNow.AddHours(1).AddMinutes(-newLocalNow.Minute);
			}
			return newLocalNow.AddMinutes(-newLocalNow.Minute + _overtimeRequestStartTimeRoundMinutes);
		}
	}
}
