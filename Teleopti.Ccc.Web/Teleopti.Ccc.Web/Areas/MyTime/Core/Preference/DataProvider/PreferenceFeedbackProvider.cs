using System.Linq;
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

		public IWorkTimeMinMax WorkTimeMinMaxForDate(DateOnly date)
		{
			var personPeriod = _loggedOnUser.CurrentUser().PersonPeriods(new DateOnlyPeriod(date, date)).SingleOrDefault();
			if (personPeriod == null) return null;
			var ruleSetBag = personPeriod.RuleSetBag;
			if (ruleSetBag == null) return null;
			return _workTimeMinMaxCalculator.WorkTimeMinMax(ruleSetBag, date);
		}
	}
}