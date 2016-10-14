using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MaxSeat;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	public class MaxSeatOptimizationTest
	{
		public MaxSeatOptimization Target;
		public FakePersonRepository PersonRepository; //TA bort sen
		public FakePersonAssignmentRepository PersonAssignmentRepository; //TA bort sen
		public IScheduleStorage ScheduleStorage; //TA BORT SEN

		[Test]
		public void ShouldConsiderMaxSeat()
		{
			var activity = new Activity("_") {RequiresSeat = true};
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), shiftCategory));
			var site = new Site("_")
			{
				MaxSeats = 1
			};
			var agentScheduledForAnHour = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = site }, schedulePeriod, ruleSet);
			PersonAssignmentRepository.Has(agentScheduledForAnHour, scenario, activity, shiftCategory, dateOnly, new TimePeriod(8, 0, 9, 0)); //should force other agent to start 9
			var agent = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = site }, schedulePeriod, ruleSet);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, dateOnly, new TimePeriod(8, 0, 16, 0));

			//FIXA SEN
			var schedules = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateOnly.ToDateOnlyPeriod().ToDateTimePeriod(TimeZoneInfo.Utc)), scenario, new PersonProvider(new[] { agentScheduledForAnHour, agent }), new ScheduleDictionaryLoadOptions(false, false, false), new[] { agentScheduledForAnHour, agent });
			//

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] {agentScheduledForAnHour, agent}, schedules, scenario);

			schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(9));
		}
	}
}