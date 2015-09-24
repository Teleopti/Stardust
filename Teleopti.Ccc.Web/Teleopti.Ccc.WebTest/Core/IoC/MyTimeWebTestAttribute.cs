using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.WebTest.Core.Common;
using Teleopti.Ccc.WebTest.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.IoC
{
	public class MyTimeWebTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			var scenario = new FakeCurrentScenario();
			var principalAuthorization = new PrincipalAuthorizationWithFullPermission();

			PrincipalAuthorization.SetInstance(principalAuthorization);

			system.AddModule(new WebModule(configuration, null));

			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble(new FakePermissionProvider(false)).For<IPermissionProvider>();
			system.UseTestDouble(new FakeScheduleDataReadScheduleRepository()).For<IScheduleRepository>();
			system.UseTestDouble(scenario).For<ICurrentScenario>();
			system.UseTestDouble(principalAuthorization).For<IPrincipalAuthorization>();
			system.UseTestDouble(new FakePersonRequestRepository()).For<IPersonRequestRepository>();
			system.UseTestDouble(new FakeSeatBookingRepository()).For<ISeatBookingRepository>();
			system.UseTestDouble(new FakeScenarioRepository(scenario.Current())).For<IScenarioRepository>();
			system.UseTestDouble(new FakeBudgetDayRepository()).For<IBudgetDayRepository>();
			system.UseTestDouble(new FakeScheduleProjectionReadOnlyRepository()).For<IScheduleProjectionReadOnlyRepository>();
			system.UseTestDouble<FakePersonScheduleDayReadModelFinder>().For<IPersonScheduleDayReadModelFinder>();
			system.UseTestDouble<FakePersonForScheduleFinder>().For<IPersonForScheduleFinder>();
			system.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakePersonNameProvider>().For<IPersonNameProvider>();
			system.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			system.UseTestDouble<FakeShiftTradeLightValidator>().For<IShiftTradeLightValidator>();
			system.UseTestDouble<FakePersonContractProvider>().For<FakePersonContractProvider>();

			system.AddService<AutoMapperConfiguration>();

		}

		protected override void Startup(IComponentContext container)
		{
			container.Resolve<AutoMapperConfiguration>().Execute(null);
		}
		
	}
}