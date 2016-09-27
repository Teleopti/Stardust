﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	public class SchedulerStateHolder_LoadSchedulesTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IScheduleStorage ScheduleStorage; //TODO: Why is this injected?
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		[Test, Ignore("Failing test for #40763")]
		public void ShouldLoadDataEvenIfDifferentTimeZonesSeemsToMakeDataBeOnOtherDay()
		{
			var userTimeZone = TimeZoneInfoFactory.MountainTimeZoneInfo();
			var date = new DateOnly(2000,1,1);
			var scenario = new Scenario("_");
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());
			var assignment = new PersonAssignment(agent, scenario, date).WithId();
			assignment.AddActivity(new Activity("_"), new TimePeriod(1,0,2,0));
			PersonAssignmentRepository.Has(assignment);
			var stateHolder = SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date, date.AddWeeks(1)), new[] {agent}, new[] {assignment}, Enumerable.Empty<ISkillDay>(), userTimeZone);
			stateHolder.SetRequestedScenario(scenario); //TODO: why is this needed?
			var scheduleDateTimePeriod = new ScheduleDateTimePeriod(stateHolder.RequestedPeriod.Period(), stateHolder.SchedulingResultState.PersonsInOrganization); //TODO: why is this injected?

			stateHolder.LoadSchedules(ScheduleStorage, new PersonProvider(new[] { agent }), new ScheduleDictionaryLoadOptions(false, false), scheduleDateTimePeriod);

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment()
				.Should().Be.EqualTo(assignment);
		}
	}
}