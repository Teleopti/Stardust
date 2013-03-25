using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class AddFullDayAbsenceCommandHandlerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldRaiseFullDayAbsenceAddedEvent()
		{
			var personRepository = new TestWriteSideRepository<IPerson> {PersonFactory.CreatePersonWithId()};
			var absenceRepository = new TestWriteSideRepository<IAbsence> {AbsenceFactory.CreateAbsenceWithId()};
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();

			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25),
				};

			var target = new AddFullDayAbsenceCommandHandler(new TestCurrentScenario(), personRepository, absenceRepository, personAbsenceRepository);
			target.Handle(command);

			personAbsenceRepository.Single().PopAllEvents().Single().Should().Be.OfType<FullDayAbsenceAddedEvent>();
		}

		[Test]
		public void ShouldSetupEntityState()
		{
			var personRepository = new TestWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();

			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 26),
				};

			var target = new AddFullDayAbsenceCommandHandler(new TestCurrentScenario(), personRepository, absenceRepository, personAbsenceRepository);
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(personRepository.Single());
			absenceLayer.Payload.Should().Be(absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartDate);
			absenceLayer.Period.EndDateTime.Should().Be(command.EndDate.AddHours(24));
		}

		[Test]
		public void ShouldConvertFromAgentsTimeZone()
		{
			var person = PersonFactory.CreatePersonWithId();
			var agentsTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			person.PermissionInformation.SetDefaultTimeZone(agentsTimeZone);
			var personRepository = new TestWriteSideRepository<IPerson> { person };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();

			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = absenceRepository.Single().Id.Value,
				PersonId = personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 26),
			};

			var target = new AddFullDayAbsenceCommandHandler(new TestCurrentScenario(), personRepository, absenceRepository, personAbsenceRepository);
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(personRepository.Single());
			absenceLayer.Payload.Should().Be(absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartDate, agentsTimeZone));
			absenceLayer.Period.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndDate.AddHours(24), agentsTimeZone));
		}
	}
}