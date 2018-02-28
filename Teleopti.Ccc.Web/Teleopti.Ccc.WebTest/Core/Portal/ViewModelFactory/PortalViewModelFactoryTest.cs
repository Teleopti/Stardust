using System;
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
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;
using Claim = System.IdentityModel.Claims.Claim;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[MyTimeWebTest]
	[TestFixture]
	public class PortalViewModelFactoryTest : ISetup
	{
		public ICurrentDataSource CurrentDataSource;
		public IPortalViewModelFactory Target;
		public FakeCurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public CurrentTenantUserFake CurrentTenantUser;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<PrincipalAuthorization>().For<IAuthorization>();
			system.UseTestDouble<PermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			system.UseTestDouble(new FakeCurrentUnitOfWorkFactory(null).WithCurrent(new FakeUnitOfWorkFactory(null, null, null, null) { Name = MyTimeWebTestAttribute.DefaultTenantName })).For<ICurrentUnitOfWorkFactory>();
		}

		[Test]
		public void ShouldHaveNavigationItems()
		{
			setPermissions();

			var result = Target.CreatePortalViewModel();

			result.NavigationItems.Should().Have.Count.EqualTo(7);
			result.NavigationItems.ElementAt(0).Action.Should().Be("Week");
			result.NavigationItems.ElementAt(0).Controller.Should().Be("Schedule");
			result.NavigationItems.ElementAt(1).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(1).Controller.Should().Be("TeamSchedule");
			result.NavigationItems.ElementAt(2).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(2).Controller.Should().Be("Availability");
			result.NavigationItems.ElementAt(3).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(3).Controller.Should().Be("Preference");
			result.NavigationItems.ElementAt(4).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(4).Controller.Should().Be("Requests");
			result.NavigationItems.ElementAt(5).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(5).Controller.Should().Be("Message");
			result.NavigationItems.ElementAt(6).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(6).Controller.Should().Be("MyReport");
		}

		[Test]
		public void ShouldHaveMessageNavigationItemsWhenHavingBothLicenseAndPermission()
		{
			setPermissions();

			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);
			var result = Target.CreatePortalViewModel();

			result.NavigationItems.Should().Have.Count.EqualTo(7);
			result.NavigationItems.ElementAt(5).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(5).Controller.Should().Be("Message");
		}

		[Test]
		public void ShouldHaveCustomerName()
		{
			var result = Target.CreatePortalViewModel();

			result.CustomerName.Should().Be.EqualTo("Teleopti_RD");
		}

		[Test]
		public void ShouldHaveLogonAgentName()
		{
			var result = Target.CreatePortalViewModel();

			result.CurrentLogonAgentName.Should().Be.EqualTo("arne arne");
		}

		[Test]
		public void ShouldHideChangePasswordIfNoApplicationLogonExists()
		{
			var res = Target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.False();
		}

		[Test]
		public void ShouldShowChangePasswordIfApplicationLogonExists()
		{
			var tenant = new Tenant("Tenn");
			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "Perra", "passadej", new OneWayEncryption());
			CurrentTenantUser.Set(personInfo);

			var res = Target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.True();
		}

		[Test]
		public void ShouldShowAsmIfPermissionToShowAsm()
		{
			setPermissions();
			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var res = Target.CreatePortalViewModel();

			Assert.That(res.HasAsmPermission, Is.True);
		}

		[Test]
		public void ShouldNotShowAsmIfNoPermission()
		{
			setPermissions(false);

			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var res = Target.CreatePortalViewModel();

			Assert.That(res.HasAsmPermission, Is.False);
		}

		[Test]
		public void ShouldNotShowBadgeIfNoPermission()
		{
			Assert.That(Target.CreatePortalViewModel().ShowBadge, Is.False);
		}

		[Test, SetCulture("en-US")]
		public void ShouldShowMeridianWhenUsCulture()
		{
			Target.CreatePortalViewModel().ShowMeridian.Should().Be.True();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldShowMeridianWhenSwedishCulture()
		{
			Target.CreatePortalViewModel().ShowMeridian.Should().Be.False();
		}

		[Test, SetCulture("en-GB")]
		public void ShouldShowMeridianWhenBritishCulture()
		{
			Target.CreatePortalViewModel().ShowMeridian.Should().Be.False();
		}

		private static void setPermissions(bool asmPermission = true)
		{
			var principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
			var claims = new List<Claim>
			{
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.MyReportWeb)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.TeamSchedule)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.StudentAvailability)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.StandardPreferences)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.TextRequests)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(
					string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
						"/AvailableData"), new AuthorizeEveryone(), Rights.PossessProperty)
			};

			if (asmPermission)
				claims.Add(new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)
					, new AuthorizeEveryone(), Rights.PossessProperty));

			principal.AddClaimSet(new DefaultClaimSet(ClaimSet.System, claims));
		}
	}
}