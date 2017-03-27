using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
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
		private readonly IPersonRuleSetBagProvider _ruleSetBagProvider;

		public PreferenceFeedbackProvider(IWorkTimeMinMaxCalculator workTimeMinMaxCalculator, ILoggedOnUser loggedOnUser,
			IScheduleProvider scheduleProvider, IPreferenceNightRestChecker perferenceNightChecker,
			IPersonRuleSetBagProvider ruleSetBagProvider)
		{
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
			_loggedOnUser = loggedOnUser;
			_scheduleProvider = scheduleProvider;
			_perferenceNightChecker = perferenceNightChecker;
			_ruleSetBagProvider = ruleSetBagProvider;
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date, IScheduleDay scheduleDay)
		{
			var bag = _ruleSetBagProvider.ForDate(_loggedOnUser.CurrentUser(), date);
			return bag != null ? _workTimeMinMaxCalculator.WorkTimeMinMax(date, bag, scheduleDay) : null;
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date)
		{
			var scheduleDay = _scheduleProvider.GetScheduleForPeriod(new DateOnlyPeriod(date, date)) ?? new IScheduleDay[] { };
			return WorkTimeMinMaxForDate(date, scheduleDay.SingleOrDefault());
		}

		public PreferenceNightRestCheckResult CheckNightRestViolation(DateOnly date)
		{
			return _perferenceNightChecker.CheckNightRestViolation(_loggedOnUser.CurrentUser(), date);
		}
	}
}