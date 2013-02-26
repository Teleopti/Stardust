using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceTemplateOptionsProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IExtendedPreferenceTemplateRepository _extendedPreferenceTemplateRepository;

		public PreferenceTemplateOptionsProvider(ILoggedOnUser loggedOnUser, IExtendedPreferenceTemplateRepository extendedPreferenceTemplateRepository)
		{
			_loggedOnUser = loggedOnUser;
			_extendedPreferenceTemplateRepository = extendedPreferenceTemplateRepository;
		}

		public IEnumerable<IExtendedPreferenceTemplate> RetrievePreferenceTemplateOptions()
		{
			var user = _loggedOnUser.CurrentUser();
			return _extendedPreferenceTemplateRepository.FindByUser(user);
		}
	}
}