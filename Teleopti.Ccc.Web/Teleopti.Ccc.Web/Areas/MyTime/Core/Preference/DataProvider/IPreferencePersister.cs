using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferencePersister
	{
		PreferenceDayViewModel Persist(PreferenceDayInput input);
		IEnumerable<PreferenceDayViewModel> Delete (List<DateOnly> dates);
		bool MustHave(MustHaveInput input);
	}
}