using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryRequestsTest
	{
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
		public void	ShouldNotCreateRequestTabIfNoPermissionsToRequests()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests)).Return(false);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb)).Return(false);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)).Return(false);

			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>(), MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>());
			var result = target.CreatePortalViewModel();

			var requestTab = (from i in result.NavigationItems where i.Controller == "Requests" select i).SingleOrDefault();
			requestTab.Should().Be.Null();
		}
		
		private void TestThatRequestTabIsCreated(string applicationFunctionPath)
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(applicationFunctionPath)).Return(true);

			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>(), MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IToggleManager>());
			var result = target.CreatePortalViewModel();

			var requestTab = (from i in result.NavigationItems where i.Controller == "Requests" select i).SingleOrDefault();
			requestTab.Should().Not.Be.Null();
		}
	}
}