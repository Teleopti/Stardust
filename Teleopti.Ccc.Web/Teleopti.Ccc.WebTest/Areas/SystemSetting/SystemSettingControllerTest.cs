using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.SystemSetting;

namespace Teleopti.Ccc.WebTest.Areas.SystemSetting
{
	[TestFixture]
	public class SystemSettingControllerTest
	{
		[Test]
		public void ShouldGetSystemSettingPermissionTrue()
		{
			var permissions = new FakePermissions();
			permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.SystemSetting);

			var target = new SystemSettingController(permissions);

			target.HasSystemSettingPermission().Should().Be.True();
		}

		[Test]
		public void ShouldGetSystemSettingPermissionFalse()
		{
			var permissions = new FakePermissions();

			var target = new SystemSettingController(permissions);

			target.HasSystemSettingPermission().Should().Be.False();
		}
	}
}
