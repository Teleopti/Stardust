using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory
{
	public interface IPreferencePeriodFeedbackViewModelFactory
	{
		PreferencePeriodFeedbackViewModel CreatePeriodFeedbackViewModel(DateOnly date);
	}
}