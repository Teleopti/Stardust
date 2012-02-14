using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal
{
	[TestFixture]
	public class PermissionProviderTest
	{

		private static IPrincipalAuthorization SetupPrincipalAuthorization()
		{
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			var principalForTest = new TeleoptiPrincipalForTest(new TeleoptiIdentity("", null, null, null), new Person());
			principalForTest.SetPrincipalAuthorization(principalAuthorization);
			Thread.CurrentPrincipal = principalForTest;
			return principalAuthorization;
		}

		[Test]
		public void ShouldReturnFalseIfNoApplicationFunctionPermission()
		{
			var principalAuthorization = SetupPrincipalAuthorization();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath")).Return(false);

			var target = new PermissionProvider();

			var result = target.HasApplicationFunctionPermission("ApplicationFunctionPath");

			result.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueIfApplicationFunctionPermission()
		{
			var principalAuthorization = SetupPrincipalAuthorization();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath")).Return(true);

			var target = new PermissionProvider();

			var result = target.HasApplicationFunctionPermission("ApplicationFunctionPath");

			result.Should().Be(true);
		}

		[Test]
		public void ShouldReturnFalseIfNoPersonPermission()
		{
			var principalAuthorization = SetupPrincipalAuthorization();
			var person = new Person();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath", DateOnly.Today, person)).Return(false);

			var target = new PermissionProvider();

			var result = target.HasPersonPermission("ApplicationFunctionPath", DateOnly.Today, person);

			result.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueIfPersonPermission()
		{
			var principalAuthorization = SetupPrincipalAuthorization();
			var person = new Person();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath", DateOnly.Today, person)).Return(true);

			var target = new PermissionProvider();

			var result = target.HasPersonPermission("ApplicationFunctionPath", DateOnly.Today, person);

			result.Should().Be(true);
		}

		[Test]
		public void ShouldReturnFalseIfNoTeamPermission()
		{
			var principalAuthorization = SetupPrincipalAuthorization();
			var team = new Team();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath", DateOnly.Today, team)).Return(false);

			var target = new PermissionProvider();

			var result = target.HasTeamPermission("ApplicationFunctionPath", DateOnly.Today, team);

			result.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueIfTeamPermission()
		{
			var principalAuthorization = SetupPrincipalAuthorization();
			var team = new Team();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath", DateOnly.Today, team)).Return(true);

			var target = new PermissionProvider();

			var result = target.HasTeamPermission("ApplicationFunctionPath", DateOnly.Today, team);

			result.Should().Be(true);
		}

	}
}