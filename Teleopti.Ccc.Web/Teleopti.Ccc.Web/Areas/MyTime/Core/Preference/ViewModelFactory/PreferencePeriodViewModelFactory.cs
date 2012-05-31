using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory
{
	public class PreferencePeriodViewModelFactory : IPreferencePeriodViewModelFactory
	{
		private readonly IPreferencePeriodFeedbackProvider _preferencePeriodFeedbackProvider;

		public PreferencePeriodViewModelFactory(IPreferencePeriodFeedbackProvider preferencePeriodFeedbackProvider) { _preferencePeriodFeedbackProvider = preferencePeriodFeedbackProvider; }

		public PreferencePeriodFeedbackViewModel CreatePeriodFeedbackViewModel(DateOnly date)
		{
			var targetDaysOff = _preferencePeriodFeedbackProvider.TargetDaysOff(date);
			var possibleResultDaysOff = _preferencePeriodFeedbackProvider.PossibleResultDaysOff(date);
			return new PreferencePeriodFeedbackViewModel
			       	{
			       		PossibleResultDaysOff = possibleResultDaysOff,
			       		TargetDaysOff = new TargetDaysOffViewModel()
			       		                	{
			       		                		Lower = targetDaysOff.Minimum,
			       		                		Upper = targetDaysOff.Maximum
			       		                	}
			       	};
		}
	}
}