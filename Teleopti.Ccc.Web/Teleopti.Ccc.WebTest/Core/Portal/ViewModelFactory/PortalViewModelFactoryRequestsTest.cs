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
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryRequestsTest
	{
		private IPersonNameProvider _personNameProvider;
		private ILoggedOnUser _loggedOnUser;
		private IUserCulture _userCulture;

		[SetUp]
		public void Setup()
		{
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(new Person() {Name = new Name()});

			var culture = CultureInfo.GetCultureInfo("sv-SE");
			_userCulture = MockRepository.GenerateMock<IUserCulture>();
			_userCulture.Expect(x => x.GetCulture()).Return(culture);

			_personNameProvider = MockRepository.GenerateMock<IPersonNameProvider>();
			_personNameProvider.Stub(x => x.BuildNameFromSetting(_loggedOnUser.CurrentUser().Name)).Return("A B");
		}

		[Test]
		public void ShouldCreateRequestTabIfOnlyPermissionsToTextRequest()
		{
			TestThatRequestTabIsCreated(DefinedRaptorApplicationFunctionPaths.TextRequests);
		}

		[Test]
		public void ShouldCreateRequestTabIfOnlyPermissionsToAbsenceRequest()
		{
			TestThatRequestTabIsCreated(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb);
		}

		[Test]
		public void ShouldCreateRequestTabIfOnlyPermissionsToShiftTradeRequest()
		{
			TestThatRequestTabIsCreated(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);
		}

		[Test]
		public void ShouldNotCreateRequestTabIfNoPermissionsToRequests()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests))
				.Return(false);
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb)).Return(false);
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)).Return(false);

			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>(), _personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>(), MockRepository.GenerateStub<ICurrentTenantUser>(),
				_userCulture);

			var result = target.CreatePortalViewModel();

			var requestTab = (from i in result.NavigationItems where i.Controller == "Requests" select i).SingleOrDefault();
			requestTab.Should().Be.Null();
		}

		private void TestThatRequestTabIsCreated(string applicationFunctionPath)
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(applicationFunctionPath)).Return(true);

			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>(), _personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>(), MockRepository.GenerateStub<ICurrentTenantUser>(),
				_userCulture);

			var result = target.CreatePortalViewModel();

			var requestTab = (from i in result.NavigationItems where i.Controller == "Requests" select i).SingleOrDefault();
			requestTab.Should().Not.Be.Null();
		}
	}
}