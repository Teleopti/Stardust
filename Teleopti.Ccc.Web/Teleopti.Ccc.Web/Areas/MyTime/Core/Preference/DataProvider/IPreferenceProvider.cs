using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferenceProvider
	{
		IPreferenceDay GetPreferencesForDate(DateOnly date);
		IEnumerable<IPreferenceDay> GetPreferencesForPeriod(DateOnlyPeriod period);
	}
}