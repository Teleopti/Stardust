using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Commands
{
	public class AbsenceCommandConverterTest
	{
		private FakeWriteSideRepository<IPerson> _personRepository;
		private FakeWriteSideRepository<IAbsence> _absenceRepository;
		private FakeCurrentScenario _currentScenario;
		private FakeScheduleStorage _scheduleStorage;

		[SetUp]
		public void Setup()
		{
			_personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			_absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			_currentScenario = new FakeCurrentScenario();
			_scheduleStorage = new FakeScheduleStorage();
		}

		[Test]
		public void ShouldConvertAbsenceForFullDay()
		{
			var target = new AbsenceCommandConverter(_currentScenario,_personRepository, _absenceRepository, _scheduleStorage, null);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			var result = target.GetCreatorInfoForFullDayAbsence(command);
			result.Absence.Should().Be.EqualTo(_absenceRepository.Single());
		}

		[Test]
		public void ShouldConvertAbsenceForFullDayWhenPreviousDaysEndIsLaterThanSelectedDaysStart()
		{
			var person = _personRepository.Single();
			var scenario = _currentScenario.Current();
			var previousDay = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(person, scenario, new DateTimePeriod(2016, 6, 6, 23, 2016, 6, 7, 11));
			_scheduleStorage.Add(previousDay);
			var selectedDay = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 6, 7, 9, 2016, 6, 7, 10));
			_scheduleStorage.Add(selectedDay);
			var target = new AbsenceCommandConverter(_currentScenario, _personRepository, _absenceRepository, _scheduleStorage, null);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartDate = new DateTime(2016, 6, 7),
				EndDate = new DateTime(2016, 6, 7),
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			var result = target.GetCreatorInfoForFullDayAbsence(command);
			result.Absence.Should().Be.EqualTo(_absenceRepository.Single());
		}

		[Test]
		public void ShouldConvertAbsenceForFullDayWhenSelectedDaysEndIsLaterThanNextDaysEnd()
		{
			var person = _personRepository.Single();
			var scenario = _currentScenario.Current();
			var selectedDay = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(person, scenario, new DateTimePeriod(2016, 6, 6, 23, 2016, 6, 7, 11));
			_scheduleStorage.Add(selectedDay);
			var nextDay = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 6, 7, 9, 2016, 6, 7, 10));
			_scheduleStorage.Add(nextDay);
			var target = new AbsenceCommandConverter(_currentScenario, _personRepository, _absenceRepository, _scheduleStorage, null);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartDate = new DateTime(2016, 6, 6),
				EndDate = new DateTime(2016, 6, 7),
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			var result = target.GetCreatorInfoForFullDayAbsence(command);
			result.Absence.Should().Be.EqualTo(_absenceRepository.Single());
			result.AbsenceTimePeriod.Should().Be.EqualTo(new DateTimePeriod(2016, 6, 6, 23, 2016, 6, 7, 11));
		}

		[Test]
		public void ShouldConvertPersonForFullDay()
		{
			var target = new AbsenceCommandConverter(_currentScenario,_personRepository, _absenceRepository, _scheduleStorage, null);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			var result = target.GetCreatorInfoForFullDayAbsence(command);
			result.Person.Should().Be.EqualTo(_personRepository.Single());
		}			
		
		[Test]
		public void ShouldConvertTrackedCommandInfoForFullDay()
		{
			var target = new AbsenceCommandConverter(_currentScenario,_personRepository, _absenceRepository, _scheduleStorage, null);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			var result = target.GetCreatorInfoForFullDayAbsence(command);
			result.TrackedCommandInfo.OperatedPersonId.Should().Be.EqualTo(operatedPersonId);
			result.TrackedCommandInfo.TrackId.Should().Be.EqualTo(trackId);
		}	
	}
}
