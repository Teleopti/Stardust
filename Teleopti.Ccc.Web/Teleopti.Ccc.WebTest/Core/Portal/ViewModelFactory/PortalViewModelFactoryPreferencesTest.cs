using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using SharpTestsEx;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;
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
	public class PortalViewModelFactoryPreferencesTest
	{
		private IPersonNameProvider _personNameProvider;
		private ILoggedOnUser _loggedOnUser;
		private IUserCulture _userCulture;
		private ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

		[SetUp]
		public void Setup()
		{
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(new Person { Name = new Name() });

			var culture = CultureInfo.GetCultureInfo("sv-SE");
			_userCulture = MockRepository.GenerateMock<IUserCulture>();
			_userCulture.Expect(x => x.GetCulture()).Return(culture);

			var principal = MockRepository.GenerateMock<ITeleoptiPrincipal>();
			principal.Expect(x => x.Regional);
			_currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			_currentTeleoptiPrincipal.Expect(x => x.Current()).Return(principal);

			_personNameProvider = MockRepository.GenerateMock<IPersonNameProvider>();
			_personNameProvider.Stub(x => x.BuildNameFromSetting(_loggedOnUser.CurrentUser().Name)).Return("A B");
		}

		[Test]
		public void ShouldNotHavePreferencesNavigationItemIfNotPermission()
		{
			var permissionProvider = NoPermissionToPreferences();
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				_personNameProvider, MockRepository.GenerateMock<ITeamGamificationSettingRepository>(),
				MockRepository.GenerateStub<ICurrentTenantUser>(), _userCulture, _currentTeleoptiPrincipal);

			var result = target.CreatePortalViewModel();

			result.Controller("Preference").Should().Be.Null();
		}

		[Test]
		public void ShouldHavePreferencesNavigationItemIfOnlyPermissionToExtendedPreferences()
		{
			var permissionProvider = NoPermissionToStandardPreferences();
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				_personNameProvider, MockRepository.GenerateMock<ITeamGamificationSettingRepository>(),
				MockRepository.GenerateStub<ICurrentTenantUser>(), _userCulture, _currentTeleoptiPrincipal);

			var result = target.CreatePortalViewModel();

			result.Controller("Preference").Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHavePreferencesNavigationItemIfOnlyPermissionToStandardPreferences()
		{
			var permissionProvider = NoPermissionToExtendedPreferences();
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				_personNameProvider, MockRepository.GenerateMock<ITeamGamificationSettingRepository>(),
				MockRepository.GenerateStub<ICurrentTenantUser>(), _userCulture, _currentTeleoptiPrincipal);

			var result = target.CreatePortalViewModel();

			result.Controller("Preference").Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotHavePreferencesNavigationItemIfJalaliCalendarIsUsedAndPermissionToExtendedPreferences()
		{
			var permissionProvider = NoPermissionToStandardPreferences();
			var userCulture = MockRepository.GenerateMock<IUserCulture>();
			userCulture.Expect(x => x.GetCulture()).Return(CultureInfo.GetCultureInfo("fa-IR"));
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				_personNameProvider, MockRepository.GenerateMock<ITeamGamificationSettingRepository>(),
				MockRepository.GenerateStub<ICurrentTenantUser>(), userCulture, _currentTeleoptiPrincipal);

			var result = target.CreatePortalViewModel();

			result.Controller("Preference").Should().Be.Null();
		}

		[Test]
		public void ShouldNotHavePreferencesNavigationItemIfJalaliCalendarIsUsedAndPermissionToStandardPreferences()
		{
			var permissionProvider = NoPermissionToExtendedPreferences();
			var userCulture = MockRepository.GenerateMock<IUserCulture>();
			userCulture.Expect(x => x.GetCulture()).Return(CultureInfo.GetCultureInfo("fa-IR"));
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				_personNameProvider, MockRepository.GenerateMock<ITeamGamificationSettingRepository>(),
				MockRepository.GenerateStub<ICurrentTenantUser>(), userCulture, _currentTeleoptiPrincipal);

			var result = target.CreatePortalViewModel();

			result.Controller("Preference").Should().Be.Null();
		}

		private IPermissionProvider NoPermissionToPreferences()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(
				x =>
					x.HasApplicationFunctionPermission(
						Arg<string>.Matches(
							new PredicateConstraint<string>(s => s != DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb &&
																 s != DefinedRaptorApplicationFunctionPaths.StandardPreferences))))
				.Return(true);
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb)).Return(false);
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences)).Return(false);

			return permissionProvider;
		}

		private IPermissionProvider NoPermissionToStandardPreferences()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(
				x =>
					x.HasApplicationFunctionPermission(
						Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.StandardPreferences))).Return(true);
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences)).Return(false);
			return permissionProvider;
		}

		private IPermissionProvider NoPermissionToExtendedPreferences()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(
				x =>
					x.HasApplicationFunctionPermission(
						Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb))).Return(true);
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb)).Return(false);
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