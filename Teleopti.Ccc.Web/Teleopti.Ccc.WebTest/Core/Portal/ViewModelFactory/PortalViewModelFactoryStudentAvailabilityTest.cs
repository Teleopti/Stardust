using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryStudentAvailabilityTest
	{
		private IPersonNameProvider _personNameProvider;
		private ILoggedOnUser _loggedOnUser;

		[SetUp]
		public void Setup()
		{
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(new Person() { Name = new Name() });

			_personNameProvider = MockRepository.GenerateMock<IPersonNameProvider>();
			_personNameProvider.Stub(x => x.BuildNameFromSetting(_loggedOnUser.CurrentUser().Name)).Return("A B");
		}

		[Test]
		public void ShouldNotHaveStudentAvailabilityNavigationItemIfNotPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(
						Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.StudentAvailability))).Return(true);
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StudentAvailability)).Return(false);

			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser, MockRepository.GenerateMock<IReportsNavigationProvider>(),
				MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IBadgeSettingProvider>(), MockRepository.GenerateMock<IToggleManager>(), _personNameProvider,
				MockRepository.GenerateMock<ITeamGamificationSettingRepository>());

			var result = target.CreatePortalViewModel();

			var studentAvailability = (from i in result.NavigationItems where i.Controller == "StudentAvailability" select i).SingleOrDefault();
			studentAvailability.Should().Be.Null();
		}
	}
}