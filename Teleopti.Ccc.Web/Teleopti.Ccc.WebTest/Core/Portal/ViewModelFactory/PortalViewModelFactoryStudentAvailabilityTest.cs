using System.Collections.Generic;
using System.Globalization;
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
using Teleopti.Ccc.IocCommon;
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
	public class PortalViewModelFactoryStudentAvailabilityTest : IIsolateSystem
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
		public void ShouldNotHaveStudentAvailabilityNavigationItemIfNotPermission()
		{
			var result = Target.CreatePortalViewModel();

			var studentAvailability =
				(from i in result.NavigationItems where i.Controller == "Availability" select i).SingleOrDefault();
			studentAvailability.Should().Be.Null();
		}

		[Test]
		public void ShouldHaveStudentAvailabilityNavigationItemIfPermitted()
		{
			setPermissions();

			var result = Target.CreatePortalViewModel();

			var studentAvailability =
				(from i in result.NavigationItems where i.Controller == "Availability" select i).SingleOrDefault();
			studentAvailability.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotHaveStudentAvailabilityNavigationItemIfJalaliCalendarIsUsed()
		{
			UserCulture.Is(new CultureInfo("fa-IR"));

			var result = Target.CreatePortalViewModel();

			var studentAvailability =
				(from i in result.NavigationItems where i.Controller == "Availability" select i).SingleOrDefault();
			studentAvailability.Should().Be.Null();
		}

		private void setPermissions()
		{
			var principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
			var claims = new List<Claim>
			{
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.StudentAvailability)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(
					string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
						"/AvailableData"), new AuthorizeEveryone(), Rights.PossessProperty)
			};

			principal.AddClaimSet(new DefaultClaimSet(ClaimSet.System, claims));
		}
	}
}