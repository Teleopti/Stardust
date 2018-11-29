using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	public class SchedulerStateHolder_LoadSchedulesTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IFindSchedulesForPersons ScheduleStorage;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		[Test]
		public void ShouldLoadDataEvenIfDifferentTimeZonesSeemsToMakeDataBeOnOtherDay()
		{
			var userTimeZone = TimeZoneInfoFactory.MountainTimeZoneInfo();
			var date = new DateOnly(2000,1,1);
			var scenario = new Scenario("_");
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());
			var assignment = new PersonAssignment(agent, scenario, date).WithId();
			assignment.AddActivity(new Activity("_"), new TimePeriod(1,0,2,0));
			PersonAssignmentRepository.Has(assignment);
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] {agent}, new[] {assignment}, Enumerable.Empty<ISkillDay>(), userTimeZone);
			stateHolder.SetRequestedScenario(scenario);

			stateHolder.LoadSchedules(ScheduleStorage, new[] { agent }, new ScheduleDictionaryLoadOptions(false, false), stateHolder.RequestedPeriod.Period());

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment()
				.Should().Be.EqualTo(assignment);
		}
	}
}