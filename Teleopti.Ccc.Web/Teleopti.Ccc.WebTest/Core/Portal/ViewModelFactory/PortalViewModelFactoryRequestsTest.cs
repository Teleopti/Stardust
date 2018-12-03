using System.Collections.Generic;
using System.IdentityModel.Claims;
using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.WebTest.Core.IoC;

using Claim = System.IdentityModel.Claims.Claim;
namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[MyTimeWebTest]
	[TestFixture]
	public class PortalViewModelFactoryRequestsTest : IIsolateSystem
	{
		public IPortalViewModelFactory Target;
		public ICurrentDataSource CurrentDataSource;
		public FakeLoggedOnUser LoggedOnUser;
		public CurrentTenantUserFake CurrentTenantUser;
		public FakeUserCulture UserCulture;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PrincipalAuthorization>().For<IAuthorization>();
			isolate.UseTestDouble<PermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			isolate.UseTestDouble(new FakeCurrentUnitOfWorkFactory(null).WithCurrent(new FakeUnitOfWorkFactory(null, null, null) { Name = MyTimeWebTestAttribute.DefaultTenantName })).For<ICurrentUnitOfWorkFactory>();
		}
		[Test]
		public void ShouldCreateRequestTabIfOnlyPermissionsToTextRequest()
		{
			setTextRequestsPermissions();

			TestThatRequestTabIsCreated();
		}

		[Test]
		public void ShouldCreateRequestTabIfOnlyPermissionsToAbsenceRequest()
		{
			setAbsenceRequestsWebPermissions();

			TestThatRequestTabIsCreated();
		}
		
		[Test]
		public void ShouldCreateRequestTabIfOnlyPermissionsToShiftTradeRequest()
		{
			setShiftTradeRequestsWebPermissions();

			TestThatRequestTabIsCreated();
		}

		[Test]
		public void ShouldCreateRequestTabIfOnlyPermissionsToOvertimeRequest()
		{
			setOvertimeRequestsWebPermissions();

			TestThatRequestTabIsCreated();
		}

		[Test]
		public void ShouldNotCreateRequestTabIfNoPermissionsToRequests()
		{
			var result = Target.CreatePortalViewModel();

			var requestTab = (from i in result.NavigationItems where i.Controller == "Requests" select i).SingleOrDefault();
			requestTab.Should().Be.Null();
		}

		private void setShiftTradeRequestsWebPermissions()
		{
			var principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
			var claims = new List<Claim>
			{
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(
					string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
						"/AvailableData"), new AuthorizeEveryone(), Rights.PossessProperty)
			};

			principal.AddClaimSet(new DefaultClaimSet(ClaimSet.System, claims));
		}

		private void setTextRequestsPermissions()
		{
			var principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
			var claims = new List<Claim>
			{
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.TextRequests)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(
					string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
						"/AvailableData"), new AuthorizeEveryone(), Rights.PossessProperty)
			};

			principal.AddClaimSet(new DefaultClaimSet(ClaimSet.System, claims));
		}

		private void setAbsenceRequestsWebPermissions()
		{
			var principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
			var claims = new List<Claim>
			{
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(
					string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
						"/AvailableData"), new AuthorizeEveryone(), Rights.PossessProperty)
			};

			principal.AddClaimSet(new DefaultClaimSet(ClaimSet.System, claims));
		}

		private void setOvertimeRequestsWebPermissions()
		{
			var principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
			var claims = new List<Claim>
			{
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(
					string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
						"/AvailableData"), new AuthorizeEveryone(), Rights.PossessProperty)
			};

			principal.AddClaimSet(new DefaultClaimSet(ClaimSet.System, claims));
		}

		private void TestThatRequestTabIsCreated()
		{
			var result =Target.CreatePortalViewModel();

			var requestTab = (from i in result.NavigationItems where i.Controller == "Requests" select i).SingleOrDefault();
			requestTab.Should().Not.Be.Null();
		}
	}
}