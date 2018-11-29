using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Commands
{
	[DomainTest]
	public class AbsenceCommandConverterTest : IIsolateSystem
	{
		public FakePersonRepository PersonRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IScheduleStorage ScheduleStorage;
		public IAbsenceCommandConverter Target;

		[Test]
		public void ShouldConvertAbsenceForFullDay()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsenceWithId();
			AbsenceRepository.Add(absence);
			ScenarioRepository.Has("Default");
			
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = absence.Id.Value,
				PersonId = person.Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			var result = Target.GetCreatorInfoForFullDayAbsence(command);
			result.Absence.Should().Be.EqualTo(absence);
		}

		[Test]
		public void ShouldConvertAbsenceForFullDayWhenPreviousDaysEndIsLaterThanSelectedDaysStart()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsenceWithId();
			AbsenceRepository.Add(absence);
			var scenario = ScenarioRepository.Has("Default");
			var previousDay = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(person, scenario, new DateTimePeriod(2016, 6, 6, 23, 2016, 6, 7, 11));
			PersonAssignmentRepository.Add(previousDay);
			var selectedDay = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 6, 7, 9, 2016, 6, 7, 10));
			PersonAssignmentRepository.Add(selectedDay);
			
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = absence.Id.Value,
				PersonId = person.Id.Value,
				StartDate = new DateTime(2016, 6, 7),
				EndDate = new DateTime(2016, 6, 7),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			var result = Target.GetCreatorInfoForFullDayAbsence(command);
			result.Absence.Should().Be.EqualTo(absence);
		}

		[Test]
		public void ShouldConvertAbsenceForFullDayWhenSelectedDaysEndIsLaterThanNextDaysEnd()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsenceWithId();
			AbsenceRepository.Add(absence);
			var scenario = ScenarioRepository.Has("Default");
			var selectedDay = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(person, scenario, new DateTimePeriod(2016, 6, 6, 23, 2016, 6, 7, 11));
			PersonAssignmentRepository.Add(selectedDay);
			var nextDay = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 6, 7, 9, 2016, 6, 7, 10));
			PersonAssignmentRepository.Add(nextDay);
			
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = absence.Id.Value,
				PersonId = person.Id.Value,
				StartDate = new DateTime(2016, 6, 6),
				EndDate = new DateTime(2016, 6, 7),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			var result = Target.GetCreatorInfoForFullDayAbsence(command);
			result.Absence.Should().Be.EqualTo(absence);
			result.AbsenceTimePeriod.Should().Be.EqualTo(new DateTimePeriod(2016, 6, 6, 23, 2016, 6, 7, 11));
		}

		[Test]
		public void ShouldConvertPersonForFullDay()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsenceWithId();
			AbsenceRepository.Add(absence);
			ScenarioRepository.Has("Default");
			
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = absence.Id.Value,
				PersonId = person.Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			var result = Target.GetCreatorInfoForFullDayAbsence(command);
			result.Person.Should().Be.EqualTo(person);
		}			
		
		[Test]
		public void ShouldConvertTrackedCommandInfoForFullDay()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsenceWithId();
			AbsenceRepository.Add(absence);
			ScenarioRepository.Has("Default");
			
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = absence.Id.Value,
				PersonId = person.Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			var result = Target.GetCreatorInfoForFullDayAbsence(command);
			result.TrackedCommandInfo.OperatedPersonId.Should().Be.EqualTo(operatedPersonId);
			result.TrackedCommandInfo.TrackId.Should().Be.EqualTo(trackId);
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePersonRepository>().For<IProxyForId<IPerson>>();
			isolate.UseTestDouble<FakeAbsenceRepository>().For<IProxyForId<IAbsence>>();
		}
	}
}
