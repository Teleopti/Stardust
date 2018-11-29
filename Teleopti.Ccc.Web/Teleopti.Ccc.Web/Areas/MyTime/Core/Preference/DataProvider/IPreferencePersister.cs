using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferencePersister
	{
		PreferenceDayViewModel Persist(PreferenceDayInput input);
		IDictionary<DateOnly, PreferenceDayViewModel> PersistMultiDays(MultiPreferenceDaysInput input);
		IDictionary<DateOnly, PreferenceDayViewModel> Delete (List<DateOnly> dates);
		bool MustHave(MustHaveInput input);
	}
}