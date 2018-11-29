using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceProvider : IPreferenceProvider
	{
		private readonly IPreferenceDayRepository _preferenceDayRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public PreferenceProvider(IPreferenceDayRepository preferenceDayRepository, ILoggedOnUser loggedOnUser)
		{
			_preferenceDayRepository = preferenceDayRepository;
			_loggedOnUser = loggedOnUser;
		}

		public IPreferenceDay GetPreferencesForDate(DateOnly date)
		{
			var user = _loggedOnUser.CurrentUser();
			return _preferenceDayRepository.Find(date, user).SingleOrDefault();
		}

		public IEnumerable<IPreferenceDay> GetPreferencesForPeriod(DateOnlyPeriod period)
		{
			var user = _loggedOnUser.CurrentUser();
			return _preferenceDayRepository.Find(period, new List<IPerson>() {user});
		}
	}
}