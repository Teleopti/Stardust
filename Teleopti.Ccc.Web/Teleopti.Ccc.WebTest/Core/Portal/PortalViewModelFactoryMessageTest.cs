using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Core.Portal
{
	[TestFixture]
	public class PortalViewModelFactoryMessageTest
	{
		[Test]
		public void ShouldNotHaveMessageNavigationItemIfNotPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<IIdentityProvider>());

			var result = target.CreatePortalViewModel();

			var message = (from i in result.NavigationItems where i.Controller == "Message" select i).SingleOrDefault();
            message.Should().Be.Null();
		}
	}
}