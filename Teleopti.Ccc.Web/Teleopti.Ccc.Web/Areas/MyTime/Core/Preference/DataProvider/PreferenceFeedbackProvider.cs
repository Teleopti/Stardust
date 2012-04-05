using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceFeedbackProvider : IPreferenceFeedbackProvider
	{
		private readonly IWorkTimeMinMaxCalculator _workTimeMinMaxCalculator;
		private readonly ILoggedOnUser _loggedOnUser;

		public PreferenceFeedbackProvider(IWorkTimeMinMaxCalculator workTimeMinMaxCalculator, ILoggedOnUser loggedOnUser)
		{
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
			_loggedOnUser = loggedOnUser;
		}

		public IWorkTimeMinMax WorkTimeMinMaxForDate(DateOnly date, IScheduleDay scheduleDay)
		{
			return _workTimeMinMaxCalculator.WorkTimeMinMax(date, _loggedOnUser.CurrentUser(), scheduleDay);
		}
	}
}