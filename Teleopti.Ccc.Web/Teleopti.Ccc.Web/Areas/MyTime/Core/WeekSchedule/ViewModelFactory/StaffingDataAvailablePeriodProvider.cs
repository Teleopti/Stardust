using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
			var today =
				new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(),
					_user.CurrentUser().PermissionInformation.DefaultTimeZone()));
			var maxEndDate = today;
			if (_toggleManager.IsEnabled(Domain.FeatureFlags.Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880))
			{
				maxEndDate = today.AddDays(ScheduleStaffingPossibilityConsts.MaxAvailableDays);
			}
			var availablePeriod = new DateOnlyPeriod(today, maxEndDate);
			return availablePeriod.Intersection(period);
		}
	}
}