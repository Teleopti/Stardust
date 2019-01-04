using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceFeedbackProvider
	{
		private readonly IWorkTimeMinMaxCalculator _workTimeMinMaxCalculator;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IPreferenceNightRestChecker _perferenceNightRestChecker;
		private readonly IPersonRuleSetBagProvider _ruleSetBagProvider;

		public PreferenceFeedbackProvider(IWorkTimeMinMaxCalculator workTimeMinMaxCalculator,
			IScheduleProvider scheduleProvider, IPreferenceNightRestChecker perferenceNightChecker,
			IPersonRuleSetBagProvider ruleSetBagProvider)
		{
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
			_scheduleProvider = scheduleProvider;
			_perferenceNightRestChecker = perferenceNightChecker;
			_ruleSetBagProvider = ruleSetBagProvider;
		}
		
		public IDictionary<DateOnly, WorkTimeMinMaxCalculationResult> WorkTimeMinMaxForPeriod(IPerson currentUser, DateOnlyPeriod period)
		{
			var scheduleDays = _scheduleProvider.GetScheduleForPeriod(period).ToDictionary(d => d.DateOnlyAsPeriod.DateOnly);
			var ruleSetBags = _ruleSetBagProvider.ForPeriod(currentUser, period);

			return period.DayCollection().ToDictionary(k => k, date =>
			{
				scheduleDays.TryGetValue(date, out var scheduleDay);
				return calculateWorkTimeMinMax(date, ruleSetBags[date], scheduleDay);
			});
		}

		public IDictionary<DateOnly, PreferenceNightRestCheckResult> CheckNightRestViolation(IPerson currentUser, DateOnlyPeriod period, IDictionary<DateOnly, WorkTimeMinMaxCalculationResult> workTimeMinMaxCalculationResults)
		{
			return _perferenceNightRestChecker.CheckNightRestViolation(currentUser,
				period, workTimeMinMaxCalculationResults);
		}

		private WorkTimeMinMaxCalculationResult calculateWorkTimeMinMax(DateOnly date, IRuleSetBag ruleSetBag,
			IScheduleDay scheduleDay)
		{
			return ruleSetBag != null ? _workTimeMinMaxCalculator.WorkTimeMinMax(date, ruleSetBag, scheduleDay) : null;
		}
	}
}