using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class RemoveShiftCommandHandlerTest
	{
		public IHandleCommand<RemoveShiftCommand> Target;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeMeetingRepository MeetingRepository;

		[Test]
		public void ShouldDeleteWholeShiftOnGivenDay()
		{
			var person = PersonFactory.CreatePerson("test");
			PersonRepository.Has(person);
			var scenario = ScenarioFactory.CreateScenario("test", true, false);
			ScenarioRepository.Has(scenario);
			var personAssigment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018, 2, 2, 8, 2018, 2, 2, 17)).WithId();
			PersonAssignmentRepository.Has(personAssigment);
			var command = new RemoveShiftCommand
			{
				Date = new DateOnly(2018, 2, 2),
				Person = person,
				TrackedCommandInfo = new TrackedCommandInfo {TrackId = Guid.NewGuid()}
			};

			Target.Handle(command);

			PersonAssignmentRepository.Get(personAssigment.Id.GetValueOrDefault()).ShiftLayers.Should().Be.Empty();
		}

		[Test]
		public void ShouldGiveErrorWhenSelectedPersonScheduleIsFullDayAbsence()
		{
			var person = PersonFactory.CreatePerson("test");
			PersonRepository.Has(person);
			var scenario = ScenarioFactory.CreateScenario("test", true, false);
			ScenarioRepository.Has(scenario);
			var scheduleTimePeriod = new DateTimePeriod(2018, 2, 2, 8, 2018, 2, 2, 17);
			var personAssigment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, scheduleTimePeriod).WithId();
			PersonAssignmentRepository.Has(personAssigment);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, scheduleTimePeriod).WithId();
			PersonAbsenceRepository.Add(personAbsence);
			var command = new RemoveShiftCommand
			{
				Date = new DateOnly(2018, 2, 2),
				Person = person,
				TrackedCommandInfo = new TrackedCommandInfo {TrackId = Guid.NewGuid()}
			};

			Target.Handle(command);

			PersonAssignmentRepository.Get(personAssigment.Id.GetValueOrDefault()).MainActivities().Count().Should().Be.EqualTo(personAssigment.MainActivities().Count());
			PersonAbsenceRepository.Get(personAbsence.Id.GetValueOrDefault()).Layer.Period.StartDateTime.Should().Be
				.EqualTo(scheduleTimePeriod.StartDateTime);
			PersonAbsenceRepository.Get(personAbsence.Id.GetValueOrDefault()).Layer.Period.EndDateTime.Should().Be
				.EqualTo(scheduleTimePeriod.EndDateTime);
			command.ErrorMessages.Single().Should().Be.EqualTo(Resources.CanNotRemoveShiftForAgentWithFullDayAbsence);
		}

		[Test]
		public void ShouldGiveErrorWhenSelectedPersonScheduleIsDayOff()
		{
			var person = PersonFactory.CreatePerson("test");
			PersonRepository.Has(person);
			var scenario = ScenarioFactory.CreateScenario("test", true, false);
			ScenarioRepository.Has(scenario);
			var scheduleTimePeriod = new DateTimePeriod(2018, 2, 2, 8, 2018, 2, 2, 17);
			var personAssigment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, scheduleTimePeriod).WithId();
			personAssigment.SetDayOff(DayOffFactory.CreateDayOff(new Description("day off")));
			PersonAssignmentRepository.Has(personAssigment);
			var command = new RemoveShiftCommand
			{
				Date = new DateOnly(2018, 2, 2),
				Person = person,
				TrackedCommandInfo = new TrackedCommandInfo {TrackId = Guid.NewGuid()}
			};

			Target.Handle(command);

			PersonAssignmentRepository.Get(personAssigment.Id.GetValueOrDefault()).DayOff().Description.Name.Should().Be.EqualTo("day off");
			command.ErrorMessages.Single().Should().Be.EqualTo(Resources.CanNotRemoveShiftForAgentWithDayOff);
		}

		[Test]
		public void ShouldGiveErrorWhenSelectedPersonScheduleIsEmpty()
		{
			var person = PersonFactory.CreatePerson("test");
			PersonRepository.Has(person);
			var date = new DateOnly(2018, 2, 2);
			var scenario = ScenarioFactory.CreateScenario("test", true, false);
			ScenarioRepository.Has(scenario);
			var personAssigment =
				PersonAssignmentFactory.CreatePersonAssignment(person, scenario,date).WithId();
			PersonAssignmentRepository.Has(personAssigment);
			var command = new RemoveShiftCommand
			{
				Date = date,
				Person = person,
				TrackedCommandInfo = new TrackedCommandInfo {TrackId = Guid.NewGuid()}
			};

			Target.Handle(command);

			command.ErrorMessages.Single().Should().Be.EqualTo(Resources.CanNotRemoveShiftForAgentWithEmptySchedule);
		}
		[Test]
		public void ShouldGiveErrorWhenSelectedPersonScheduleIsNull()
		{
			var person = PersonFactory.CreatePerson("test");
			PersonRepository.Has(person);
			var date = new DateOnly(2018, 2, 2);
			var scenario = ScenarioFactory.CreateScenario("test", true, false);
			ScenarioRepository.Has(scenario);

			var command = new RemoveShiftCommand
			{
				Date = date,
				Person = person,
				TrackedCommandInfo = new TrackedCommandInfo {TrackId = Guid.NewGuid()}
			};

			Target.Handle(command);

			command.ErrorMessages.Single().Should().Be.EqualTo(Resources.CanNotRemoveShiftForAgentWithEmptySchedule);
		}

		[Test]
		public void ShouldKeepOvertimeActivity()
		{
			var person = PersonFactory.CreatePerson("test");
			PersonRepository.Has(person);
			var date = new DateOnly(2018, 2, 2);
			var scheduleTimePeriod = new DateTimePeriod(2018, 2, 2, 8, 2018, 2, 2, 17);
			var overtimePeriod = new DateTimePeriod(2018, 2, 2, 16, 2018, 2, 2, 20);
			var scenario = ScenarioFactory.CreateScenario("test", true, false);
			ScenarioRepository.Has(scenario);
			var overtimeAct = new Activity("overtime").WithId();
			var personAssigment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, scheduleTimePeriod)
				.WithId();
			personAssigment.AddOvertimeActivity(overtimeAct, overtimePeriod, 
				new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));
			PersonAssignmentRepository.Has(personAssigment);

			var command = new RemoveShiftCommand
			{
				Date = date,
				Person = person,
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(command);

			personAssigment = PersonAssignmentRepository.Get(personAssigment.Id.Value);
			personAssigment.MainActivities().Count().Should().Be.EqualTo(0);
			personAssigment.OvertimeActivities().Single().Payload.Name.Should().Be.EqualTo("overtime");
		}

		[Test]
		public void ShouldKeepPersonalActivity()
		{
			var person = PersonFactory.CreatePerson("test");
			PersonRepository.Has(person);
			var date = new DateOnly(2018, 2, 2);
			var scheduleTimePeriod = new DateTimePeriod(2018, 2, 2, 8, 2018, 2, 2, 17);
			var personalActivityPeriod = new DateTimePeriod(2018, 2, 2, 16, 2018, 2, 2, 20);
			var scenario = ScenarioFactory.CreateScenario("test", true, false);
			ScenarioRepository.Has(scenario);
			var personalAct = new Activity("personal activity").WithId();
			var personAssigment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, scheduleTimePeriod)
					.WithId();
			personAssigment.AddPersonalActivity(personalAct, personalActivityPeriod);
			PersonAssignmentRepository.Has(personAssigment);

			var command = new RemoveShiftCommand
			{
				Date = date,
				Person = person,
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(command);

			personAssigment = PersonAssignmentRepository.Get(personAssigment.Id.Value);
			personAssigment.MainActivities().Count().Should().Be.EqualTo(0);
			personAssigment.PersonalActivities().Single().Payload.Name.Should().Be.EqualTo("personal activity");
		}


		[Test]
		public void ShouldKeepIntradayAbsence()
		{
			var person = PersonFactory.CreatePerson("test");
			PersonRepository.Has(person);
			var date = new DateOnly(2018, 2, 2);
			var scheduleTimePeriod = new DateTimePeriod(2018, 2, 2, 8, 2018, 2, 2, 17);
			var intradayAbsencePeriod = new DateTimePeriod(2018, 2, 2, 16, 2018, 2, 2, 20);
			var scenario = ScenarioFactory.CreateScenario("test", true, false);
			ScenarioRepository.Has(scenario);
			var personAssigment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, scheduleTimePeriod)
					.WithId();
			PersonAssignmentRepository.Has(personAssigment);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, intradayAbsencePeriod).WithId();
			PersonAbsenceRepository.Add(personAbsence);

			var command = new RemoveShiftCommand
			{
				Date = date,
				Person = person,
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(command);

			personAssigment = PersonAssignmentRepository.Get(personAssigment.Id.Value);
			personAssigment.MainActivities().Count().Should().Be.EqualTo(0);
			PersonAbsenceRepository.LoadAll().Count().Should().Be.EqualTo(1);
		}
		[Test]
		public void ShouldKeepPersonMeeting()
		{
			var person = PersonFactory.CreatePerson("test");
			PersonRepository.Has(person);
			var date = new DateOnly(2018, 2, 2);
			var scheduleTimePeriod = new DateTimePeriod(2018, 2, 2, 8, 2018, 2, 2, 17);
			var scenario = ScenarioFactory.CreateScenario("test", true, false);
			ScenarioRepository.Has(scenario);
			var personAssigment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, scheduleTimePeriod)
					.WithId();
			PersonAssignmentRepository.Has(personAssigment);
			var meeting = new Meeting(person,
				new List<IMeetingPerson>{new MeetingPerson(person, false)},
				"subject",
				"location",
				"description",
				ActivityFactory.CreateActivity("meeting"),
				scenario);
			MeetingRepository.Has(meeting);


			var command = new RemoveShiftCommand
			{
				Date = date,
				Person = person,
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};
			Target.Handle(command);

			personAssigment = PersonAssignmentRepository.Get(personAssigment.Id.Value);
			personAssigment.MainActivities().Count().Should().Be.EqualTo(0);
			MeetingRepository.LoadAll().Single().GetSubject(new NoFormatting()).Should().Be.EqualTo("subject");
		}

		[Test]
		public void ShouldRaiseEvent()
		{
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);
			var date = new DateOnly(2018, 2, 2);
			var scheduleTimePeriod = new DateTimePeriod(2018, 2, 2, 8, 2018, 2, 2, 17);
			var scenario = ScenarioFactory.CreateScenario("test", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var personAssigment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, scheduleTimePeriod)
					.WithId();
			PersonAssignmentRepository.Has(personAssigment);

			var command = new RemoveShiftCommand
			{
				Date = date,
				Person = person,
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			Target.Handle(command);

			var events = PersonAssignmentRepository.Get(personAssigment.Id.GetValueOrDefault()).PopAllEvents(null).OfType<PersonAssignmentLayerRemovedEvent>();
			events.Single().CommandId.Should().Be.EqualTo(command.TrackedCommandInfo.TrackId);
		}
	}
}