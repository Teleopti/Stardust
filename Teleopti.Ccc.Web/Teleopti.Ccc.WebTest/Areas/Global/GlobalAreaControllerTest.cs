using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class GlobalAreaControllerTest
	{
		[Test]
		public void ShouldReturnEmptyWithToggleOff()
		{
			var toggleManager = new FakeToggleManager();
			var areaPathProvider = new AreaWithPermissionPathProvider(new FakePermissionProvider(), toggleManager, new FakeLicenseActivatorProvider());
			var target = new GlobalAreaController(areaPathProvider, toggleManager);

			var result = target.GetApplicationAreas();
			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotGetAnyAreaWithPermissionsOff()
		{
			var toggleManager = new TrueToggleManager();
			var areaPathProvider = new AreaWithPermissionPathProvider(new FakeNoPermissionProvider(), toggleManager, new FakeLicenseActivatorProvider());
			var target = new GlobalAreaController(areaPathProvider, toggleManager);

			var result = target.GetApplicationAreas();
			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldGetMessagesAreaWithLicenseForSMSLinkOnly()
		{
			var toggleManager = new TrueToggleManager();
			var licenseActivator = new FakeLicenseActivator();
			licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
			var areaPathProvider = new AreaWithPermissionPathProvider(new FakeNoPermissionProvider(), toggleManager, new FakeLicenseActivatorProvider(licenseActivator));
			var target = new GlobalAreaController(areaPathProvider, toggleManager);

			var result = target.GetApplicationAreas();
			result.Count().Should().Be(1);
		}
	}
}