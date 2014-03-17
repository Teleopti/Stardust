using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
	public class PortalViewModelFactoryStudentAvailabilityTest
	{
		[Test]
		public void ShouldNotHaveStudentAvailabilityNavigationItemIfNotPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.StudentAvailability))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StudentAvailability)).Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>(), MockRepository.GenerateMock<IReportsNavigationProvider>());

			var result = target.CreatePortalViewModel();

			var studentAvailability = (from i in result.NavigationItems where i.Controller == "StudentAvailability" select i).SingleOrDefault();
			studentAvailability.Should().Be.Null();
		}
	}
}