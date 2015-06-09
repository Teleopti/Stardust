using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	class FakePreferenceProvider : IPreferenceProvider
	{
		private readonly IPreferenceDay[] _preferenceDays;

		public FakePreferenceProvider(params IPreferenceDay[] preferenceDays)
		{
			_preferenceDays = preferenceDays;
		}

		public IPreferenceDay GetPreferencesForDate(DateOnly date)
		{
			return _preferenceDays.FirstOrDefault(pd => pd.RestrictionDate == date);
		}

		public IEnumerable<IPreferenceDay> GetPreferencesForPeriod(DateOnlyPeriod period)
		{
			return _preferenceDays.Where(pd => period.Contains(pd.RestrictionDate));
		}		
	}
}
