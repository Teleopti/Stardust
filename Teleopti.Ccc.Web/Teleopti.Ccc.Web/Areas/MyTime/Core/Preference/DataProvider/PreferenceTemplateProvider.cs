using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceTemplateProvider : IPreferenceTemplateProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IExtendedPreferenceTemplateRepository _extendedPreferenceTemplateRepository;

		public PreferenceTemplateProvider(ILoggedOnUser loggedOnUser, IExtendedPreferenceTemplateRepository extendedPreferenceTemplateRepository)
		{
			_loggedOnUser = loggedOnUser;
			_extendedPreferenceTemplateRepository = extendedPreferenceTemplateRepository;
		}

		public IEnumerable<IExtendedPreferenceTemplate> RetrievePreferenceTemplates()
		{
			var user = _loggedOnUser.CurrentUser();
			return _extendedPreferenceTemplateRepository.FindByUser(user);
		}
	}
}