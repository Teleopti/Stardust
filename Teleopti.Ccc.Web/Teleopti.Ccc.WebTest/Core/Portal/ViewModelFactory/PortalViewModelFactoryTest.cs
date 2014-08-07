using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryTest
	{
		private IReportsNavigationProvider _reportsNavProvider;

		[SetUp]
		public void Setup()
		{
			_reportsNavProvider = MockRepository.GenerateMock<IReportsNavigationProvider>();

		}

		[Test]
		public void ShouldHaveNavigationItems()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.Anything)).Return(true);
			_reportsNavProvider.Stub(x => x.GetNavigationItems())
				.Return(new[] {new ReportNavigationItem {Action = "Index", Controller = "MyReport", IsMyReport = true}});
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>(),
				_reportsNavProvider, MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IBadgeSettingProvider>(), MockRepository.GenerateMock<IToggleManager>());

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
				MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>(),
				_reportsNavProvider, MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IBadgeSettingProvider>(), MockRepository.GenerateMock<IToggleManager>());

			var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();
			licenseActivator.Stub(x => x.CustomerName).Return("Customer Name");
			licenseActivatorProvider.Stub(x => x.Current()).Return(licenseActivator);

			var result = target.CreatePortalViewModel();

			result.CustomerName.Should().Be.EqualTo("Customer Name");
		}

		[Test]
		public void ShouldHideChangePasswordIfNoApplicationAuthenticationExists()
		{
			var agent = new Person();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Expect(m => m.CurrentUser()).Return(agent);
			var target = new PortalViewModelFactory(MockRepository.GenerateStub<IPermissionProvider>(),
				MockRepository.GenerateStub<ILicenseActivatorProvider>(), MockRepository.GenerateMock<IPushMessageProvider>(),
				loggedOnUser, _reportsNavProvider, MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IBadgeSettingProvider>(), MockRepository.GenerateMock<IToggleManager>());
			var res = target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.False();
		}

		[Test]
		public void ShouldHideChangePasswordIfNoApplicationLogonExists()
		{
			var agent = new Person {ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo()};
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Expect(m => m.CurrentUser()).Return(agent);
			var target = new PortalViewModelFactory(MockRepository.GenerateStub<IPermissionProvider>(),
				MockRepository.GenerateStub<ILicenseActivatorProvider>(), MockRepository.GenerateMock<IPushMessageProvider>(),
				loggedOnUser, _reportsNavProvider, MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IBadgeSettingProvider>(), MockRepository.GenerateMock<IToggleManager>());
			var res = target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.False();
		}



		[Test]
		public void ShouldShowChangePasswordIfApplicationLogonExists()
		{
			var agent = new Person { ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo{ApplicationLogOnName = "Arne Weise"} };
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Expect(m => m.CurrentUser()).Return(agent);
			var target = new PortalViewModelFactory(MockRepository.GenerateStub<IPermissionProvider>(),
				MockRepository.GenerateStub<ILicenseActivatorProvider>(), MockRepository.GenerateMock<IPushMessageProvider>(),
				loggedOnUser, _reportsNavProvider, MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IBadgeSettingProvider>(), MockRepository.GenerateMock<IToggleManager>());
			var res = target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.True();
		}

		[Test]
		public void ShouldShowAsmIfPermissionToShowAsm()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Expect(p => p.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(true);

			var target = CreateTarget(permissionProvider);

			Assert.That(target.CreatePortalViewModel().HasAsmPermission, Is.True);
		}

		[Test]
		public void ShouldNotShowAsmIfNoPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Expect(p => p.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(false);

			var target = CreateTarget(permissionProvider);
			
			Assert.That(target.CreatePortalViewModel().HasAsmPermission, Is.False);
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
				MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>(),
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IBadgeSettingProvider>(), MockRepository.GenerateMock<IToggleManager>());
		}
	}

}