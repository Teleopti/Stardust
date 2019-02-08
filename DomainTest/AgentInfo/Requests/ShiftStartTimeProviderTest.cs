using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	[DomainTest]
	public class ShiftStartTimeProviderTest
	{
		public IShiftStartTimeProvider ShiftStartTimeProvider;
		public ICurrentScenario CurrentScenario;
		public FakePersonAssignmentRepository FakeAssignmentRepository;
		public FakeActivityRepository FakeActivityRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;

		[Test]
		public void ShouldGetShiftStartTime()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = ShiftStartTimeProvider.GetShiftStartTimeForPerson(agent, date).Value;

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(8);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetShiftStartTimeWithPartialAbsence()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var absence = new Absence().WithId();
			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 10));
			var personAbsence = new PersonAbsence(agent, CurrentScenario.Current(), absenceLayer);
			PersonAbsenceRepository.Has(personAbsence);

			var result = ShiftStartTimeProvider.GetShiftStartTimeForPerson(agent, date).Value;

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(8);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetShiftStartTimeWithFullDayAbsence()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var absence = new Absence().WithId();
			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			var personAbsence = new PersonAbsence(agent, CurrentScenario.Current(), absenceLayer);
			PersonAbsenceRepository.Has(personAbsence);

			var result = ShiftStartTimeProvider.GetShiftStartTimeForPerson(agent, date).Value;

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(8);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetShiftStartTimeWhenThereIsAConnectedOvertimeActivity()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var overtime = ActivityFactory.CreateActivity("overtime activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			personAssignment.AddOvertimeActivity(overtime, new DateTimePeriod(2018, 1, 8, 07, 2018, 1, 8, 08), MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime definition set", MultiplicatorType.Overtime));
			FakeAssignmentRepository.Has(personAssignment);

			var result = ShiftStartTimeProvider.GetShiftStartTimeForPerson(agent, date).Value;

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(7);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetShiftStartTimeWhenThereIsADisconnectedOvertimeActivity()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var overtime = ActivityFactory.CreateActivity("overtime activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			personAssignment.AddOvertimeActivity(overtime, new DateTimePeriod(2018, 1, 8, 06, 2018, 1, 8, 07), MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime definition set", MultiplicatorType.Overtime));
			FakeAssignmentRepository.Has(personAssignment);

			var result = ShiftStartTimeProvider.GetShiftStartTimeForPerson(agent, date).Value;

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(6);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetShiftStartTimeWhenThereIsAnOvernightShiftThatEndsAtTomorrow()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 18, 2018, 1, 9, 2));
			FakeAssignmentRepository.Has(personAssignment);

			var result = ShiftStartTimeProvider.GetShiftStartTimeForPerson(agent, date).Value;

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(18);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldNotGetShiftStartTimeWithEmptyDay()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var personAssignment = PersonAssignmentFactory.CreateEmptyAssignment(agent, CurrentScenario.Current(),
				new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = ShiftStartTimeProvider.GetShiftStartTimeForPerson(agent, date);

			result.Should().Be(null);
		}

		[Test]
		public void ShouldNotGetShiftStartTimeWithDayOff()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("Slackday", "SD"));
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(agent, CurrentScenario.Current(), date, dayOffTemplate);
			FakeAssignmentRepository.Has(personAssignment);

			var result = ShiftStartTimeProvider.GetShiftStartTimeForPerson(agent, date);

			result.Should().Be(null);
		}

		[Test]
		public void ShouldNotGetShiftStartTimeWhenThereIsAnOvernightShiftThatEndsAtToday()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignmentYesterday = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date.AddDays(-1));
			personAssignmentYesterday.AddActivity(phone, new DateTimePeriod(2018, 1, 7, 18, 2018, 1, 8, 02));
			FakeAssignmentRepository.Has(personAssignmentYesterday);

			var result = ShiftStartTimeProvider.GetShiftStartTimeForPerson(agent, date);

			result.Should().Be(null);
		}
	}
}
