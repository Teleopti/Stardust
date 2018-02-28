using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.WebTest.Core.IoC;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[MyTimeWebTest]
	[TestFixture]
	public class PortalViewModelFactoryTeamScheduleTest : ISetup
	{
		public IPortalViewModelFactory Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<PrincipalAuthorization>().For<IAuthorization>();
			system.UseTestDouble<PermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			system.UseTestDouble(new FakeCurrentUnitOfWorkFactory(null).WithCurrent(new FakeUnitOfWorkFactory(null, null, null, null) { Name = MyTimeWebTestAttribute.DefaultTenantName })).For<ICurrentUnitOfWorkFactory>();
		}

		[Test]
		public void ShouldNotHaveTeamScheduleNavigationItemIfNotPermission()
		{
			var result = relevantTab(Target.CreatePortalViewModel());

			result.Should().Be.Null();
		}

		private static NavigationItem relevantTab(PortalViewModel result)
		{
			return (from i in result.NavigationItems where i.Controller == "TeamSchedule" select i)
				.SingleOrDefault();
		}
	}
}