using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider
{
	public class StudentAvailabilityFeedbackProvider : IStudentAvailabilityFeedbackProvider
	{
		private readonly IWorkTimeMinMaxCalculator _workTimeMinMaxCalculator;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IPersonRuleSetBagProvider _personRuleSetBagProvider;

		public StudentAvailabilityFeedbackProvider(IWorkTimeMinMaxCalculator workTimeMinMaxCalculator,
			ILoggedOnUser loggedOnUser, IScheduleProvider scheduleProvider, IPersonRuleSetBagProvider personRuleSetBagProvider)
		{
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
			_loggedOnUser = loggedOnUser;
			_scheduleProvider = scheduleProvider;
			_personRuleSetBagProvider = personRuleSetBagProvider;
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date, IScheduleDay scheduleDay)
		{
			var bag = _personRuleSetBagProvider.ForDate(_loggedOnUser.CurrentUser(),date);
			if (bag == null) return null;

			return _workTimeMinMaxCalculator.WorkTimeMinMax(date, bag, scheduleDay,
				new EffectiveRestrictionOptions {UseStudentAvailability = true});
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date)
		{
			var period = new DateOnlyPeriod(date, date);
			var scheduleDay = _scheduleProvider.GetScheduleForStudentAvailability(period) ?? new IScheduleDay[] {};
			return WorkTimeMinMaxForDate(date, scheduleDay.SingleOrDefault());
		}
	}
}