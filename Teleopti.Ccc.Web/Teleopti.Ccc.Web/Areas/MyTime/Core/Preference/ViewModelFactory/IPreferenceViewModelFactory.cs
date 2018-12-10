using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory
{
	public interface IPreferenceViewModelFactory
	{
		PreferenceViewModel CreateViewModel(DateOnly date);

		PreferenceDayFeedbackViewModel CreateDayFeedbackViewModel(DateOnly date);

		IEnumerable<PreferenceDayFeedbackViewModel> CreateDayFeedbackViewModel(DateOnlyPeriod period);

		PreferenceDayViewModel CreateDayViewModel(DateOnly today);

		IEnumerable<PreferenceAndScheduleDayViewModel> CreatePreferencesAndSchedulesViewModel(DateOnly @from, DateOnly to);

		IEnumerable<PreferenceTemplateViewModel> CreatePreferenceTemplateViewModels();

		PreferenceWeeklyWorkTimeViewModel CreatePreferenceWeeklyWorkTimeViewModel(DateOnly date);
		IDictionary<DateOnly, PreferenceWeeklyWorkTimeViewModel> CreatePreferenceWeeklyWorkTimeViewModels(IEnumerable<DateOnly> dates);
	}
}