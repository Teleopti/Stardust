using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using SharpTestsEx;
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
	public class PortalViewModelFactoryPreferencesTest
	{
		[Test]
		public void ShouldNotHavePreferencesNavigationItemIfNotPermission()
		{
			var permissionProvider = NoPermissionToPreferences();
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>(), MockRepository.GenerateMock<IReportsNavigationProvider>());

			var result = target.CreatePortalViewModel();

			result.Controller("Preference").Should().Be.Null();
		}

		[Test]
		public void ShouldHavePreferencesNavigationItemIfOnlyPermissionToExtendedPreferences()
		{
			var permissionProvider = NoPermissionToStandardPreferences();
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>(), MockRepository.GenerateMock<IReportsNavigationProvider>());

			var result = target.CreatePortalViewModel();

			result.Controller("Preference").Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHavePreferencesNavigationItemIfOnlyPermissionToStandardPreferences()
		{
			var permissionProvider = NoPermissionToExtendedPreferences();
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>(), MockRepository.GenerateMock<IReportsNavigationProvider>());

			var result = target.CreatePortalViewModel();

			result.Controller("Preference").Should().Not.Be.Null();
		}

		private IPermissionProvider NoPermissionToPreferences()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Matches(new PredicateConstraint<string>(s => s != DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb &&
			                                                                                                                         s != DefinedRaptorApplicationFunctionPaths.StandardPreferences)))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb)).Return(false);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences)).Return(false);

			return permissionProvider;
		}

		private IPermissionProvider NoPermissionToStandardPreferences()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.StandardPreferences))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences)).Return(false);
			return permissionProvider;
		}

		private IPermissionProvider NoPermissionToExtendedPreferences()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb)).Return(false);
			return permissionProvider;
		}
	}

	public static class Ext
	{
		public static NavigationItem Controller(this PortalViewModel model, string controllerName)
		{
			return model.NavigationItems.SingleOrDefault(c => c.Controller == controllerName);
		}
	}
}