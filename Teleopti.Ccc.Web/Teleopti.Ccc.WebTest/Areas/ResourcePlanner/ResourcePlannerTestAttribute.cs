using System;
using Teleopti.Ccc.Domain.AgentInfo;
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
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebModule(configuration, null));
			system.AddModule(new SchedulingCommonModule());
			system.AddModule(new ResourcePlannerModule());

			system.UseTestDouble<FakePeopleAndSkillLoaderDecider>().For<IPeopleAndSkillLoaderDecider>();
			system.UseTestDouble<FakeRequiredScheduleHelper>().For<IRequiredScheduleHelper>();
			system.UseTestDouble<FakeScheduleCommand>().For<IScheduleCommand>();
			system.UseTestDouble<FakeDayOffTemplateRepository>().For<IDayOffTemplateRepository>();
			system.UseTestDouble<FakeScheduleDataReadScheduleRepository>().For<IScheduleRepository>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakeDayOffsInPeriodCalculator>().For<IDayOffsInPeriodCalculator>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakeCommonStateHolder>().For<ICommonStateHolder>();
			system.UseTestDouble<FakeSchedulingResultStateHolder>().For<ISchedulingResultStateHolder>();
			system.UseTestDouble<FakeScheduleRangePersister>().For<IScheduleRangePersister>();
			system.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			system.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			system.UseTestDouble<FakeFixedStaffSchedulingService>().For<IFixedStaffSchedulingService>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FakeUnitOfWorkFactory>().For<IUnitOfWorkFactory>();
			system.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("Default", true, true))).For<IScenarioRepository>();
			system.UseTestDouble(new FakeCurrentTeleoptiPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("", null, null, null), PersonFactory.CreatePerson(new Name("Anna", "Andersson"), TimeZoneInfo.Utc)))).For<ICurrentTeleoptiPrincipal>();
			system.UseTestDouble<FakeScheduleTagSetter>().For<IScheduleTagSetter>();
			system.UseTestDouble<FakeGroupPageCreator>().For<IGroupPageCreator>();
			system.UseTestDouble<FakeGroupScheduleGroupPageDataProvider>().For<IGroupScheduleGroupPageDataProvider>();
			system.UseTestDouble<FakeTeamBlockScheduleCommand>().For<ITeamBlockScheduleCommand>();
			system.UseTestDouble<FakeClassicScheduleCommand>().For<IClassicScheduleCommand>();
			system.UseTestDouble<FakeResourceOptimizationHelper>().For<IResourceOptimizationHelper>();
			system.UseTestDouble<FakeScheduleRange>().For<IScheduleRange>();
			system.UseTestDouble<FakeScheduleDictionary>().For<IScheduleDictionary>();
			system.UseTestDouble<FakeScheduleParameters>().For<IScheduleParameters>();
			system.UseTestDouble<FakePlanningPeriodRepository>().For<IPlanningPeriodRepository>();
			system.UseTestDouble<FakeFixedStaffLoader>().For<IFixedStaffLoader>();
			system.UseTestDouble<FakeMissingForecastProvider>().For<IMissingForecastProvider>();
			system.UseTestDouble<FakeClassicDaysOffOptimizationCommand>().For<IClassicDaysOffOptimizationCommand>();
		}
	}
}