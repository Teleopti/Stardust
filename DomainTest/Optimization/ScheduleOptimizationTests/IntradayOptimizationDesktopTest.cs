using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ScheduleOptimizationTests
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_IntradayIslands_36939)]
	[UseEventPublisher(typeof(RunInProcessEventPublisher))]
	public class IntradayOptimizationDesktopTest : ISetup
	{
		public IIntradayOptimizationCommandHandler Target;
		public FillSchedulerStateHolder FillSchedulerStateHolder;

		[Test]
		public void ShouldFillSchedulerState()
		{
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");

			var agent = new Person().WithId();
			agent.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()),
				new Skill().WithId());

			var agents = new List<IPerson> { agent };
			var schedulingResultStateHolder = new FakeSchedulingResultStateHolder { PersonsInOrganization = agents };
			var dateTimePeriod = new DateTimePeriod(2015, 10, 12, 2015, 10, 12);
			var schedules = new ScheduleDictionaryForTest(scenario, dateTimePeriod);
			var range = new FakeScheduleRange(schedules, new ScheduleParameters(scenario, agent, dateTimePeriod));
			schedules.AddTestItem(agent, range);
			schedulingResultStateHolder.Schedules = schedules;
			var schedulerStateFrom = new SchedulerStateHolder(null, null, agents, null, schedulingResultStateHolder, new TimeZoneGuardWrapper());

			using (FillSchedulerStateHolder.Add(schedulerStateFrom))
			{
				Target.Execute(new IntradayOptimizationCommand
				{
					Agents = agents,
					Period = new DateOnlyPeriod(dateOnly, dateOnly)
				});

				Assert.AreEqual(FillSchedulerStateHolder.FilledSchedulerStateHolder.AllPermittedPersons.First(), agent);
			}
		}

		[Test, Ignore]
		public void ShouldSynchronizeScheduleStateHolder()
		{
			var agent = new Person().WithId();
			var agents = new List<IPerson> {agent};
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var skill = SkillFactory.CreateSkill("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateTimePeriod = new DateTimePeriod(2015, 10, 12, 2015, 10, 13);
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var dateOnly = new DateOnly(dateTimePeriod.StartDateTime);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("_") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			var scheduleParameters = new ScheduleParameters(scenario, agent, dateTimePeriod);
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(dateTimePeriod), new Dictionary<IPerson, IScheduleRange>());
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			var schedulingResultStateHolder = new FakeSchedulingResultStateHolder { PersonsInOrganization = agents };	
			var timeZoneGuardWrapper = new TimeZoneGuardWrapper();
			var scheduleRange = new ScheduleRange(scheduleDictionary, scheduleParameters);

			skill.Activity = phoneActivity;
			skill.SetId(Guid.NewGuid());
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var skillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			
			agent.AddPersonPeriod(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_")}));
			agent.AddSchedulePeriod(schedulePeriod);
			agent.AddSkill(skill, dateOnly);
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			
			ass.AddActivity(phoneActivity, timePeriod);
			ass.SetShiftCategory(shiftCategory);
			
            scheduleRange.Add(ass);
			scheduleDictionary.UsePermissions(false);
			scheduleDictionary.AddTestItem(agent, scheduleRange);	
			schedulingResultStateHolder.Schedules = scheduleDictionary;		
			schedulingResultStateHolder.SkillDays = skillDays;

			var schedulerStateHolderBase = new SchedulerStateHolder(scenario, 
																new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), 
																timeZoneGuardWrapper.CurrentTimeZone()), 
																agents, 
																null, 
																schedulingResultStateHolder, 
																timeZoneGuardWrapper);

			using (FillSchedulerStateHolder.Add(schedulerStateHolderBase))
			{
				Target.Execute(new IntradayOptimizationCommand
				{
					Agents = agents,
					Period = new DateOnlyPeriod(dateOnly, dateOnly)
				});

				var scheduleDay = schedulerStateHolderBase.Schedules[agent].ScheduledDay(dateOnly);
				scheduleDay.PersonAssignment().Period.Should().Be.EqualTo(new DateTimePeriod(dateTime.AddHours(8).AddMinutes(15), dateTime.AddHours(17).AddMinutes(15)));
			}
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FillSchedulerStateHolder>().For<IFillSchedulerStateHolder>();
			system.UseTestDouble<SynchronizeSchedulerStateHolderDesktop>().For<ISynchronizeIntradayOptimizationResult>();
		}
	}
}