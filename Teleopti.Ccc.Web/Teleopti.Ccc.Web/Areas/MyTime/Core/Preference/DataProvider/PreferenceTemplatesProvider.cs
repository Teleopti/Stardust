using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceTemplatesProvider : IPreferenceTemplatesProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IExtendedPreferenceTemplateRepository _extendedPreferenceTemplateRepository;

		public PreferenceTemplatesProvider(ILoggedOnUser loggedOnUser, IExtendedPreferenceTemplateRepository extendedPreferenceTemplateRepository)
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