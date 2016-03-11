using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
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

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FillSchedulerStateHolder>().For<IFillSchedulerStateHolder>();
			system.UseTestDouble<SynchronizeSchedulerStateHolderDesktop>().For<ISynchronizeIntradayOptimizationResult>();
		}
	}
}