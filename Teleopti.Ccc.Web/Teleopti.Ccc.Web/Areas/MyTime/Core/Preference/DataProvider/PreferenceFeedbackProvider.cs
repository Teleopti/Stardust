using System.Collections.Concurrent;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceFeedbackProvider : IPreferenceFeedbackProvider
	{
		private readonly IWorkTimeMinMaxCalculator _workTimeMinMaxCalculator;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleProvider _scheduleProvider;

		public PreferenceFeedbackProvider(IWorkTimeMinMaxCalculator workTimeMinMaxCalculator, ILoggedOnUser loggedOnUser, IScheduleProvider scheduleProvider)
		{
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
			_loggedOnUser = loggedOnUser;
			_scheduleProvider = scheduleProvider;
		}

		public IWorkTimeMinMax WorkTimeMinMaxForDate(DateOnly date, IScheduleDay scheduleDay, out PreferenceType? preferenceType)
		{
			return _workTimeMinMaxCalculator.WorkTimeMinMax(date, _loggedOnUser.CurrentUser(), scheduleDay, out preferenceType);
		}

		public IWorkTimeMinMax WorkTimeMinMaxForDate(DateOnly date, out PreferenceType? preferenceType)
		{
			var scheduleDay = _scheduleProvider.GetScheduleForPeriod(new DateOnlyPeriod(date, date)) ?? new IScheduleDay[] {};
			return WorkTimeMinMaxForDate(date, scheduleDay.SingleOrDefault(),  out preferenceType);
		}
	}
}