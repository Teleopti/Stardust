
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider
{
	public class NameFormatSettingsPersisterAndProvider : ISettingsPersisterAndProvider<NameFormatSettings>
	{
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
		private const string nameFormatKey = "NameFormatSettings";

		public NameFormatSettingsPersisterAndProvider(IPersonalSettingDataRepository personalSettingDataRepository)
		{
			_personalSettingDataRepository = personalSettingDataRepository;
		}

		public NameFormatSettings Persist(NameFormatSettings nameFormatSettings)
		{
			var setting = _personalSettingDataRepository.FindValueByKey(nameFormatKey, new NameFormatSettings());
			setting.NameFormatId = nameFormatSettings.NameFormatId;
			_personalSettingDataRepository.PersistSettingValue(setting);
			return setting;
		}

		public NameFormatSettings Get()
		{
			return _personalSettingDataRepository.FindValueByKey(nameFormatKey, new NameFormatSettings());
		}

		public NameFormatSettings GetByOwner(Interfaces.Domain.IPerson person)
		{
			return _personalSettingDataRepository.FindValueByKeyAndOwnerPerson(nameFormatKey, person, new NameFormatSettings());
		}
	}
}