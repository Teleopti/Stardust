using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceFeedbackProvider : IPreferenceFeedbackProvider
	{
		private readonly IWorkTimeMinMaxCalculator _workTimeMinMaxCalculator;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IPreferenceNightRestChecker _perferenceNightRestChecker;
		private readonly IPersonRuleSetBagProvider _ruleSetBagProvider;

		public PreferenceFeedbackProvider(IWorkTimeMinMaxCalculator workTimeMinMaxCalculator, ILoggedOnUser loggedOnUser,
			IScheduleProvider scheduleProvider, IPreferenceNightRestChecker perferenceNightChecker,
			IPersonRuleSetBagProvider ruleSetBagProvider)
		{
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
			_loggedOnUser = loggedOnUser;
			_scheduleProvider = scheduleProvider;
			_perferenceNightRestChecker = perferenceNightChecker;
			_ruleSetBagProvider = ruleSetBagProvider;
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date, IScheduleDay scheduleDay)
		{
			var ruleSetBag = _ruleSetBagProvider.ForDate(_loggedOnUser.CurrentUser(), date);
			return calculateWorkTimeMinMax(date, ruleSetBag, scheduleDay);
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date)
		{
			var scheduleDay = _scheduleProvider.GetScheduleForPeriod(date.ToDateOnlyPeriod()) ?? new IScheduleDay[] { };
			return WorkTimeMinMaxForDate(date, scheduleDay.SingleOrDefault());
		}

		public IDictionary<DateOnly, WorkTimeMinMaxCalculationResult> WorkTimeMinMaxForPeriod(DateOnlyPeriod period)
		{
			var result = new Dictionary<DateOnly, WorkTimeMinMaxCalculationResult>();

			var scheduleDays = _scheduleProvider.GetScheduleForPeriod(period).ToDictionary(d => d.DateOnlyAsPeriod.DateOnly);
			var ruleSetBags = _ruleSetBagProvider.ForPeriod(_loggedOnUser.CurrentUser(), period);

			foreach (var date in period.DayCollection())
			{
				IScheduleDay scheduleDay;
				scheduleDays.TryGetValue(date, out scheduleDay);
				var ruleSetBag = ruleSetBags[date];
				result.Add(date, calculateWorkTimeMinMax(date, ruleSetBag, scheduleDay));
			}

			return result;
		}

		public IDictionary<DateOnly, PreferenceNightRestCheckResult> CheckNightRestViolation(
			DateOnlyPeriod period, IDictionary<DateOnly, WorkTimeMinMaxCalculationResult> workTimeMinMaxCalculationResults)
		{
			return _perferenceNightRestChecker.CheckNightRestViolation(_loggedOnUser.CurrentUser(),
				period, workTimeMinMaxCalculationResults);
		}

		private WorkTimeMinMaxCalculationResult calculateWorkTimeMinMax(DateOnly date, IRuleSetBag ruleSetBag,
			IScheduleDay scheduleDay)
		{
			return ruleSetBag != null ? _workTimeMinMaxCalculator.WorkTimeMinMax(date, ruleSetBag, scheduleDay) : null;
		}
	}
}