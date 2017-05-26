
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider
{
	public class NameFormatSettingsPersisterAndProvider : ISettingsPersisterAndProvider<NameFormatSettings>
	{
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
		
		public NameFormatSettingsPersisterAndProvider(IPersonalSettingDataRepository personalSettingDataRepository)
		{
			_personalSettingDataRepository = personalSettingDataRepository;
		}

		public NameFormatSettings Persist(NameFormatSettings nameFormatSettings)
		{
			var setting = _personalSettingDataRepository.FindValueByKey(NameFormatSettings.Key, new NameFormatSettings());
			setting.NameFormatId = nameFormatSettings.NameFormatId;
			_personalSettingDataRepository.PersistSettingValue(NameFormatSettings.Key, setting);
			return setting;
		}

		public NameFormatSettings Get()
		{
			return _personalSettingDataRepository.FindValueByKey(NameFormatSettings.Key, new NameFormatSettings());
		}

		public NameFormatSettings GetByOwner(IPerson person)
		{
			return _personalSettingDataRepository.FindValueByKeyAndOwnerPerson(NameFormatSettings.Key , person, new NameFormatSettings());
		}
	}
}