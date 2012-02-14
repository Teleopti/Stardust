using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceProvider : IPreferenceProvider
	{
		private readonly IScheduleProvider _scheduleProvider;

		public PreferenceProvider(IScheduleProvider scheduleProvider) {
			_scheduleProvider = scheduleProvider;
		}

		public IEnumerable<IPreferenceDay> GetPreferencesForPeriod(DateOnlyPeriod period)
		{
			return from d in _scheduleProvider.GetScheduleForPeriod(period)
			       from r in d.PersonRestrictionCollection().OfType<IPreferenceDay>()
			       select r;
		}
	}
}