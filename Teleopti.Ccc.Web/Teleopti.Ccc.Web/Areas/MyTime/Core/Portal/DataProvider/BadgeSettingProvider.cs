using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public class BadgeSettingProvider : IBadgeSettingProvider
	{
		private readonly IAgentBadgeSettingsRepository _repository;

		public BadgeSettingProvider(IAgentBadgeSettingsRepository repository)
		{
			_repository = repository;
		}

		public IAgentBadgeSettings GetBadgeSettings()
		{
			return _repository.GetSettings();
		}
	}
}