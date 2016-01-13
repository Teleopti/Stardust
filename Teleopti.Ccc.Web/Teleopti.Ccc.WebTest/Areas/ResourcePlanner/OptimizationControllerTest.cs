using System;
using System.Collections.Generic;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[ResourcePlannerTest]
	public class OptimizationControllerTest
	{
		public OptimizationController Target;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public MutableNow Now;
		public FakeFixedStaffLoader FixedStaffLoader;
		public FakeSchedulingResultStateHolder SchedulingResultStateHolder;
		public FakeClassicDaysOffOptimizationCommand OptimizationCommand;

		[Test]
		public void ShouldOptimizeFixedStaff()
		{
			var id = Guid.NewGuid();
			Now.Is(new DateTime(2015, 04, 05));
			var planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(Now, new List<AggregatedSchedulePeriod>()));
			planningPeriod.SetId(id);
			PlanningPeriodRepository.Add(planningPeriod);
			var period = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(period.StartDate);
			agent.SetId(Guid.NewGuid());
			FixedStaffLoader.SetPeople(agent);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true);
			var dateTimePeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
			var schedules = new ScheduleDictionaryForTest(scenario, dateTimePeriod);
			var range = new ScheduleRange(schedules, new ScheduleParameters(scenario, agent, dateTimePeriod));
			schedules.AddTestItem(agent, range);
			SchedulingResultStateHolder.Schedules = schedules;
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				var result =
					(OkNegotiatedContentResult<OptimizationResultModel>)
						Target.FixedStaff(id, new BlockToken { Action = "Scheduling" });

				result.Content.Should().Not.Be.Null();
			}
			OptimizationCommand.OptimizationExecute.Should().Be.True();
		}

		[Test]
		public void ShouldUpdatePlanningPeriodState()
		{
			var id = Guid.NewGuid();
			Now.Is(new DateTime(2015, 04, 05));
			var planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(Now, new List<AggregatedSchedulePeriod>()));
			planningPeriod.SetId(id);
			PlanningPeriodRepository.Add(planningPeriod);
			var period = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(period.StartDate);
			agent.SetId(Guid.NewGuid());
			FixedStaffLoader.SetPeople(agent);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true);
			var dateTimePeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
			var schedules = new ScheduleDictionaryForTest(scenario, dateTimePeriod);
			var range = new ScheduleRange(schedules, new ScheduleParameters(scenario, agent, dateTimePeriod));
			schedules.AddTestItem(agent, range);
			SchedulingResultStateHolder.Schedules = schedules;
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				Target.FixedStaff(id, new BlockToken{Action = "Scheduling"});
			}
			PlanningPeriodRepository.Load(id).State.Should().Be.EqualTo(PlanningPeriodState.Scheduled);
		}
	}
}
