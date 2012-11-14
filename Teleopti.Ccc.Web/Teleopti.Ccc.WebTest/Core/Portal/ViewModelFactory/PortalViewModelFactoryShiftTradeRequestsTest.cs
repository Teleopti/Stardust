using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryShiftTradeRequestsTest
	{
		[Test]
		public void ShouldNotHaveShiftTradeRequestsNavigationItemIfNotPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)).Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<IIdentityProvider>(), MockRepository.GenerateMock<IPushMessageProvider>());

			var result = target.CreatePortalViewModel();

			var shiftTradeRequestsItem = (from i in result.NavigationItems where i.Controller == "ShiftTradeRequests" select i).SingleOrDefault();
			shiftTradeRequestsItem.Should().Be.Null();
		}

		[Test]
		public void ShouldHaveShiftTradeRequestsNavigationItem()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)).Return(true);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<IIdentityProvider>(), MockRepository.GenerateMock<IPushMessageProvider>());

			var result = target.CreatePortalViewModel();

			var shiftTradeRequestsItem = (from i in result.NavigationItems where i.Controller == "ShiftTradeRequests" select i).SingleOrDefault();
			shiftTradeRequestsItem.Should().Not.Be.Null();
		}
	}
}