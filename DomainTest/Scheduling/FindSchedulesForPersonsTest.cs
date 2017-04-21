using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	[TestFixture(true)]
	[TestFixture(false)]
	public class FindSchedulesForPersonsTest
	{
		private readonly bool _loadByPerson;
		public IFindSchedulesForPersons Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		public FindSchedulesForPersonsTest(bool loadByPerson)
		{
			_loadByPerson = loadByPerson;
		}

		[Test]
		public void ShouldIncludeNotSelectedAgentsScheduleOutsideChoosenPeriod()
		{
			var date = new DateOnly(2000,1,1);
			var dateWithNotSelectedAgentAss = date.AddWeeks(3);
			var scenario = new Scenario("_");
			var selectedAgent = new Person().WithSchedulePeriodOneMonth(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			var notSelectedAgent = new Person().WithSchedulePeriodOneDay(dateWithNotSelectedAgentAss).InTimeZone(TimeZoneInfo.Utc).WithId();
			PersonAssignmentRepository.Has(notSelectedAgent, scenario, new Activity("_"), new ShiftCategory("_"), dateWithNotSelectedAgentAss, new TimePeriod(8, 17));

			var schedules = Target.FindSchedulesForPersons(
					new ScheduleDateTimePeriod(date.ToDateTimePeriod(TimeZoneInfo.Utc), new[] { selectedAgent, notSelectedAgent }),
					scenario,
					new PersonProvider(new[] { selectedAgent, notSelectedAgent }) { DoLoadByPerson = _loadByPerson },
					new ScheduleDictionaryLoadOptions(false, false),
					new[] { selectedAgent });

			schedules[notSelectedAgent].ScheduledDay(dateWithNotSelectedAgentAss).PersonAssignment(true).ShiftLayers
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotIncludeNotSelectedAgentsScheduleOutsideChoosenPeriod()
		{
			if (!_loadByPerson)
				Assert.Ignore("If green -> better perf in RAM due to less loaded. Skip this for now though - fix if necessary.");

			var date = DateOnly.Today;
			var dateWithNotSelectedAgentAss = date.AddDays(3);
			var scenario = new Scenario("_");
			var selectedAgent = new Person().WithSchedulePeriodOneDay(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			var notSelectedAgent = new Person().WithSchedulePeriodOneWeek(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			PersonAssignmentRepository.Has(notSelectedAgent, scenario, new Activity("_"), new ShiftCategory("_"), dateWithNotSelectedAgentAss, new TimePeriod(8,17));

			var schedules = Target.FindSchedulesForPersons(
					new ScheduleDateTimePeriod(date.ToDateTimePeriod(TimeZoneInfo.Utc), new[] {selectedAgent, notSelectedAgent}),
					scenario,
					new PersonProvider(new[] {selectedAgent, notSelectedAgent}) {DoLoadByPerson = _loadByPerson },
					new ScheduleDictionaryLoadOptions(false, false),
					new[] {selectedAgent});

			schedules[notSelectedAgent].ScheduledDay(dateWithNotSelectedAgentAss).PersonAssignment(true).ShiftLayers
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldIncludeSelectedAgentsScheduleOutsideChoosenPeriod()
		{
			var date = DateOnly.Today;
			var dateWithNotSelectedAgentAss = date.AddDays(3);
			var scenario = new Scenario("_");
			var selectedAgent = new Person().WithSchedulePeriodOneDay(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			PersonAssignmentRepository.Has(selectedAgent, scenario, new Activity("_"), new ShiftCategory("_"), dateWithNotSelectedAgentAss, new TimePeriod(8, 17));

			var schedules = Target.FindSchedulesForPersons(
					new ScheduleDateTimePeriod(date.ToDateTimePeriod(TimeZoneInfo.Utc), new[] { selectedAgent }),
					scenario,
					new PersonProvider(new[] { selectedAgent }) { DoLoadByPerson = _loadByPerson },
					new ScheduleDictionaryLoadOptions(false, false),
					new[] { selectedAgent });

			schedules[selectedAgent].ScheduledDay(dateWithNotSelectedAgentAss).PersonAssignment(true).ShiftLayers
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldIncludeNotSelectedAgentsScheduleInsideChoosenPeriod()
		{
			var date = DateOnly.Today;
			var scenario = new Scenario("_");
			var selectedAgent = new Person().WithSchedulePeriodOneDay(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			var notSelectedAgent = new Person().WithSchedulePeriodOneWeek(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			PersonAssignmentRepository.Has(notSelectedAgent, scenario, new Activity("_"), new ShiftCategory("_"), date, new TimePeriod(8, 17));

			var schedules = Target.FindSchedulesForPersons(
					new ScheduleDateTimePeriod(date.ToDateTimePeriod(TimeZoneInfo.Utc), new[] { selectedAgent, notSelectedAgent }),
					scenario,
					new PersonProvider(new[] { selectedAgent, notSelectedAgent }) { DoLoadByPerson = _loadByPerson },
					new ScheduleDictionaryLoadOptions(false, false),
					new[] { selectedAgent });

			schedules[notSelectedAgent].ScheduledDay(date).PersonAssignment(true).ShiftLayers
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotIncludeSelectedAgentsScheduleOutsideSelectedAgentPeriod()
		{
			var date = new DateOnly(2000,1,1);
			var dateWithAssignment = date.AddWeeks(3);
			var scenario = new Scenario("_");
			var selectedAgent = new Person().WithSchedulePeriodOneDay(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			var notSelectedAgent = new Person().WithSchedulePeriodOneMonth(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			var selectedAgentAss = new PersonAssignment(selectedAgent, scenario, dateWithAssignment);
			PersonAssignmentRepository.Has(selectedAgentAss);

			var schedules = Target.FindSchedulesForPersons(
					new ScheduleDateTimePeriod(date.ToDateTimePeriod(TimeZoneInfo.Utc), new[] { selectedAgent, notSelectedAgent }),
					scenario,
					new PersonProvider(new[] { selectedAgent, notSelectedAgent }) { DoLoadByPerson = _loadByPerson },
					new ScheduleDictionaryLoadOptions(false, false),
					new[] { selectedAgent });

			schedules[selectedAgent].Contains(selectedAgentAss, true).Should().Be.False();
		}

		[Test]
		public void ShouldIncludeSelectedAgentsScheduleOnceOnly()
		{
			var date = DateOnly.Today;
			var scenario = new Scenario("_");
			var selectedAgent = new Person().WithSchedulePeriodOneDay(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			PersonAssignmentRepository.Has(selectedAgent, scenario, new Activity("_"), new ShiftCategory("_"), date, new TimePeriod(8, 17));

			var schedules = Target.FindSchedulesForPersons(
					new ScheduleDateTimePeriod(date.ToDateTimePeriod(TimeZoneInfo.Utc), new[] { selectedAgent }),
					scenario,
					new PersonProvider(new[] { selectedAgent }) { DoLoadByPerson = _loadByPerson },
					new ScheduleDictionaryLoadOptions(false, false),
					new[] { selectedAgent });

			schedules[selectedAgent].ScheduledDay(date).PersistableScheduleDataCollection().Count()
				.Should().Be.EqualTo(1);
		}
	}
}