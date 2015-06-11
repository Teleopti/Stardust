using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class ResourcePlannerTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ISystem builder, IIocConfiguration configuration)
		{
			builder.RegisterModule(new WebModule(configuration, null));
			builder.RegisterModule(new SchedulingCommonModule());
			builder.RegisterModule(new ResourcePlannerModule());

			builder.UseTestDouble<FakePeopleAndSkillLoaderDecider>().For<IPeopleAndSkillLoaderDecider>();
			builder.UseTestDouble<FakeRequiredScheduleHelper>().For<IRequiredScheduleHelper>();
			builder.UseTestDouble<FakeScheduleCommand>().For<IScheduleCommand>();
			builder.UseTestDouble<FakeDayOffTemplateRepository>().For<IDayOffTemplateRepository>();
			builder.UseTestDouble<FakeScheduleDataReadScheduleRepository>().For<IScheduleRepository>();
			builder.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			builder.UseTestDouble<FakeDayOffsInPeriodCalculator>().For<IDayOffsInPeriodCalculator>();
			builder.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			builder.UseTestDouble<FakeCommonStateHolder>().For<ICommonStateHolder>();
			builder.UseTestDouble<FakeSchedulingResultStateHolder>().For<ISchedulingResultStateHolder>();
			builder.UseTestDouble<FakeScheduleRangePersister>().For<IScheduleRangePersister>();
			builder.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			builder.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			builder.UseTestDouble<FakeFixedStaffSchedulingService>().For<IFixedStaffSchedulingService>();
			builder.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			builder.UseTestDouble<FakeUnitOfWorkFactory>().For<IUnitOfWorkFactory>();
			builder.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("Default", true, true))).For<IScenarioRepository>();
			builder.UseTestDouble(new FakeCurrentTeleoptiPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("", null, null, null), PersonFactory.CreatePerson(new Name("Anna", "Andersson"), TimeZoneInfo.Utc)))).For<ICurrentTeleoptiPrincipal>();
			builder.UseTestDouble<FakeScheduleTagSetter>().For<IScheduleTagSetter>();
			builder.UseTestDouble<FakeGroupPageCreator>().For<IGroupPageCreator>();
			builder.UseTestDouble<FakeGroupScheduleGroupPageDataProvider>().For<IGroupScheduleGroupPageDataProvider>();
			builder.UseTestDouble<FakeTeamBlockScheduleCommand>().For<ITeamBlockScheduleCommand>();
			builder.UseTestDouble<FakeClassicScheduleCommand>().For<IClassicScheduleCommand>();
			builder.UseTestDouble<FakeResourceOptimizationHelper>().For<IResourceOptimizationHelper>();
			builder.UseTestDouble<FakeScheduleRange>().For<IScheduleRange>();
			builder.UseTestDouble<FakeScheduleDictionary>().For<IScheduleDictionary>();
			builder.UseTestDouble<FakeScheduleParameters>().For<IScheduleParameters>();
			builder.UseTestDouble<FakePlanningPeriodRepository>().For<IPlanningPeriodRepository>();
			builder.UseTestDouble<FakeMissingForecastProvider>().For<IMissingForecastProvider>();
		}
	}
}