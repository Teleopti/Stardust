using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.DataProvider
{
	[TestFixture]
	public class ReportsProviderTest
	{
		//[Test]
		//public void ShouldReturnFalseIfHasNoReportsPermission()
		//{
		//	var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();

		//	principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath")).Return(false);

		//	var target = new PermissionProvider(principalAuthorization);

		//	var result = target.HasApplicationFunctionPermission("ApplicationFunctionPath");

		//	result.Should().Be(false);
		//}

		[Test]
		public void ShouldReturnMyReportWhenNoOtherReportsPermission()
		{
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath")).Return(false);

			var permissionProvider = new PermissionProvider(principalAuthorization);
			var target = new ReportsProvider(principalAuthorization);

			var result = target.HasReportsPermission(principalAuthorization);

			result.Should().Be(true);
		}

	}
}