using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.WebTest.Areas.Permissions;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class GlobalAreaControllerTest
	{
		[Test]
		public void ShouldReturnAllAreas()
		{
			var applicationFunctionsToggleFilter = new FakeApplicationFunctionsToggleFilter();
			applicationFunctionsToggleFilter
				.AddFakeFunction(new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.SeatPlanner }
			, o => true);

			var areaPathProvider = new AreaWithPermissionPathProvider(new FakePermissionProvider(),
				new FakeToggleManager(), new FakeLicenseActivatorProvider(), applicationFunctionsToggleFilter);
			var target = new GlobalAreaController(areaPathProvider);

			var result = target.GetApplicationAreas();
			result.Count().Should().Be(5);
		}

		[Test]
		public void ShouldNotGetAnyAreaWithPermissionsOff()
		{
			var areaPathProvider = new AreaWithPermissionPathProvider(new FakeNoPermissionProvider(),
				new TrueToggleManager(), new FakeLicenseActivatorProvider(), new FakeApplicationFunctionsToggleFilter());
			var target = new GlobalAreaController(areaPathProvider);

			var result = target.GetApplicationAreas();
			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldGetMessagesAreaWithLicenseForSmsLinkOnly()
		{
			var licenseActivator = new FakeLicenseActivator();
			licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
			var areaPathProvider = new AreaWithPermissionPathProvider(new FakeNoPermissionProvider(),
				new TrueToggleManager(), new FakeLicenseActivatorProvider(licenseActivator), new FakeApplicationFunctionsToggleFilter());
			var target = new GlobalAreaController(areaPathProvider);

			var result = target.GetApplicationAreas();
			result.Count().Should().Be(1);
		}
	}
}