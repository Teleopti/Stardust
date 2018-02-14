using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

		public StaffingDataAvailablePeriodProvider(INow now, IToggleManager toggleManager, ILoggedOnUser user)
		{
			_now = now;
			_toggleManager = toggleManager;
			_user = user;
		}

		public DateOnlyPeriod? GetPeriodForAbsence(DateOnly date, bool forThisWeek)
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

		public DateOnlyPeriod? GetPeriodForOvertime(DateOnly date, bool forThisWeek)
		{
			var period = date.ToDateOnlyPeriod();
			if (forThisWeek)
			{
				period = DateHelper.GetWeekPeriod(date, CultureInfo.CurrentCulture);
			}

			var person = _user.CurrentUser();
			var timezone = person.PermissionInformation.DefaultTimeZone();
			var today = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timezone));

			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet == null)
				return null;

			var days = period.DayCollection();
			var availableDays = new List<DateOnly>();
			foreach (var day in days)
			{
				var overtimeRequestOpenPeriod =
					workflowControlSet.GetMergedOvertimeRequestOpenPeriod(
						day.ToDateTimePeriod(timezone), today, person.PermissionInformation);

				if (overtimeRequestOpenPeriod.AutoGrantType != Domain.WorkflowControl.OvertimeRequestAutoGrantType.Deny)
				{
					availableDays.Add(day);
				}
			}
			if (availableDays.Count == 0)
			{
				return null;
			}

			var availablePeriod = new DateOnlyPeriod(availableDays.Min(), availableDays.Max());
			return availablePeriod.Intersection(period);
		}
	}
}