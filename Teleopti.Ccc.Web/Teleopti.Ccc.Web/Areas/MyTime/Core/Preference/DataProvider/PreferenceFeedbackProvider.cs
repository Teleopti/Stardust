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
		private readonly IScenarioProvider _scenarioProvider;

		public PreferenceFeedbackProvider(IWorkTimeMinMaxCalculator workTimeMinMaxCalculator, ILoggedOnUser loggedOnUser, IScenarioProvider scenarioProvider)
		{
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
			_loggedOnUser = loggedOnUser;
			_scenarioProvider = scenarioProvider;
		}

		public PreferenceFeedbackProvider(IWorkTimeMinMaxCalculator workTimeMinMaxCalculator)
		{
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
		}

		public IWorkTimeMinMax WorkTimeMinMaxForDate(IScheduleDay scheduleDay)
		{
			return _workTimeMinMaxCalculator.WorkTimeMinMax(scheduleDay);
		}
	}
}