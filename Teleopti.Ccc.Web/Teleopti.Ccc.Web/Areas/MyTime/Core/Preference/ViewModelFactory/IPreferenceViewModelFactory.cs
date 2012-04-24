using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory
{
	public interface IPreferenceViewModelFactory
	{
		PreferenceViewModel CreateViewModel(DateOnly date);
		PreferenceDayFeedbackViewModel CreateDayFeedbackViewModel(DateOnly date);
	}
}