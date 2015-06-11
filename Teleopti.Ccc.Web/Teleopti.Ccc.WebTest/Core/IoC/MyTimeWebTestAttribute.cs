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
		protected override void RegisterInContainer(ISystem builder, IIocConfiguration configuration)
		{
			var scenario = new FakeCurrentScenario();
			var principalAuthorization = new PrincipalAuthorizationWithFullPermission();

			PrincipalAuthorization.SetInstance(principalAuthorization);

			builder.RegisterModule(new WebModule(configuration, null));

			builder.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			builder.UseTestDouble<FakeNow>().For<INow>();
			builder.UseTestDouble(new FakePermissionProvider(false)).For<IPermissionProvider>();
			builder.UseTestDouble(new FakeScheduleDataReadScheduleRepository()).For<IScheduleRepository>();
			builder.UseTestDouble(scenario).For<ICurrentScenario>();
			builder.UseTestDouble(principalAuthorization).For<IPrincipalAuthorization>();
			builder.UseTestDouble(new FakePersonRequestRepository()).For<IPersonRequestRepository>();
			builder.UseTestDouble(new FakeScenarioRepository(scenario.Current())).For<IScenarioRepository>();
			builder.UseTestDouble(new FakeBudgetDayRepository()).For<IBudgetDayRepository>();
			builder.UseTestDouble(new FakeScheduleProjectionReadOnlyRepository()).For<IScheduleProjectionReadOnlyRepository>();
			builder.UseTestDouble<FakePersonScheduleDayReadModelFinder>().For<IPersonScheduleDayReadModelFinder>();
			builder.UseTestDouble<FakePersonForScheduleFinder>().For<IPersonForScheduleFinder>();
			builder.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
			builder.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			builder.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			builder.UseTestDouble<FakePersonNameProvider>().For<IPersonNameProvider>();
			builder.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			builder.UseTestDouble<FakeShiftTradeLightValidator>().For<IShiftTradeLightValidator>();
			builder.UseTestDouble<FakePersonContractProvider>().For<FakePersonContractProvider>();

			builder.AddService<AutoMapperConfiguration>();

		}

		protected override void BeforeInject(IComponentContext container)
		{
			container.Resolve<AutoMapperConfiguration>().Execute(null);
		}
		
	}
}