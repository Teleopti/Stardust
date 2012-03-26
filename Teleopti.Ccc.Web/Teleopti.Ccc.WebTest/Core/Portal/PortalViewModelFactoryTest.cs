using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal
{
	[TestFixture]
	public class PortalViewModelFactoryTest
	{
		[Test]
		public void ShouldHaveNavigationItems()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.Anything)).Return(true);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<ICurrentPrincipalProvider>());

			var result = target.CreatePortalViewModel();

			result.NavigationItems.Should().Have.Count.EqualTo(5);
			result.NavigationItems.ElementAt(0).Action.Should().Be("Week");
			result.NavigationItems.ElementAt(0).Controller.Should().Be("Schedule");
			result.NavigationItems.ElementAt(1).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(1).Controller.Should().Be("TeamSchedule");
			result.NavigationItems.ElementAt(2).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(2).Controller.Should().Be("StudentAvailability");
			result.NavigationItems.ElementAt(3).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(3).Controller.Should().Be("Preference");
			result.NavigationItems.ElementAt(4).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(4).Controller.Should().Be("Requests");
		}

		[Test]
		public void ShouldHaveCustomerName()
		{
			var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();
			var target = new PortalViewModelFactory(MockRepository.GenerateMock<IPermissionProvider>(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), licenseActivator, MockRepository.GenerateStub<ICurrentPrincipalProvider>());

			licenseActivator.Stub(x => x.CustomerName).Return("Customer Name");

			var result = target.CreatePortalViewModel();

			result.CustomerName.Should().Be.EqualTo("Customer Name");
		}

		[Test]
		public void ShouldHideChangePasswordIfWindowsAuthentication()
		{
			var principalProvider = MockRepository.GenerateMock<ICurrentPrincipalProvider>();
			var winPrincipal =
				new TeleoptiPrincipal(new TeleoptiIdentity(string.Empty, null, null, null, AuthenticationTypeOption.Windows), null);
			var target = new PortalViewModelFactory(MockRepository.GenerateStub<IPermissionProvider>(), null, MockRepository.GenerateStub<ILicenseActivator>(), principalProvider);
			principalProvider.Expect(mock => mock.Current()).Return(winPrincipal);
			var res = target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.False();
		}

		[Test]
		public void ShouldShowChangePasswordIfFormsAuthentication()
		{
			var principalProvider = MockRepository.GenerateMock<ICurrentPrincipalProvider>();
			var winPrincipal =
				new TeleoptiPrincipal(new TeleoptiIdentity(string.Empty, null, null, null, AuthenticationTypeOption.Application), null);
			var target = new PortalViewModelFactory(MockRepository.GenerateStub<IPermissionProvider>(), null, MockRepository.GenerateStub<ILicenseActivator>(), principalProvider);
			principalProvider.Expect(mock => mock.Current()).Return(winPrincipal);
			var res = target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.True();
		}
	}

}