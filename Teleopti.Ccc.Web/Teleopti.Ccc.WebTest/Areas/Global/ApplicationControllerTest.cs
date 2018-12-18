using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.WebTest.Areas.Permissions;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class ApplicationControllerTest
	{
		[Test]
		public void ShouldNotGetUnpermittedModules()
		{
			var toggleManager = new FakeToggleManager(Toggles.Wfm_WebPlan_Pilot_46815);
			var areaPathProvider = new AreaWithPermissionPathProvider(new FakeNoPermissionProvider(), toggleManager, new FakeLicenseActivatorProvider(), new FakeApplicationFunctionsToggleFilter());
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var target = new ApplicationController(areaPathProvider, globalSettingDataRepository);

			var result = target.GetWfmAreasWithPermission();
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetEnabledModules()
		{
			var toggleManager = new FakeToggleManager();
			var areaPathProvider = new AreaWithPermissionPathProvider(new FakeNoPermissionProvider(), toggleManager, new FakeLicenseActivatorProvider(), new FakeApplicationFunctionsToggleFilter());
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var target = new ApplicationController(areaPathProvider, globalSettingDataRepository);

			var result = target.GetWfmAreasWithPermission();
			result.Any(x=>((dynamic)(x)).InternalName == "resourceplanner" ).Should().Be.False();
		}

		[Test]
		public void ShouldGetWfmAreasList()
		{
			var toggleManager = new FakeToggleManager();
			var areaPathProvider = new AreaWithPermissionPathProvider(new FakeNoPermissionProvider(), toggleManager, new FakeLicenseActivatorProvider(), new FakeApplicationFunctionsToggleFilter());
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var target = new ApplicationController(areaPathProvider, globalSettingDataRepository);

			var results = target.GetWfmAreasList();

			results.Any(x => ((dynamic)(x)).InternalName == "forecast").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "resourceplanner").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "permissions").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "outbound").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "people").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "requests").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "seatPlan").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "seatMap").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "rta").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "intraday").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "teams").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "reports").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "staffingModule").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "myTime").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "gamification").Should().Be.True();
			results.Any(x => ((dynamic)(x)).InternalName == "systemSettings").Should().Be.True();
		}

		[Test]
		public void ShouldGetSupportEmailSetting()
		{
			var setting = new StringSetting {StringValue = "test@test.fr"};
			var toggleManager = new FakeToggleManager();
			var areaPathProvider = new AreaWithPermissionPathProvider(new FakeNoPermissionProvider(), toggleManager, new FakeLicenseActivatorProvider(), new FakeApplicationFunctionsToggleFilter());
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			globalSettingDataRepository.PersistSettingValue("SupportEmailSetting", setting);
			var target = new ApplicationController(areaPathProvider, globalSettingDataRepository);

			dynamic result = target.GetSupportEmailSetting();
			var emailSetting = (String)result.Content;
			emailSetting.Should().Not.Be.Null();
			emailSetting.Should().Be("test@test.fr");
		}
	}
}
