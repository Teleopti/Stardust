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
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryMessageTest
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
		public void ShouldNotHaveMessageNavigationItemIfNotPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(
				x =>
					x.HasApplicationFunctionPermission(
						Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger))).Return(true);
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>(),
				_personNameProvider, MockRepository.GenerateMock<ITeamGamificationSettingRepository>(),
				MockRepository.GenerateStub<ICurrentTenantUser>(), _userCulture);

			var result = target.CreatePortalViewModel();

			var message = (from i in result.NavigationItems where i.Controller == "Message" select i).SingleOrDefault();
			message.Should().Be.Null();
		}

		[Test]
		public void ShouldPayAttentionToMessageTabDueToUnreadMessages()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var pushMessageProvider = MockRepository.GenerateMock<IPushMessageProvider>();

			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(true);
			pushMessageProvider.Stub(x => x.UnreadMessageCount).Return(1);

			var target = new PortalViewModelFactory(permissionProvider,
				MockRepository.GenerateMock<ILicenseActivatorProvider>(), pushMessageProvider,
				_loggedOnUser, MockRepository.GenerateMock<IReportsNavigationProvider>(),
				MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>(),
				_personNameProvider, MockRepository.GenerateMock<ITeamGamificationSettingRepository>(),
				MockRepository.GenerateStub<ICurrentTenantUser>(), _userCulture);

			var result = target.CreatePortalViewModel();
			NavigationItem message =
				(from i in result.NavigationItems where i.Controller == "Message" select i).SingleOrDefault();
			message.PayAttention.Should().Be.True();
		}

		[Test]
		public void ShouldShowNumberOfUnreadMessages()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var pushMessageProvider = MockRepository.GenerateMock<IPushMessageProvider>();

			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(true);
			pushMessageProvider.Stub(x => x.UnreadMessageCount).Return(1);

			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				pushMessageProvider, _loggedOnUser,
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>(),
				_personNameProvider, MockRepository.GenerateMock<ITeamGamificationSettingRepository>(),
				MockRepository.GenerateStub<ICurrentTenantUser>(), _userCulture);

			var result = target.CreatePortalViewModel();
			NavigationItem message =
				(from i in result.NavigationItems where i.Controller == "Message" select i).SingleOrDefault();
			message.UnreadMessageCount.Should().Be.EqualTo(1);
		}
	}
}