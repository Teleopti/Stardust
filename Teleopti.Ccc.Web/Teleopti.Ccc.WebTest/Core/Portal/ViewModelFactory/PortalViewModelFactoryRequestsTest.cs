using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Core.RequestContext;

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

		private void TestThatRequestTabIsCreated(string applicationFunctionPath)
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(applicationFunctionPath)).Return(true);

			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());
			var result = target.CreatePortalViewModel();

			var requestTab = (from i in result.NavigationItems where i.Controller == "Requests" select i).SingleOrDefault();
			requestTab.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHaveDatePicker()
		{
			var target = new PortalViewModelFactory(new FakePermissionProvider(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = ToolBarItemOfType<ToolBarDatePicker>(target.CreatePortalViewModel());

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHaveCreateShiftTradeRequestsButtonIfPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();

			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)).Return(true);

			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = ToolBarItemsOfType<ToolBarButtonItem>(target.CreatePortalViewModel());

			result.Should().Not.Be.Null();
			/* Istf denna kodrad borde man kolla att buttontypen finns i listan, inte på en speciell position eftersom det kommer inte alltid stämma! */
			result[1].ButtonType.Should().Be.EqualTo("addShiftTradeRequest");

			//result.Should().Contain()

			//foreach (var toolBarButtonItem in result)
			//{
			//    if (toolBarButtonItem.ButtonType == "addShiftTradeRequest")
			//    {
					
			//    }
			//}
		}

		[Test]
		public void ShouldHaveAddRequestButtonIfPermission()
		{
			var target = new PortalViewModelFactory(new FakePermissionProvider(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = ToolBarItemsOfType<ToolBarButtonItem>(target.CreatePortalViewModel());

			result.Should().Not.Be.Null();
			result[2].ButtonType.Should().Be.EqualTo("addRequest");
		}

		private static SectionNavigationItem RelevantTab(PortalViewModel result)
		{
			return (from i in result.NavigationItems where i.Controller == "Requests" select i)
					.SingleOrDefault();
		}

		private static T ToolBarItemOfType<T>(PortalViewModel result) where T : ToolBarItemBase
		{
			return (from i in RelevantTab(result).ToolBarItems where i is T select i)
				.Cast<T>()
				.SingleOrDefault();
		}

		private static List<T> ToolBarItemsOfType<T>(PortalViewModel result) where T : ToolBarItemBase
		{
			return (from i in RelevantTab(result).ToolBarItems where i is T select i)
				.Cast<T>()
				.ToList();
		}
	}
}