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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryStudentAvailabilityTest
	{
		[Test]
		public void ShouldNotHaveStudentAvailabilityNavigationItemIfNotPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.StudentAvailability))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StudentAvailability)).Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var studentAvailability = (from i in result.NavigationItems where i.Controller == "Availability" select i).SingleOrDefault();
			studentAvailability.Should().Be.Null();
		}

		[Test]
		public void StudentAvailabilityShouldHaveDatePicker()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.Anything)).Return(true);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var preference = (from i in result.NavigationItems where i.Controller == "Availability" select i).Single();
			var datePicker = (from i in preference.ToolBarItems where i is ToolBarDatePicker select i).SingleOrDefault();
			datePicker.Should().Not.Be.Null();
		}

		[Test]
		public void StudentAvailabilityShouldHaveEditAndDeleteButtons()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(null)).Return(true).IgnoreArguments();
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var preference = (from i in result.NavigationItems where i.Controller == "Availability" select i).Single();
			var buttons = from i in preference.ToolBarItems where i is ToolBarButtonItem select i;
			buttons.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void StudentAvailabilityShouldHaveToolbarItems()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.Anything)).Return(true);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var studentAvailabilityToolbarItems =
				result.NavigationItems.Where(ni => ni.Controller.Equals("Availability")).Select(x => x.ToolBarItems).First();

			studentAvailabilityToolbarItems.Should().Have.Count.EqualTo(5);
		}

	}
}