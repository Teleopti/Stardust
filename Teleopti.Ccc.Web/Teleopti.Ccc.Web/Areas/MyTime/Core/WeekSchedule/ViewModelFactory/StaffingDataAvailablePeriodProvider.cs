using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
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

		public DateOnlyPeriod? GetPeriod(DateOnly date, bool forThisWeek)
		{
			var period = date.ToDateOnlyPeriod();
			if (forThisWeek)
			{
				period = DateHelper.GetWeekPeriod(date, CultureInfo.CurrentCulture);
			}
			
			var userTimezone = _user.CurrentUser().PermissionInformation.DefaultTimeZone();
			var userToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), userTimezone));
			var maxEndDate = userToday;
			if (_toggleManager.IsEnabled(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880))
			{
				maxEndDate = userToday.AddDays(ScheduleStaffingPossibilityConsts.MaxAvailableDays);
				var endOfAbsenceOpenPeriod = getEndOfAbsenceRequestOpenPeriod(userToday);
				if (endOfAbsenceOpenPeriod != null && endOfAbsenceOpenPeriod < maxEndDate)
				{
					maxEndDate = endOfAbsenceOpenPeriod.Value;
				}
			}

			var availablePeriod = new DateOnlyPeriod(userToday, maxEndDate);
			return availablePeriod.Intersection(period);
		}

		private DateOnly? getEndOfAbsenceRequestOpenPeriod(DateOnly userToday)
		{
			var absenceOpenPeriods = _user.CurrentUser().WorkflowControlSet?.AbsenceRequestOpenPeriods;
			if (absenceOpenPeriods == null || absenceOpenPeriods.Count == 0)
			{
				return null;
			}

			var periodAppliedForToday = absenceOpenPeriods.Where(p => p.OpenForRequestsPeriod.Contains(userToday))
				.OrderByDescending(p => p.OrderIndex).FirstOrDefault();
			if (periodAppliedForToday == null || !isValidOpenPeriod(periodAppliedForToday))
			{
				return null;
			}

			return periodAppliedForToday.GetPeriod(userToday).EndDate;
		}

		private static bool isValidOpenPeriod(IAbsenceRequestOpenPeriod openPeriod)
		{
			var validateStaffingWithBudgetGroup = openPeriod.StaffingThresholdValidator is StaffingThresholdWithShrinkageValidator ||
												  openPeriod.StaffingThresholdValidator is StaffingThresholdValidator;
			var isAutoDeny = openPeriod.AbsenceRequestProcess is DenyAbsenceRequest;
			return validateStaffingWithBudgetGroup && !isAutoDeny;
		}
	}
}