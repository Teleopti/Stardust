using System.Collections.Generic;
using System.IdentityModel.Claims;
using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.WebTest.Core.IoC;

using Claim = System.IdentityModel.Claims.Claim;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[MyTimeWebTest]
	[TestFixture]
	public class PortalViewModelFactoryMessageTest : IIsolateSystem
	{
		public IPortalViewModelFactory Target;
		public FakePushMessageDialogueRepository PushMessageDialogueRepository;
		public ICurrentDataSource CurrentDataSource;
		public FakeLoggedOnUser LoggedOnUser;
		public CurrentTenantUserFake CurrentTenantUser;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PrincipalAuthorization>().For<IAuthorization>();
			isolate.UseTestDouble<FakePushMessageDialogueRepository>().For<IPushMessageDialogueRepository>();
			isolate.UseTestDouble<PermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			isolate.UseTestDouble(new FakeCurrentUnitOfWorkFactory(null).WithCurrent(new FakeUnitOfWorkFactory(null, null, null) { Name = MyTimeWebTestAttribute.DefaultTenantName })).For<ICurrentUnitOfWorkFactory>();
		}

		[Test]
		public void ShouldNotHaveMessageNavigationItemIfNotPermission()
		{
			setPermissions(false);

			var result = Target.CreatePortalViewModel();

			var message = (from i in result.NavigationItems where i.Controller == "Message" select i).SingleOrDefault();
			message.Should().Be.Null();
		}

		[Test]
		public void ShouldPayAttentionToMessageTabDueToUnreadMessages()
		{
			setPermissions();
			PushMessageDialogueRepository.Add(new PushMessageDialogue(new PushMessage(new List<string> { "test" }),
				LoggedOnUser.CurrentUser()));

			var result = Target.CreatePortalViewModel();
			var message =
				(from i in result.NavigationItems where i.Controller == "Message" select i).SingleOrDefault();
			message.PayAttention.Should().Be.True();
		}

		[Test]
		public void ShouldShowNumberOfUnreadMessages()
		{
			setPermissions();
			PushMessageDialogueRepository.Add(new PushMessageDialogue(new PushMessage(new List<string> {"test"}),
				LoggedOnUser.CurrentUser()));

			var result = Target.CreatePortalViewModel();
			var message =
				(from i in result.NavigationItems where i.Controller == "Message" select i).SingleOrDefault();
			message.UnreadMessageCount.Should().Be.EqualTo(1);
		}

		private static void setPermissions(bool asmPermission = true)
		{
			var principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
			var claims = new List<Claim>
			{
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.MyReportWeb)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.TeamSchedule)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.StudentAvailability)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.StandardPreferences)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.TextRequests)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(
					string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
						"/AvailableData"), new AuthorizeEveryone(), Rights.PossessProperty)
			};

			if (asmPermission)
				claims.Add(new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)
					, new AuthorizeEveryone(), Rights.PossessProperty));

			principal.AddClaimSet(new DefaultClaimSet(ClaimSet.System, claims));
		}
	}
}