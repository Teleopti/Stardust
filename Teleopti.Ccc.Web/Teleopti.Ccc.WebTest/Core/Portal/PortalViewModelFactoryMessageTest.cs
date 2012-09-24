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
	public class PortalViewModelFactoryMessageTest
	{
		[Test]
		public void ShouldNotHaveMessageNavigationItemIfNotPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<IIdentityProvider>(), MockRepository.GenerateMock<IPushMessageProvider>());

			var result = target.CreatePortalViewModel();

			var message = (from i in result.NavigationItems where i.Controller == "Message" select i).SingleOrDefault();
            message.Should().Be.Null();
		}

        [Test]
        public void ShouldPayAttentionToMessageTabDueToUnreadMessages()
        {
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var pushMessageProvider = MockRepository.GenerateMock<IPushMessageProvider>();

			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(true);
			pushMessageProvider.Stub(x => x.UnreadMessageCount).Return(1);

			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<IIdentityProvider>(), pushMessageProvider);

            var result = target.CreatePortalViewModel();
			SectionNavigationItem message = (from i in result.NavigationItems where i.Controller == "Message" select i).SingleOrDefault();
			message.PayAttention.Should().Be.True();
        }

		[Test]
		public void ShouldShowNumberOFUnreadMessages()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var pushMessageProvider = MockRepository.GenerateMock<IPushMessageProvider>();

			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(true);
			pushMessageProvider.Stub(x => x.UnreadMessageCount).Return(1);

			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<IIdentityProvider>(), pushMessageProvider);

			var result = target.CreatePortalViewModel();
			SectionNavigationItem message = (from i in result.NavigationItems where i.Controller == "Message" select i).SingleOrDefault();
			message.UnreadMessageCount.Should().Be.EqualTo(1);
		}
	}
}