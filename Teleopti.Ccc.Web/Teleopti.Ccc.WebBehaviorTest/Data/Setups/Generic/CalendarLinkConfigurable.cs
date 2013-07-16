using System;
using System.Globalization;
using System.Reflection;
using System.Web;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class CalendarLinkConfigurable : IUserDataSetup
	{
		public bool IsActive { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var personalSettingDataRepository = new PersonalSettingDataRepository(uow);
			var setting = personalSettingDataRepository.FindValueByKey("CalendarLinkSettings", new CalendarLinkSettings());
			setting.IsActive = IsActive;
			setting.CalendarUrl = TestSiteConfigurationSetup.Url + "MyTime/Share?id=" +
			                      HttpUtility.UrlEncode(
									  StringEncryption.Encrypt("TestData" + "/" + user.Id.Value));
			var personalSettingData = new PersonalSettingData("CalendarLinkSettings", user);
			personalSettingData.SetValue(setting);
			var setOwnerMethod = typeof(CalendarLinkSettings).GetMethod("SetOwner", BindingFlags.Instance | BindingFlags.NonPublic,
			                                        Type.DefaultBinder, new[] {typeof (ISettingData)}, null);
			setOwnerMethod.Invoke(setting, new object[] {personalSettingData});
			personalSettingDataRepository.PersistSettingValue(setting);
		}
	}
}