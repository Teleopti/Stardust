﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Commands
{
	[TestFixture]
	public class AbsenceCommandConverterTest
	{
		private FakeWriteSideRepository<IPerson> _personRepository;
		private FakeWriteSideRepository<IAbsence> _absenceRepository;
		private FakeCurrentScenario _currentScenario;
		private FakeScheduleRepository _scheduleRepository;

		[SetUp]
		public void Setup()
		{
			_personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			_absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			_currentScenario = new FakeCurrentScenario();
			_scheduleRepository = new FakeScheduleRepository();
		}

		[Test]
		public void ShouldConvertAbsence()
		{
			var target = new AbsenceCommandConverter(_currentScenario,_personRepository, _absenceRepository, _scheduleRepository);

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

			var result = target.GetCreatorInfo(command);
			result.Absence.Should().Be.EqualTo(_absenceRepository.Single());
		}		
		
		[Test]
		public void ShouldConvertPerson()
		{
			var target = new AbsenceCommandConverter(_currentScenario,_personRepository, _absenceRepository, _scheduleRepository);

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

			var result = target.GetCreatorInfo(command);
			result.Person.Should().Be.EqualTo(_personRepository.Single());
		}			
		
		[Test]
		public void ShouldConvertTrackedCommandInfo()
		{
			var target = new AbsenceCommandConverter(_currentScenario,_personRepository, _absenceRepository, _scheduleRepository);

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

			var result = target.GetCreatorInfo(command);
			result.TrackedCommandInfo.OperatedPersonId.Should().Be.EqualTo(operatedPersonId);
			result.TrackedCommandInfo.TrackId.Should().Be.EqualTo(trackId);
		}		
	}
}
