using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
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
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			var scenario = new FakeCurrentScenario();
			var principalAuthorization = new PrincipalAuthorizationWithFullPermission();

			PrincipalAuthorization.SetInstance(principalAuthorization);

			builder.RegisterModule(new WebModule(configuration, null));
			builder.RegisterType<FakeLoggedOnUser>().As<ILoggedOnUser>().SingleInstance();
			builder.RegisterType<FakeNow>().As<INow>().SingleInstance();
			builder.RegisterInstance(new FakePermissionProvider(false)).As<IPermissionProvider>().SingleInstance();
			builder.RegisterInstance(new FakeScheduleDataReadScheduleRepository()).AsSelf().As<IScheduleRepository>().SingleInstance();
			builder.RegisterInstance(scenario).As<ICurrentScenario>().SingleInstance();
			builder.RegisterInstance(principalAuthorization).As<IPrincipalAuthorization>().SingleInstance();
			builder.RegisterInstance(new FakePersonRequestRepository()).As<IPersonRequestRepository>().SingleInstance();
			builder.RegisterInstance(new FakeScenarioRepository(scenario.Current())).As<IScenarioRepository>().SingleInstance();
			builder.RegisterInstance(new FakeBudgetDayRepository()).As<IBudgetDayRepository>().SingleInstance();
			builder.RegisterInstance(new FakeScheduleProjectionReadOnlyRepository()).As<IScheduleProjectionReadOnlyRepository>().SingleInstance();
			builder.RegisterType<AutoMapperConfiguration>().As<AutoMapperConfiguration>().SingleInstance();
			builder.RegisterType<FakePersonScheduleDayReadModelFinder>().As<IPersonScheduleDayReadModelFinder>().SingleInstance();
			builder.RegisterType<FakePersonForScheduleFinder>().As<IPersonForScheduleFinder>().SingleInstance();
			builder.RegisterType<FakeBusinessUnitRepository>().As<IBusinessUnitRepository>().SingleInstance();
			builder.RegisterType<FakeTeamRepository>().As<ITeamRepository>().SingleInstance();
			builder.RegisterType<FakePersonRepository>().As<IPersonRepository>().SingleInstance();
			builder.RegisterType<FakePersonNameProvider>().As<IPersonNameProvider>().SingleInstance();
			builder.RegisterType<FakePersonAssignmentRepository>().As<IPersonAssignmentRepository>().SingleInstance();
			builder.RegisterType<FakeShiftTradeLightValidator>().As<IShiftTradeLightValidator>().SingleInstance();


		}

		protected override void BeforeInject(IComponentContext container)
		{
			container.Resolve<AutoMapperConfiguration>().Execute(null);
		}

		
	}
}