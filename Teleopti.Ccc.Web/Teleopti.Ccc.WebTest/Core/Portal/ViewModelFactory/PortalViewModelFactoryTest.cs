using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryTest
	{
		private IReportsNavigationProvider _reportsNavProvider;
		private static IPersonNameProvider _personNameProvider;
		private static ILoggedOnUser _loggedOnUser;
		private static IUserCulture _userCulture;

		[SetUp]
		public void Setup()
		{
			_reportsNavProvider = MockRepository.GenerateMock<IReportsNavigationProvider>();

			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(new Person() {Name = new Name()});

			var culture = CultureInfo.GetCultureInfo("sv-SE");
			_userCulture = MockRepository.GenerateMock<IUserCulture>();
			_userCulture.Expect(x => x.GetCulture()).Return(culture);

			_personNameProvider = MockRepository.GenerateMock<IPersonNameProvider>();
			_personNameProvider.Stub(x => x.BuildNameFromSetting(_loggedOnUser.CurrentUser().Name)).Return("Agent Name");
		}

		[Test]
		public void ShouldHaveNavigationItems()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.Anything)).Return(true);
			_reportsNavProvider.Stub(x => x.GetNavigationItems())
				.Return(new[] {new ReportNavigationItem {Action = "Index", Controller = "MyReport", IsWebReport = true}});
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				_reportsNavProvider, MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>(), _personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>(), MockRepository.GenerateStub<ICurrentTenantUser>(),
				_userCulture);

			var result = target.CreatePortalViewModel();

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
		public void ShouldHaveCustomerName()
		{
			var licenseActivatorProvider = MockRepository.GenerateMock<ILicenseActivatorProvider>();
			var target = new PortalViewModelFactory(MockRepository.GenerateMock<IPermissionProvider>(), licenseActivatorProvider,
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				_reportsNavProvider, MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>(), _personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>(), MockRepository.GenerateStub<ICurrentTenantUser>(),
				_userCulture);

			var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();
			licenseActivator.Stub(x => x.CustomerName).Return("Customer Name");
			licenseActivatorProvider.Stub(x => x.Current()).Return(licenseActivator);

			var result = target.CreatePortalViewModel();

			result.CustomerName.Should().Be.EqualTo("Customer Name");
		}

		[Test]
		public void ShouldHaveLogonAgentName()
		{
			var target = new PortalViewModelFactory(MockRepository.GenerateMock<IPermissionProvider>(),
				MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				_reportsNavProvider, MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>(), _personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>(), MockRepository.GenerateStub<ICurrentTenantUser>(),
				_userCulture);

			var result = target.CreatePortalViewModel();

			result.CurrentLogonAgentName.Should().Be.EqualTo("Agent Name");
		}

		[Test]
		public void ShouldHideChangePasswordIfNoApplicationLogonExists()
		{
			var agent = new PersonInfo();
			var loggedOnUser = MockRepository.GenerateMock<ICurrentTenantUser>();
			loggedOnUser.Expect(m => m.CurrentUser()).Return(agent);
			var target = new PortalViewModelFactory(MockRepository.GenerateStub<IPermissionProvider>(),
				MockRepository.GenerateStub<ILicenseActivatorProvider>(), MockRepository.GenerateMock<IPushMessageProvider>(),
				_loggedOnUser, _reportsNavProvider, MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>(), _personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>(), loggedOnUser, _userCulture);

			var res = target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.False();
		}



		[Test]
		public void ShouldShowChangePasswordIfApplicationLogonExists()
		{
			var agent = new PersonInfo();
			agent.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make());
			var loggedOnUser = MockRepository.GenerateMock<ICurrentTenantUser>();
			loggedOnUser.Expect(m => m.CurrentUser()).Return(agent);
			var target = new PortalViewModelFactory(MockRepository.GenerateStub<IPermissionProvider>(),
				MockRepository.GenerateStub<ILicenseActivatorProvider>(), MockRepository.GenerateMock<IPushMessageProvider>(),
				_loggedOnUser, _reportsNavProvider, MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>(), _personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>(), loggedOnUser, _userCulture);

			var res = target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.True();
		}

		[Test]
		public void ShouldShowAsmIfPermissionToShowAsm()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Expect(
				p => p.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(true);

			var target = CreateTarget(permissionProvider);

			Assert.That(target.CreatePortalViewModel().HasAsmPermission, Is.True);
		}

		[Test]
		public void ShouldNotShowAsmIfNoPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Expect(
				p => p.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(false);

			var target = CreateTarget(permissionProvider);

			Assert.That(target.CreatePortalViewModel().HasAsmPermission, Is.False);
		}

		[Test]
		public void ShouldNotShowBadgeIfNoPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Expect(p => p.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewBadge))
				.Return(false);

			var target = CreateTarget(permissionProvider);

			Assert.That(target.CreatePortalViewModel().ShowBadge, Is.False);
		}

		[Test, SetCulture("en-US")]
		public void ShouldShowMeridianWhenUsCulture()
		{
			var target = CreateTarget(MockRepository.GenerateMock<IPermissionProvider>());

			target.CreatePortalViewModel().ShowMeridian.Should().Be.True();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldShowMeridianWhenSwedishCulture()
		{
			var target = CreateTarget(MockRepository.GenerateMock<IPermissionProvider>());

			target.CreatePortalViewModel().ShowMeridian.Should().Be.False();
		}

		[Test, SetCulture("en-GB")]
		public void ShouldShowMeridianWhenBritishCulture()
		{
			var target = CreateTarget(MockRepository.GenerateMock<IPermissionProvider>());

			target.CreatePortalViewModel().ShowMeridian.Should().Be.False();
		}

		private static PortalViewModelFactory CreateTarget(IPermissionProvider permissionProvider)
		{
			return new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>(), _personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>(), MockRepository.GenerateStub<ICurrentTenantUser>(),
				_userCulture);
		}
	}

}