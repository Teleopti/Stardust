using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class CalendarLinkConfigurable : IDataSetup
	{
		public bool IsActive { get; set; }
		public void Apply(IUnitOfWork uow)
		{
			var personalSettingDataRepository = new PersonalSettingDataRepository(uow);
			var setting = personalSettingDataRepository.FindValueByKey("CalendarLinkSettings", new CalendarLinkSettings());
			setting.IsActive = IsActive;
			personalSettingDataRepository.PersistSettingValue(setting);
		}
	}
}