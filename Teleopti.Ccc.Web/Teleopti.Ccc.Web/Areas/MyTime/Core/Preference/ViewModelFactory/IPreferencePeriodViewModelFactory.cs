using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory
{
	public interface IPreferencePeriodViewModelFactory
	{
		PreferencePeriodFeedbackViewModel CreatePeriodFeedbackViewModel(DateOnly date);
	}
}