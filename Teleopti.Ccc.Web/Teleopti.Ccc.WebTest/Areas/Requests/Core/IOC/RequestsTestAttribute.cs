using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Ccc.WebTest.Core.Common;
using Teleopti.Ccc.WebTest.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC
{
	class RequestsTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			var scenario = new FakeCurrentScenario();
			var principalAuthorization = new PrincipalAuthorizationWithFullPermission();

			PrincipalAuthorization.SetInstance(principalAuthorization);

			system.AddModule(new WebModule(configuration, null));

			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble(scenario).For<ICurrentScenario>();
			system.UseTestDouble(principalAuthorization).For<IPrincipalAuthorization>();
			system.UseTestDouble<FakePersonNameProvider>().For<IPersonNameProvider>();
			system.UseTestDouble(new FakePersonRequestRepository()).For<IPersonRequestRepository>();
			system.UseTestDouble<FakePeopleSearchProvider>().For<IPeopleSearchProvider>();

			system.UseTestDouble<SyncCommandDispatcher>().For<ICommandDispatcher>();
			system.UseTestDouble<FakeRequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();
			system.UseTestDouble<FakeScheduleDataReadScheduleRepository>().For<IScheduleRepository>();
			
		}
		
		

	}
}
