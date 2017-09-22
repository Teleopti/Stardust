using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class AgentsPerPageSettingPersisterAndProvider : ISettingsPersisterAndProvider<AgentsPerPageSetting>
	{
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
		private const string agentsPerPageSettingKey = "AgentsPerPage";

		public AgentsPerPageSettingPersisterAndProvider(IPersonalSettingDataRepository personalSettingDataRepository)
		{
			_personalSettingDataRepository = personalSettingDataRepository;
		}

		public AgentsPerPageSetting Persist(AgentsPerPageSetting agentsPerPageSetting)
		{
			var setting = _personalSettingDataRepository.FindValueByKey(agentsPerPageSettingKey, new AgentsPerPageSetting());
			setting.AgentsPerPage = agentsPerPageSetting.AgentsPerPage;
			_personalSettingDataRepository.PersistSettingValue(setting);
			return setting;
		}

		public AgentsPerPageSetting Get()
		{
			return _personalSettingDataRepository.FindValueByKey(agentsPerPageSettingKey, new AgentsPerPageSetting());
		}

		public AgentsPerPageSetting GetByOwner(IPerson person)
		{
			return _personalSettingDataRepository.FindValueByKeyAndOwnerPerson(agentsPerPageSettingKey, person, new AgentsPerPageSetting());
		}
	}
}