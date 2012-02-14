using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferencePersister
	{
		PreferenceDayInputResult Persist(PreferenceDayInput input);
		PreferenceDayInputResult Delete(DateOnly date);
	}
}