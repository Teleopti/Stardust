using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Core.Portal
{
	[TestFixture]
	public class PortalViewModelFactoryRequestsTest
	{
		[Test]
		public void ShouldNotHaveRequestsNavigationItemIfNotPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.TextRequests))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests)).Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<ICurrentPrincipalProvider>());

			var result = target.CreatePortalViewModel();

			var requests = (from i in result.NavigationItems where i.Controller == "Requests" select i).SingleOrDefault();
			requests.Should().Be.Null();
		}

		[Test]
		public void ScheduleShouldHaveAddTextRequestButton()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.Anything)).Return(true);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<ICurrentPrincipalProvider>());

			var result = target.CreatePortalViewModel();

			var addTextRequstButton = (from n in result.NavigationItems
			                           from i in n.ToolBarItems
			                           where n.Controller == "Schedule"
			                                 && i is ToolBarButtonItem
			                                 && ((ToolBarButtonItem)i).ButtonType == "addTextRequest"
			                           select i).SingleOrDefault();
			addTextRequstButton.Should().Not.Be.Null();
		}

		[Test]
		public void ScheduleShouldNotHaveAddTextRequestButtonIfNoPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.TextRequests))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests)).Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<ICurrentPrincipalProvider>());

			var result = target.CreatePortalViewModel();

			var addTextRequstButton = (from n in result.NavigationItems
			                           from i in n.ToolBarItems
			                           where n.Controller == "Schedule"
			                                 && i is ToolBarButtonItem
			                                 && ((ToolBarButtonItem)i).ButtonType == "addTextRequest"
			                           select i).SingleOrDefault();
			addTextRequstButton.Should().Be.Null();
		}

	}
}