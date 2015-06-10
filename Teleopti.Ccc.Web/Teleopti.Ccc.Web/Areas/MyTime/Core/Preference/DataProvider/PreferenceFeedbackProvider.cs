using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceFeedbackProvider : IPreferenceFeedbackProvider
	{
		private readonly IWorkTimeMinMaxCalculator _workTimeMinMaxCalculator;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IPreferenceNightRestChecker _perferenceNightChecker;


		public PreferenceFeedbackProvider(IWorkTimeMinMaxCalculator workTimeMinMaxCalculator, ILoggedOnUser loggedOnUser, IScheduleProvider scheduleProvider, IPreferenceNightRestChecker perferenceNightChecker)
		{
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
			_loggedOnUser = loggedOnUser;
			_scheduleProvider = scheduleProvider;
			_perferenceNightChecker = perferenceNightChecker;
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date, IScheduleDay scheduleDay)
		{
			return _workTimeMinMaxCalculator.WorkTimeMinMax(date, _loggedOnUser.CurrentUser(), scheduleDay);
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date)
		{
			var scheduleDay = _scheduleProvider.GetScheduleForPeriod(new DateOnlyPeriod(date, date)) ?? new IScheduleDay[] {};
			return WorkTimeMinMaxForDate(date, scheduleDay.SingleOrDefault());
		}

		public PreferenceNightRestCheckResult CheckNightRestViolation(DateOnly date)
		{
			return _perferenceNightChecker.CheckNightRestViolation(_loggedOnUser.CurrentUser(), date);
		}

	}
}