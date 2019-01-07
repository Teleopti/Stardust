using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Infrastructure.Staffing
{
	public class StaffingDataAvailablePeriodProvider : IStaffingDataAvailablePeriodProvider
	{
		private readonly INow _now;
		private readonly IToggleManager _toggleManager;
		private readonly IOvertimeRequestOpenPeriodProvider _mergedOvertimeRequestOpenPeriodProvider;

		public StaffingDataAvailablePeriodProvider(INow now, IToggleManager toggleManager,
			IOvertimeRequestOpenPeriodProvider mergedOvertimeRequestOpenPeriodProvider)
		{
			_now = now;
			_toggleManager = toggleManager;
			_mergedOvertimeRequestOpenPeriodProvider = mergedOvertimeRequestOpenPeriodProvider;
		}

		public DateOnlyPeriod? GetPeriodForAbsence(IPerson person, DateOnly date, bool forThisWeek)
		{
			return getStaffingDataAvailablePeriod(person, date, forThisWeek);
		}

		public List<DateOnlyPeriod> GetPeriodsForOvertime(IPerson person, DateOnly date, bool forThisWeek = false)
		{
			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet == null)
				return new List<DateOnlyPeriod>();

			if (workflowControlSet.OvertimeRequestOpenPeriods.Count == 0)
			{
				var staffingDataAvailablePeriod = getStaffingDataAvailablePeriod(person, date, forThisWeek);
				return staffingDataAvailablePeriod.HasValue
					? new List<DateOnlyPeriod> {staffingDataAvailablePeriod.Value}
					: new List<DateOnlyPeriod>();
			}

			var period = date.ToDateOnlyPeriod();
			if (forThisWeek)
			{
				period = DateHelper.GetWeekPeriod(date, CultureInfo.CurrentCulture);
			}
			
			var days = period.DayCollection();
			var availableDays = new List<DateOnly>();
			foreach (var day in days)
			{
				var overtimeRequestOpenPeriods =
					_mergedOvertimeRequestOpenPeriodProvider.GetOvertimeRequestOpenPeriods(person,
						day);

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

		private DateOnlyPeriod? getStaffingDataAvailablePeriod(IPerson person, DateOnly date, bool forThisWeek)
		{
			var period = date.ToDateOnlyPeriod();
			if (forThisWeek)
			{
				period = DateHelper.GetWeekPeriod(date, CultureInfo.CurrentCulture);
			}

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var today = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timeZone));

			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(_toggleManager);
			var maxEndDate = today.AddDays(staffingInfoAvailableDays);

			var availablePeriod = new DateOnlyPeriod(today, maxEndDate);
			return availablePeriod.Intersection(period);
		}
	}
}