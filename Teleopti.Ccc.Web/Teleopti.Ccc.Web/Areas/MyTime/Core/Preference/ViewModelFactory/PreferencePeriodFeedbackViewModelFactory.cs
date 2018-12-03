using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core.Extensions;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory
{
	public class PreferencePeriodFeedbackViewModelFactory : IPreferencePeriodFeedbackViewModelFactory
	{
		private readonly IPreferencePeriodFeedbackProvider _preferencePeriodFeedbackProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public PreferencePeriodFeedbackViewModelFactory(IPreferencePeriodFeedbackProvider preferencePeriodFeedbackProvider, ILoggedOnUser loggedOnUser)
		{
			_preferencePeriodFeedbackProvider = preferencePeriodFeedbackProvider;
			_loggedOnUser = loggedOnUser;
		}

		public PreferencePeriodFeedbackViewModel CreatePeriodFeedbackViewModel(DateOnly date)
		{
			var feedback = _preferencePeriodFeedbackProvider.PeriodFeedback(date);
			
			var result = new PreferencePeriodFeedbackViewModel
				{
					PossibleResultDaysOff = feedback.PossibleResultDaysOff,
					TargetDaysOff = new TargetDaysOffViewModel
						{
							Lower = feedback.TargetDaysOff.Minimum,
							Upper = feedback.TargetDaysOff.Maximum
						},
					TargetContractTime = new TargetContractTimeViewModel
						{
							LowerMinutes = feedback.TargetTime.StartTime.TotalMinutes,
							UpperMinutes = feedback.TargetTime.EndTime.TotalMinutes
						},
					
			};
			var wfc = _loggedOnUser.CurrentUser().WorkflowControlSet;
			if (wfc != null)
			{
				result.PreferencePeriodStart = wfc.PreferencePeriod.StartDate.Date.ToFixedDateFormat();
				result.PreferencePeriodEnd = wfc.PreferencePeriod.EndDate.Date.ToFixedDateFormat();
				result.PreferenceOpenPeriodStart = wfc.PreferenceInputPeriod.StartDate.Date.ToFixedDateFormat();
				result.PreferenceOpenPeriodEnd = wfc.PreferenceInputPeriod.EndDate.Date.ToFixedDateFormat();
			}
			return result;
		}
	}
}