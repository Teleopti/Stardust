using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
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
	public class ResourcePlannerTest_DontUseAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebModule(configuration, null));
			system.AddModule(new SchedulingCommonModule(configuration));
			system.AddModule(new ResourcePlannerModule());

			system.UseTestDouble<FakeScheduleDataReadScheduleRepository>().For<IScheduleRepository>();
			system.UseTestDouble<FakeSchedulingResultStateHolder>().For<ISchedulingResultStateHolder>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FakeUnitOfWorkFactory>().For<IUnitOfWorkFactory>();
			system.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("Default", true, true))).For<IScenarioRepository>();
			system.UseTestDouble(new FakeCurrentTeleoptiPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("", null, null, null), PersonFactory.CreatePerson(new Name("Anna", "Andersson"), TimeZoneInfo.Utc)))).For<ICurrentTeleoptiPrincipal>();
			system.UseTestDouble<FakeScheduleTagSetter>().For<IScheduleTagSetter>();
			system.UseTestDouble<FakeGroupPageCreator>().For<IGroupPageCreator>();
			system.UseTestDouble<FakeScheduleRange>().For<IScheduleRange>();
			system.UseTestDouble<FakeScheduleDictionary>().For<IScheduleDictionary>();
			system.UseTestDouble<FakeScheduleParameters>().For<IScheduleParameters>();
			system.UseTestDouble(new FakeTimeZoneGuard(TimeZoneInfo.Utc)).For<ITimeZoneGuard>();
			system.UseTestDouble<FakePlanningPeriodRepository>().For<IPlanningPeriodRepository>();
			system.UseTestDouble<FakeFixedStaffLoader>().For<IFixedStaffLoader>();
			system.UseTestDouble<FakeMissingForecastProvider>().For<IMissingForecastProvider>();
			system.UseTestDouble<FakeDayOffRulesRepository>().For<IDayOffRulesRepository>();
			system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}
	}
}