using System;
using System.Globalization;
using System.Reflection;
using System.Web;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class CalendarLinkConfigurable : IUserDataSetup
	{
		public bool IsActive { get; set; }

		public string SharingUrl { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var personalSettingDataRepository = PersonalSettingDataRepository.DONT_USE_CTOR(unitOfWork);
			var setting = personalSettingDataRepository.FindValueByKey("CalendarLinkSettings", new CalendarLinkSettings());
			setting.IsActive = IsActive;
			SharingUrl = TestSiteConfigurationSetup.URL + "MyTime/Share?id=" +
								  HttpServerUtility.UrlTokenEncode(
									  Encryption.EncryptStringToBytes("TestData" + "/" + person.Id.Value, EncryptionConstants.Image1, EncryptionConstants.Image2)) + "&type=text/plain";

			var personalSettingData = new PersonalSettingData("CalendarLinkSettings", person);
			personalSettingData.SetValue(setting);
			var setOwnerMethod = typeof(CalendarLinkSettings).GetMethod("SetOwner", BindingFlags.Instance | BindingFlags.NonPublic,
			                                        Type.DefaultBinder, new[] {typeof (ISettingData)}, null);
			setOwnerMethod.Invoke(setting, new object[] {personalSettingData});
			personalSettingDataRepository.PersistSettingValue(setting);
		}
	}
}