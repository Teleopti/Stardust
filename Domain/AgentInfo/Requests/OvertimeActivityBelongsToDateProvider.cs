using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	[EnabledBy(Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
	public class OvertimeActivityBelongsToDateProvider : IOvertimeActivityBelongsToDateProvider
	{
		private readonly TimeSpan belongsToDateThreshold = TimeSpan.FromHours(2);
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;

		public OvertimeActivityBelongsToDateProvider(IScheduleStorage scheduleStorage, ICurrentScenario currentScenario)
		{
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
		}

		public DateOnly GetBelongsToDate(IPerson person, DateTimePeriod overtimeActivityPeriod)
		{
			var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), overtimeActivityPeriod.ChangeStartTime(TimeSpan.FromDays(-1)),
				_currentScenario.Current());

			var yesterday = overtimeActivityPeriod.ToDateOnlyPeriod(timeZoneInfo).StartDate.AddDays(-1);
			var today = overtimeActivityPeriod.ToDateOnlyPeriod(timeZoneInfo).StartDate;

			var yesterdayGap =
				calculateValidGapToClosestShift(scheduleDictionary[person].ScheduledDay(yesterday), overtimeActivityPeriod);
			var todayGap =
				calculateValidGapToClosestShift(scheduleDictionary[person].ScheduledDay(today), overtimeActivityPeriod);

			if (isValidGap(yesterdayGap) && isValidGap(todayGap))
			{
				return yesterdayGap <= todayGap ? yesterday : today;
			}

			if (isValidGap(yesterdayGap) && !isValidGap(todayGap))
			{
				return yesterday;
			}

			if (!isValidGap(yesterdayGap) && isValidGap(todayGap))
			{
				return today;
			}

			return today;
		}

		private bool isValidGap(TimeSpan? gap)
		{
			return gap.HasValue;
		}

		private TimeSpan? calculateValidGapToClosestShift(IScheduleDay scheduleDay, DateTimePeriod overtimeActivityPeriod)
		{
			var personAssignment = scheduleDay.PersonAssignment();

			if (personAssignment == null)
				return null;

			TimeSpan? gap = null;

			if (overtimeActivityPeriod.StartDateTime.CompareTo(personAssignment.Period.EndDateTime) >= 0)
			{
				gap = overtimeActivityPeriod.StartDateTime - personAssignment.Period.EndDateTime;
			}
			else if (overtimeActivityPeriod.EndDateTime.CompareTo(personAssignment.Period.StartDateTime) <= 0)
			{
				gap = personAssignment.Period.StartDateTime - overtimeActivityPeriod.EndDateTime;
			}

			return gap > belongsToDateThreshold ? null : gap;
		}
	}

	[DisabledBy(Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
	public class OvertimeActivityBelongsToDateProviderToggle74984Off : IOvertimeActivityBelongsToDateProvider
	{
		public DateOnly GetBelongsToDate(IPerson person, DateTimePeriod overtimeActivityPeriod)
		{
			var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
			return new DateOnly(TimeZoneHelper.ConvertFromUtc(overtimeActivityPeriod.StartDateTime, timeZoneInfo));
		}
	}
}