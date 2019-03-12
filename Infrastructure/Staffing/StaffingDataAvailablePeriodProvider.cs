using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common.Time;
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

		public DateOnlyPeriod? GetPeriodForAbsenceForWeek(IPerson person, DateOnly date)
		{
			var weekPeriod = DateHelper.GetWeekPeriod(date, CultureInfo.CurrentCulture);

			return getStaffingDataAvailablePeriod(person).Intersection(weekPeriod);
		}

		public DateOnlyPeriod? GetPeriodForAbsenceForMobileDay(IPerson person, DateOnly date)
		{
			return getStaffingDataAvailablePeriod(person).Intersection(date.ToDateOnlyPeriod());
		}

		public List<DateOnlyPeriod> GetPeriodsForOvertimeForWeek(IPerson person, DateOnly date)
		{
			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet == null)
				return new List<DateOnlyPeriod>();

			var weekPeriod  = DateHelper.GetWeekPeriod(date, CultureInfo.CurrentCulture);

			if (workflowControlSet.OvertimeRequestOpenPeriods.Count == 0)
			{
				var staffingDataAvailablePeriod = getStaffingDataAvailablePeriod(person).Intersection(weekPeriod);
				return staffingDataAvailablePeriod.HasValue
					? new List<DateOnlyPeriod> { staffingDataAvailablePeriod.Value }
					: new List<DateOnlyPeriod>();
			}

			var days = weekPeriod.DayCollection();
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
				var intersectionPeriod = availablePeriod.Intersection(weekPeriod);
				if (intersectionPeriod.HasValue)
				{
					returnPeriods.Add(intersectionPeriod.Value);
				}
			}

			return returnPeriods;
		}

		public List<DateOnlyPeriod> GetPeriodsForOvertimeForMobileDay(IPerson person, DateOnly date)
		{
			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet == null)
				return new List<DateOnlyPeriod>();

			if (workflowControlSet.OvertimeRequestOpenPeriods.Count == 0)
			{
				var staffingDataAvailablePeriod = getStaffingDataAvailablePeriod(person);
				return new List<DateOnlyPeriod> {staffingDataAvailablePeriod};
			}

			var days = date.ToDateOnlyPeriod().DayCollection();
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
				var intersectionPeriod = availablePeriod.Intersection(date.ToDateOnlyPeriod());
				if (intersectionPeriod.HasValue)
				{
					returnPeriods.Add(intersectionPeriod.Value);
				}
			}

			return returnPeriods;
		}

		private DateOnlyPeriod getStaffingDataAvailablePeriod(IPerson person)
		{
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var today = _now.CurrentLocalDate(timeZone);

			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(_toggleManager);
			var maxEndDate = today.AddDays(staffingInfoAvailableDays);

			var availablePeriod = new DateOnlyPeriod(today, maxEndDate);
			return availablePeriod;
		}
	}
}