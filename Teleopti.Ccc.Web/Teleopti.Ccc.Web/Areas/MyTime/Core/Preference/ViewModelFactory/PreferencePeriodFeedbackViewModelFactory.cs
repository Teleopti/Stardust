using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory
{
	public class PreferencePeriodFeedbackViewModelFactory : IPreferencePeriodFeedbackViewModelFactory
	{
		private readonly IPreferencePeriodFeedbackProvider _preferencePeriodFeedbackProvider;

		public PreferencePeriodFeedbackViewModelFactory(IPreferencePeriodFeedbackProvider preferencePeriodFeedbackProvider)
		{
			_preferencePeriodFeedbackProvider = preferencePeriodFeedbackProvider;
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
							LowerMinutes = feedback.TargetTime.Minimum.TotalMinutes,
							UpperMinutes = feedback.TargetTime.Maximum.TotalMinutes
						},
				};
		}
	}
}