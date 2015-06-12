using System;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class ResourcePlannerTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterModule(new WebModule(configuration, null));
			builder.RegisterModule(new SchedulingCommonModule());
			builder.RegisterModule(new ResourcePlannerModule());

			builder.RegisterType<FakePeopleAndSkillLoaderDecider>().As<IPeopleAndSkillLoaderDecider>().SingleInstance();
			builder.RegisterType<FakeRequiredScheduleHelper>().As<IRequiredScheduleHelper>().SingleInstance();
			builder.RegisterType<FakeScheduleCommand>().As<IScheduleCommand>().SingleInstance();
			builder.RegisterType<FakeDayOffTemplateRepository>().As<IDayOffTemplateRepository>().SingleInstance();
			builder.RegisterType<FakeScheduleDataReadScheduleRepository>().As<IScheduleRepository>().SingleInstance();
			builder.RegisterType<FakeSkillRepository>().As<ISkillRepository>().SingleInstance();
			builder.RegisterType<FakeDayOffsInPeriodCalculator>().As<IDayOffsInPeriodCalculator>().SingleInstance();
			builder.RegisterType<FakePersonRepository>().As<IPersonRepository>().SingleInstance();
			builder.RegisterType<FakeCommonStateHolder>().As<ICommonStateHolder>().SingleInstance();
			builder.RegisterType<FakeSchedulingResultStateHolder>().As<ISchedulingResultStateHolder>().SingleInstance();
			builder.RegisterType<FakeScheduleRangePersister>().As<IScheduleRangePersister>().SingleInstance();
			builder.RegisterType<FakeActivityRepository>().As<IActivityRepository>().SingleInstance();
			builder.RegisterType<FakePersonAbsenceAccountRepository>().As<IPersonAbsenceAccountRepository>().SingleInstance();
			builder.RegisterType<FakeFixedStaffSchedulingService>().As<IFixedStaffSchedulingService>().SingleInstance();
			builder.RegisterType<FakeCurrentUnitOfWorkFactory>().As<ICurrentUnitOfWorkFactory>().SingleInstance();
			builder.RegisterType<FakeUnitOfWorkFactory>().As<IUnitOfWorkFactory>().SingleInstance();
			builder.RegisterInstance<IScenarioRepository>(new FakeScenarioRepository(ScenarioFactory.CreateScenario("Default", true, true))).SingleInstance();
			builder.RegisterInstance<ICurrentTeleoptiPrincipal>(
				new FakeCurrentTeleoptiPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("", null, null, null),
					PersonFactory.CreatePerson(new Name("Anna", "Andersson"), TimeZoneInfo.Utc)))).SingleInstance();
			builder.RegisterType<FakeScheduleTagSetter>().As<IScheduleTagSetter>().SingleInstance();
			builder.RegisterType<FakeGroupPageCreator>().As<IGroupPageCreator>().SingleInstance();
			builder.RegisterType<FakeGroupScheduleGroupPageDataProvider>().As<IGroupScheduleGroupPageDataProvider>().SingleInstance();
			builder.RegisterType<FakeTeamBlockScheduleCommand>().As<ITeamBlockScheduleCommand>().SingleInstance();
			builder.RegisterType<FakeClassicScheduleCommand>().As<IClassicScheduleCommand>().SingleInstance();
			builder.RegisterType<FakeResourceOptimizationHelper>().As<IResourceOptimizationHelper>().SingleInstance();
			builder.RegisterType<FakeScheduleRange>().As<IScheduleRange>().SingleInstance();
			builder.RegisterType<FakeScheduleDictionary>().As<IScheduleDictionary>().SingleInstance();
			builder.RegisterType<FakeScheduleParameters>().As<IScheduleParameters>().SingleInstance();
			builder.RegisterType<FakePlanningPeriodRepository>().As<IPlanningPeriodRepository>().SingleInstance();
			builder.RegisterType<FakeMissingForecastProvider>().As<IMissingForecastProvider>().SingleInstance();
		}
	}
}