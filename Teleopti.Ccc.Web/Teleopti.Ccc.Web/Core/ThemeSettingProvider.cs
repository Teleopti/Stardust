using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;

namespace Teleopti.Ccc.Web.Core
{
	[Serializable]
	public class ThemeSetting : SettingValue
	{
		public string Name { get; set; }
		public bool Overlay { get; set; }
	}

	public class ThemeSettingProvider : ISettingsPersisterAndProvider<ThemeSetting>
	{
		private readonly IPersonalSettingDataRepository _settingDataRepository;
		public const string Key = "ThemeSetting";

		public ThemeSettingProvider(IPersonalSettingDataRepository settingDataRepository)
		{
			_settingDataRepository = settingDataRepository;
		}

		public ThemeSetting Persist(ThemeSetting themeSetting)
		{
			var setting = _settingDataRepository.FindValueByKey(Key, new ThemeSetting());
			setting.Name = themeSetting.Name;
			setting.Overlay = themeSetting.Overlay;
			_settingDataRepository.PersistSettingValue(setting);
			return setting;
		}

		public ThemeSetting Get()
		{
			return _settingDataRepository.FindValueByKey(Key, new ThemeSetting());
		}

		public ThemeSetting GetByOwner(IPerson person)
		{
			return _settingDataRepository.FindValueByKeyAndOwnerPerson(Key, person, new ThemeSetting());
		}
	}
}