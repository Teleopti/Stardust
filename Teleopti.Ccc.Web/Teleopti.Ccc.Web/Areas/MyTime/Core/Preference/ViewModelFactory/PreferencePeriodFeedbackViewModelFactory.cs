using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory
{
	public class PreferencePeriodFeedbackViewModelFactory : IPreferencePeriodFeedbackViewModelFactory
	{
		private readonly IPreferencePeriodFeedbackProvider _preferencePeriodFeedbackProvider;
		private readonly ITimeFormatter _timeFormatter;

		public PreferencePeriodFeedbackViewModelFactory(IPreferencePeriodFeedbackProvider preferencePeriodFeedbackProvider, ITimeFormatter timeFormatter)
		{
			_preferencePeriodFeedbackProvider = preferencePeriodFeedbackProvider;
			_timeFormatter = timeFormatter;
		}

		public PreferencePeriodFeedbackViewModel CreatePeriodFeedbackViewModel(DateOnly date)
		{
			var feedback = _preferencePeriodFeedbackProvider.PeriodFeedback(date);
			return new PreferencePeriodFeedbackViewModel
			       	{
			       		PossibleResultDaysOff = feedback.PossibleResultDaysOff,
			       		TargetDaysOff = new TargetDaysOffViewModel
			       		                	{
			       		                		Lower = feedback.TargetDaysOff.Minimum,
			       		                		Upper = feedback.TargetDaysOff.Maximum
			       		                	},
			       		TargetContractTime = new TargetContractTimeViewModel
			       		              	{
											Lower = _timeFormatter.GetLongHourMinuteTimeString(feedback.TargetTime.Minimum),
											Upper = _timeFormatter.GetLongHourMinuteTimeString(feedback.TargetTime.Maximum)
			       		              	}
			       	};
		}
	}
}