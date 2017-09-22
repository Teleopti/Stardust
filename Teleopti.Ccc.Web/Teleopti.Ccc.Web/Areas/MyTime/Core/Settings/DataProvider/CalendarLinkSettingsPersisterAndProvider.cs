using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider
{
	public class CalendarLinkSettingsPersisterAndProvider : ISettingsPersisterAndProvider<CalendarLinkSettings>
	{
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
		private const string calendarLinkKey = "CalendarLinkSettings";

		public CalendarLinkSettingsPersisterAndProvider(IPersonalSettingDataRepository personalSettingDataRepository)
		{
			_personalSettingDataRepository = personalSettingDataRepository;
		}

		public CalendarLinkSettings Persist(CalendarLinkSettings calendarLinkSettings)
		{
			var setting = _personalSettingDataRepository.FindValueByKey(calendarLinkKey, new CalendarLinkSettings());
			setting.IsActive = calendarLinkSettings.IsActive;
			_personalSettingDataRepository.PersistSettingValue(setting);
			return setting;
		}

		public CalendarLinkSettings Get()
		{
			return _personalSettingDataRepository.FindValueByKey(calendarLinkKey, new CalendarLinkSettings());
		}

		public CalendarLinkSettings GetByOwner(IPerson person)
		{
			return _personalSettingDataRepository.FindValueByKeyAndOwnerPerson(calendarLinkKey, person, new CalendarLinkSettings());
		}

	}
}