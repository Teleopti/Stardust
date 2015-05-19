using System;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[ResourcePlannerTest]
	public class ScheduleControllerTest
	{
		public ScheduleController Target;
		public IScheduleCommand ScheduleCommand;
		public IPersonRepository FakePersonRepository;
		public ISchedulingResultStateHolder FakeSchedulingResultStateHolder;
		public IScheduleRepository FakeScheduleDataReadScheduleRepository;

		[Test]
		public void ShouldScheduleFixedStaff()
		{
			var period = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(period.StartDate);
			agent.SetId(Guid.NewGuid());
			FakePersonRepository.Add(agent);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true);
			var dateTimePeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
			var schedules = new ScheduleDictionaryForTest(scenario, dateTimePeriod);
			var range = new ScheduleRange(schedules, new ScheduleParameters(scenario, agent, dateTimePeriod));
			schedules.AddTestItem(agent, range);
			FakeSchedulingResultStateHolder.Schedules = schedules;
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				var result =
					(OkNegotiatedContentResult<SchedulingResultModel>)
						Target.FixedStaff(new FixedStaffSchedulingInput {StartDate = period.StartDate.Date, EndDate = period.EndDate.Date});

				result.Content.Should().Not.Be.Null();
			}
			((FakeScheduleCommand)ScheduleCommand).Executed.Should().Be.True();
		}

		[Test]
		public void ShouldReturnPersonWithSchedulePeriodOutofPlanningPeriod()
		{
			var period = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			IPerson agent1 = PersonFactory.CreatePersonWithPersonPeriodTeamSite(period.StartDate);
			agent1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(agent1, period.StartDate.AddDays(3));
			agent1.SetId(Guid.NewGuid());
			FakePersonRepository.Add(agent1);
			
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				var result =
					(OkNegotiatedContentResult<SchedulingResultModel>)
						Target.FixedStaff(new FixedStaffSchedulingInput { StartDate = period.StartDate.Date, EndDate = period.EndDate.Date });

				result.Content.Should().Not.Be.Null();
				result.Content.BusinessRulesValidationResults.ToList().Count.Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldReturnTargetDayOffNotFullfilled()
		{
			var period = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			IPerson agent1 = PersonFactory.CreatePersonWithPersonPeriodTeamSite(period.StartDate);
			agent1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(agent1, period.StartDate);
			agent1.SetId(Guid.NewGuid());
			FakePersonRepository.Add(agent1);
			((FakeScheduleDataReadScheduleRepository) FakeScheduleDataReadScheduleRepository).InitRangeValues(8,7,TimeSpan.Zero,TimeSpan.Zero); 
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				var result =
					(OkNegotiatedContentResult<SchedulingResultModel>)
						Target.FixedStaff(new FixedStaffSchedulingInput { StartDate = period.StartDate.Date, EndDate = period.EndDate.Date });

				result.Content.Should().Not.Be.Null();
				result.Content.BusinessRulesValidationResults.ToList().Count.Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldReturnNumberOfScheduledAgents()
		{
			var period = new DateOnlyPeriod(2015, 5, 1, 2015, 5, 31);
			IPerson agent1 = PersonFactory.CreatePersonWithPersonPeriodTeamSite(period.StartDate);
			agent1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(agent1, period.StartDate, ContractFactory.CreateContract("hourly"),PartTimePercentageFactory.CreatePartTimePercentage("hourly"));
			((FakeScheduleDataReadScheduleRepository)FakeScheduleDataReadScheduleRepository).InitRangeValues(8, 8, TimeSpan.FromHours(8), TimeSpan.FromHours(7)); 
			agent1.SetId(Guid.NewGuid());
			FakePersonRepository.Add(agent1);
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				var result =
					(OkNegotiatedContentResult<SchedulingResultModel>)
						Target.FixedStaff(new FixedStaffSchedulingInput { StartDate = period.StartDate.Date, EndDate = period.EndDate.Date });

				result.Content.Should().Not.Be.Null();
				result.Content.ScheduledAgentsCount.Should().Be.EqualTo(1);
			}
		}
	}
}