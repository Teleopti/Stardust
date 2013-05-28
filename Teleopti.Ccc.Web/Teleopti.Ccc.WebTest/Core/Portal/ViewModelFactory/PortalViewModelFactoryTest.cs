using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryTest
	{
		[Test]
		public void ShouldHaveNavigationItems()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.Anything)).Return(true);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			result.NavigationItems.Should().Have.Count.EqualTo(6);
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
		}

		[Test]
		public void ShouldHaveCustomerName()
		{
			var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();
			var target = new PortalViewModelFactory(MockRepository.GenerateMock<IPermissionProvider>(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), licenseActivator, MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			licenseActivator.Stub(x => x.CustomerName).Return("Customer Name");

			var result = target.CreatePortalViewModel();

			result.CustomerName.Should().Be.EqualTo("Customer Name");
		}

		[Test]
		public void ShouldHideChangePasswordIfNoApplicationAuthenticationExists()
		{
			var agent = new Person();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Expect(m => m.CurrentUser()).Return(agent);
			var target = new PortalViewModelFactory(MockRepository.GenerateStub<IPermissionProvider>(), null, MockRepository.GenerateStub<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), loggedOnUser);
			var res = target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.False();
		}

		[Test]
		public void ShouldHideChangePasswordIfNoApplicationLogonExists()
		{
			var agent = new Person {ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo()};
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Expect(m => m.CurrentUser()).Return(agent);
			var target = new PortalViewModelFactory(MockRepository.GenerateStub<IPermissionProvider>(), null, MockRepository.GenerateStub<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), loggedOnUser);
			var res = target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.False();
		}



		[Test]
		public void ShouldShowChangePasswordIfApplicationLogonExists()
		{
			var agent = new Person { ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo{ApplicationLogOnName = "Arne Weise"} };
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Expect(m => m.CurrentUser()).Return(agent);
			var target = new PortalViewModelFactory(MockRepository.GenerateStub<IPermissionProvider>(), null, MockRepository.GenerateStub<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), loggedOnUser);
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

		private static PortalViewModelFactory CreateTarget(IPermissionProvider permissionProvider)
		{
			return new PortalViewModelFactory(permissionProvider, null, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>()); 
		}
	}

}