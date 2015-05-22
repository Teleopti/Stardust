using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider
{
	public class StudentAvailabilityFeedbackProvider : IStudentAvailabilityFeedbackProvider
	{
		private readonly IWorkTimeMinMaxCalculator _workTimeMinMaxCalculator;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleProvider _scheduleProvider;

		public StudentAvailabilityFeedbackProvider(IWorkTimeMinMaxCalculator workTimeMinMaxCalculator,
			ILoggedOnUser loggedOnUser, IScheduleProvider scheduleProvider)
		{
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
			_loggedOnUser = loggedOnUser;
			_scheduleProvider = scheduleProvider;
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date, IScheduleDay scheduleDay)
		{
			return _workTimeMinMaxCalculator.WorkTimeMinMax(date, _loggedOnUser.CurrentUser(), scheduleDay,
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