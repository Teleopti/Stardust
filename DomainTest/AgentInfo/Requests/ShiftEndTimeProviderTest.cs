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
	public class ShiftEndTimeProviderTest
	{
		public IShiftEndTimeProvider ShiftEndTimeProvider;
		public ICurrentScenario CurrentScenario;
		public FakePersonAssignmentRepository FakeAssignmentRepository;
		public FakeActivityRepository FakeActivityRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		
		[Test]
		public void ShouldGetShiftEndTime()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = ShiftEndTimeProvider.GetShiftEndTimeForPerson(agent, date).Value;

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(17);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetShiftEndTimeWithPartialAbsence()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var absence = new Absence().WithId();
			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(2018, 1, 8, 16, 2018, 1, 8, 17));
			var personAbsence = new PersonAbsence(agent, CurrentScenario.Current(), absenceLayer);
			PersonAbsenceRepository.Has(personAbsence);

			var result = ShiftEndTimeProvider.GetShiftEndTimeForPerson(agent, date).Value;

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(17);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetShiftEndTimeWithFullDayAbsence()
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

			var result = ShiftEndTimeProvider.GetShiftEndTimeForPerson(agent, date).Value;

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(17);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetShiftEndTimeWhenThereIsAConnectedOvertimeActivity()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var overtime = ActivityFactory.CreateActivity("overtime activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			personAssignment.AddOvertimeActivity(overtime, new DateTimePeriod(2018, 1, 8, 17, 2018, 1, 8, 18), MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime definition set", MultiplicatorType.Overtime));
			FakeAssignmentRepository.Has(personAssignment);

			var result = ShiftEndTimeProvider.GetShiftEndTimeForPerson(agent, date).Value;

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(18);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetShiftEndTimeWhenThereIsADisconnectedOvertimeActivity()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var overtime = ActivityFactory.CreateActivity("overtime activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			personAssignment.AddOvertimeActivity(overtime, new DateTimePeriod(2018, 1, 8, 18, 2018, 1, 8, 19), MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime definition set", MultiplicatorType.Overtime));
			FakeAssignmentRepository.Has(personAssignment);

			var result = ShiftEndTimeProvider.GetShiftEndTimeForPerson(agent, date).Value;

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(19);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetShiftEndTimeWhenThereIsAnOvernightShiftThatEndsAtTomorrow()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 18, 2018, 1, 9, 2));
			FakeAssignmentRepository.Has(personAssignment);

			var result = ShiftEndTimeProvider.GetShiftEndTimeForPerson(agent, date).Value;

			result.Date.Should().Be(date.Date.AddDays(1));
			result.Hour.Should().Be(2);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldNotGetShiftEndTimeWithEmptyDay()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var personAssignment = PersonAssignmentFactory.CreateEmptyAssignment(agent, CurrentScenario.Current(),
				new DateTimePeriod(2018, 1, 8, 8, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = ShiftEndTimeProvider.GetShiftEndTimeForPerson(agent, date);

			result.Should().Be(null);
		}

		[Test]
		public void ShouldNotGetShiftEndTimeWithDayOff()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("Slackday", "SD"));
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(agent, CurrentScenario.Current(), date, dayOffTemplate);
			FakeAssignmentRepository.Has(personAssignment);

			var result = ShiftEndTimeProvider.GetShiftEndTimeForPerson(agent, date);

			result.Should().Be(null);
		}

		[Test]
		public void ShouldNotGetShiftEndTimeWhenThereIsAnOvernightShiftThatEndsAtToday()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignmentYesterday = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date.AddDays(-1));
			personAssignmentYesterday.AddActivity(phone, new DateTimePeriod(2018, 1, 7, 18, 2018, 1, 8, 2));
			FakeAssignmentRepository.Has(personAssignmentYesterday);

			var result = ShiftEndTimeProvider.GetShiftEndTimeForPerson(agent, date);

			result.Should().Be(null);
		}
	}
}
