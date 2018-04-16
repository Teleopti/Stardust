﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class StaffingDataAvailablePeriodProvider : IStaffingDataAvailablePeriodProvider
	{
		private readonly INow _now;
		private readonly IToggleManager _toggleManager;
		private readonly ILoggedOnUser _user;
		private readonly IOvertimeRequestOpenPeriodProvider _mergedOvertimeRequestOpenPeriodProvider;

		public StaffingDataAvailablePeriodProvider(INow now, IToggleManager toggleManager, ILoggedOnUser user, IOvertimeRequestOpenPeriodProvider mergedOvertimeRequestOpenPeriodProvider)
		{
			_now = now;
			_toggleManager = toggleManager;
			_user = user;
			_mergedOvertimeRequestOpenPeriodProvider = mergedOvertimeRequestOpenPeriodProvider;
		}

		public DateOnlyPeriod? GetPeriodForAbsence(DateOnly date, bool forThisWeek)
		{
			return getStaffingDataAvailablePeriod(date, forThisWeek);
		}

		public List<DateOnlyPeriod> GetPeriodsForOvertime(DateOnly date, bool forThisWeek)
		{
			var person = _user.CurrentUser();
			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet == null)
				return new List<DateOnlyPeriod>();

			if (workflowControlSet.OvertimeRequestOpenPeriods.Count == 0)
			{
				var staffingDataAvailablePeriod = getStaffingDataAvailablePeriod(date, forThisWeek);
				return staffingDataAvailablePeriod.HasValue
					? new List<DateOnlyPeriod> {staffingDataAvailablePeriod.Value}
					: new List<DateOnlyPeriod>();
			}

			var period = date.ToDateOnlyPeriod();
			if (forThisWeek)
			{
				period = DateHelper.GetWeekPeriod(date, CultureInfo.CurrentCulture);
			}

			var timezone = person.PermissionInformation.DefaultTimeZone();
			var days = period.DayCollection();
			var availableDays = new List<DateOnly>();
			foreach (var day in days)
			{
				var overtimeRequestOpenPeriods =
					_mergedOvertimeRequestOpenPeriodProvider.GetOvertimeRequestOpenPeriods(person,
						day.ToDateTimePeriod(timezone));

				if (overtimeRequestOpenPeriods != null && overtimeRequestOpenPeriods.Any())
				{
					availableDays.Add(day);
				}
			}
			if (availableDays.Count == 0)
			{
				return new List<DateOnlyPeriod>();
			}

			var availablePeriods = availableDays.SplitToContinuousPeriods();
			var returnPeriods = new List<DateOnlyPeriod>();

			foreach (var availablePeriod in availablePeriods)
			{
				var intersectionPeriod = availablePeriod.Intersection(period);
				if (intersectionPeriod.HasValue)
				{
					returnPeriods.Add(intersectionPeriod.Value);
				}
			}

			return returnPeriods;
		}

		private DateOnlyPeriod? getStaffingDataAvailablePeriod(DateOnly date, bool forThisWeek)
		{
			var period = date.ToDateOnlyPeriod();
			if (forThisWeek)
			{
				period = DateHelper.GetWeekPeriod(date, CultureInfo.CurrentCulture);
			}
			var today =
				new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(),
					_user.CurrentUser().PermissionInformation.DefaultTimeZone()));

			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(_toggleManager);
			var maxEndDate = today.AddDays(staffingInfoAvailableDays);

			var availablePeriod = new DateOnlyPeriod(today, maxEndDate);
			return availablePeriod.Intersection(period);
		}
	}
}