using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using SharpTestsEx;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryPreferencesTest
	{
		private IPersonNameProvider _personNameProvider;
		private ILoggedOnUser _loggedOnUser;
		private IUserCulture _userCulture;

		[SetUp]
		public void Setup()
		{
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(new Person {Name = new Name()});

			var culture = CultureInfo.GetCultureInfo("sv-SE");
			_userCulture = MockRepository.GenerateMock<IUserCulture>();
			_userCulture.Expect(x => x.GetCulture()).Return(culture);

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
				MockRepository.GenerateMock<IToggleManager>(), _personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>(), MockRepository.GenerateStub<ICurrentTenantUser>(),
				_userCulture);

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
				MockRepository.GenerateMock<IToggleManager>(), _personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>(), MockRepository.GenerateStub<ICurrentTenantUser>(),
				_userCulture);

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
				MockRepository.GenerateMock<IToggleManager>(), _personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>(), MockRepository.GenerateStub<ICurrentTenantUser>(),
				_userCulture);


			var result = target.CreatePortalViewModel();

			result.Controller("Preference").Should().Not.Be.Null();
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