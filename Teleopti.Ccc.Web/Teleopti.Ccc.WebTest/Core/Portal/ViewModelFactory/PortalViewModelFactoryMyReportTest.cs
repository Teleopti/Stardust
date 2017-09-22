using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
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
	public class PortalViewModelFactoryMyReportTest
	{
		private static NavigationItem relevantTab(PortalViewModel result)
		{
			return (from i in result.NavigationItems where i.Controller == "MyReport" select i)
				.SingleOrDefault();
		}

		private IPersonNameProvider _personNameProvider;
		private ILoggedOnUser _loggedOnUser;
		private IUserCulture _userCulture;
		private ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

		[SetUp]
		public void Setup()
		{
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(new Person().WithName(new Name()));

			var culture = CultureInfo.GetCultureInfo("sv-SE");
			_userCulture = MockRepository.GenerateMock<IUserCulture>();
			_userCulture.Expect(x => x.GetCulture()).Return(culture);

			_currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();

			_personNameProvider = MockRepository.GenerateMock<IPersonNameProvider>();
			_personNameProvider.Stub(x => x.BuildNameFromSetting(_loggedOnUser.CurrentUser().Name)).Return("A B");
		}

		[Test]
		public void NavigationItems_WhenNoPermissionForMyReportWeb_ShouldNotContainLinkToReports()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.MyReportWeb)))
				.Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.MyReportWeb))
				.Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				_personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>(), MockRepository.GenerateStub<ICurrentTenantUser>(),
				_userCulture, _currentTeleoptiPrincipal, new FakeToggleManager());

			var result = relevantTab(target.CreatePortalViewModel());

			result.Should().Be.Null();
		}
	}
}