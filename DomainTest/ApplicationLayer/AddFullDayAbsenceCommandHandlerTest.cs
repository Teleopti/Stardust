using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
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
			var personRepository = new TestWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var currentDataSource = new FakeCurrentDatasource("datasource");
			var currentScenario = new FakeCurrentScenario();

			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = absenceRepository.Single().Id.Value,
				PersonId = personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
			};

			var target = new AddFullDayAbsenceCommandHandler(currentDataSource, currentScenario, personRepository,
			                                                 absenceRepository, personAbsenceRepository,
			                                                 MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(),
			                                                 new NewtonsoftJsonDeserializer());
			target.Handle(command);

			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.Datasource.Should().Be("datasource");
			@event.BusinessUnitId.Should().Be(currentScenario.Current().BusinessUnit.Id.Value);
			@event.AbsenceId.Should().Be(absenceRepository.Single().Id.Value);
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.ScenarioId.Should().Be(currentScenario.Current().Id.Value);
			@event.StartDateTime.Should().Be(command.StartDate);
			@event.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
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

			var target = new AddFullDayAbsenceCommandHandler(new FakeCurrentDatasource(), new FakeCurrentScenario(),
															 personRepository, absenceRepository, personAbsenceRepository,
															 MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(),
															 new NewtonsoftJsonDeserializer());
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(personRepository.Single());
			absenceLayer.Payload.Should().Be(absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartDate);
			absenceLayer.Period.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
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

			var target = new AddFullDayAbsenceCommandHandler(new FakeCurrentDatasource(), new FakeCurrentScenario(),
															 personRepository, absenceRepository, personAbsenceRepository,
															 MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(),
															 new NewtonsoftJsonDeserializer());
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(personRepository.Single());
			absenceLayer.Payload.Should().Be(absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartDate, agentsTimeZone));
			absenceLayer.Period.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndDate.AddHours(24).AddMinutes(-1), agentsTimeZone));
			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartDate, agentsTimeZone));
			@event.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndDate.AddHours(24).AddMinutes(-1), agentsTimeZone));
		}

		[Test]
		public void ShouldConsiderShiftEndOnDayBeforeStartDate()
		{
			var person = PersonFactory.CreatePersonWithId();
			var agentsTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			person.PermissionInformation.SetDefaultTimeZone(agentsTimeZone);
			var personRepository = new TestWriteSideRepository<IPerson> { person };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();

			var startDate = new DateTime(2013, 3, 25);
			var endDate = new DateTime(2013, 3, 26);
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = absenceRepository.Single().Id.Value,
				PersonId = personRepository.Single().Id.Value,
				StartDate = startDate,
				EndDate = endDate,
			};

			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			const string testShift = @"{HasUnderlyingShift:true,Projection:[{End:'2013-03-25T05:00:00Z'}]}";
			personScheduleDayReadModelRepository.Stub(x => x.ForPerson(new DateOnly(startDate.AddDays(-1)), person.Id.Value)).Return(new PersonScheduleDayReadModel
			{
				Shift = testShift
			});
			var deserializer = new NewtonsoftJsonDeserializer();

			var target = new AddFullDayAbsenceCommandHandler(new FakeCurrentDatasource(), new FakeCurrentScenario(),
			                                                 personRepository, absenceRepository, personAbsenceRepository,
			                                                 personScheduleDayReadModelRepository, deserializer);
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(personRepository.Single());
			absenceLayer.Payload.Should().Be(absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartDate.AddHours(5), agentsTimeZone));
		}

		[Test]
		public void ShouldConsiderShiftEndOnEndDate()
		{
			var person = PersonFactory.CreatePersonWithId();
			var agentsTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			person.PermissionInformation.SetDefaultTimeZone(agentsTimeZone);
			var personRepository = new TestWriteSideRepository<IPerson> { person };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();

			var startDate = new DateTime(2013, 3, 25);
			var endDate = new DateTime(2013, 3, 26);
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = absenceRepository.Single().Id.Value,
				PersonId = personRepository.Single().Id.Value,
				StartDate = startDate,
				EndDate = endDate,
			};

			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			const string testShift = @"{HasUnderlyingShift:true,Projection:[{End:'2013-03-27T05:00:00Z'}]}";
			personScheduleDayReadModelRepository.Stub(x => x.ForPerson(new DateOnly(endDate), person.Id.Value)).Return(new PersonScheduleDayReadModel
			{
				Shift = testShift
			});
			var deserializer = new NewtonsoftJsonDeserializer();

			var target = new AddFullDayAbsenceCommandHandler(new FakeCurrentDatasource(), new FakeCurrentScenario(),
			                                                 personRepository, absenceRepository, personAbsenceRepository,
			                                                 personScheduleDayReadModelRepository, deserializer);
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(personRepository.Single());
			absenceLayer.Payload.Should().Be(absenceRepository.Single());
			absenceLayer.Period.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndDate.AddDays(1).AddHours(5), agentsTimeZone));
		}

	}
}