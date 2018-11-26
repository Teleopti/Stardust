using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.Insights.Core
{
	[TestFixture]
	public class InsightsPermissionProviderTest
	{
		[Test]
		[FakePermissions]
		public void ShouldHaveInsightsPermissions()
		{
			var permissions = new FakePermissions();
			permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.ViewInsightsReport);
			permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.EditInsightsReport);
			var target = new PermissionProvider(permissions);

			var permission = target.GetInsightsPermission(null, null);
			permission.CanViewReport.Should().Be.True();
			permission.CanEditReport.Should().Be.True();
		}

		[Test]
		public void ShouldNotHaveInsightsPermissions()
		{
			var permissions = new FakePermissions();
			var target = new PermissionProvider(permissions);

			var permission = target.GetInsightsPermission(null, null);
			permission.CanViewReport.Should().Be.False();
			permission.CanEditReport.Should().Be.False();
		}
	}
}
