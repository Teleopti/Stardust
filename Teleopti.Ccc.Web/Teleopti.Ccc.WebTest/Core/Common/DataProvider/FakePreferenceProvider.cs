using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;


namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	class FakePreferenceProvider : IPreferenceProvider
	{
		private readonly List<IPreferenceDay> _preferenceDays;

		public FakePreferenceProvider(params IPreferenceDay[] preferenceDays)
		{
			_preferenceDays = new List<IPreferenceDay>();
			foreach (var preferenceDay in preferenceDays)
			{
				_preferenceDays.Add(preferenceDay);
			}
		}

		public IPreferenceDay GetPreferencesForDate(DateOnly date)
		{
			return _preferenceDays.FirstOrDefault(pd => pd.RestrictionDate == date);
		}

		public IEnumerable<IPreferenceDay> GetPreferencesForPeriod(DateOnlyPeriod period)
		{
			return _preferenceDays.Where(pd => period.Contains(pd.RestrictionDate));
		}

		public void AddPreference(IPreferenceDay pd)
		{
			_preferenceDays.Add(pd);
		}

	}
}
