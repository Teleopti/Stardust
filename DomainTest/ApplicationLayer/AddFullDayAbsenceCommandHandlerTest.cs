using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class AddFullDayAbsenceCommandHandlerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldRaiseFullDayAbsenceAddedEvent()
		{
			var personRepository = new WriteSideRepository<IPerson> {TestEntityFactory.MakeWithId<Person>()};
			var absenceRepository = new WriteSideRepository<IAbsence> {TestEntityFactory.MakeWithId<Absence>()};
			var personAbsenceRepository = new WriteSideRepository<IPersonAbsence>();

			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25, 0, 0, 0, DateTimeKind.Utc),
					EndDate = new DateTime(2013, 3, 25, 0, 0, 0, DateTimeKind.Utc),
				};

			var target = new AddFullDayAbsenceCommandHandler(new TestCurrentScenario(), personRepository, absenceRepository, personAbsenceRepository);
			target.Handle(command);

			personAbsenceRepository.Single().PopAllEvents().Single().Should().Be.OfType<FullDayAbsenceAddedEvent>();
		}

		[Test]
		public void ShouldSetupEntityState()
		{
			var personRepository = new WriteSideRepository<IPerson> { TestEntityFactory.MakeWithId<Person>() };
			var absenceRepository = new WriteSideRepository<IAbsence> { TestEntityFactory.MakeWithId<Absence>() };
			var personAbsenceRepository = new WriteSideRepository<IPersonAbsence>();

			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25, 0, 0, 0, DateTimeKind.Utc),
					EndDate = new DateTime(2013, 3, 25, 0, 0, 0, DateTimeKind.Utc),
				};

			var target = new AddFullDayAbsenceCommandHandler(new TestCurrentScenario(), personRepository, absenceRepository, personAbsenceRepository);
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(personRepository.Single());
			absenceLayer.Payload.Should().Be(absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartDate);
			absenceLayer.Period.EndDateTime.Should().Be(command.StartDate.AddHours(24));
		}

	}

	public static class TestEntityFactory
	{
		public static T MakeWithId<T>()
		{
			var entity = Activator.CreateInstance<T>() as Entity;
			entity.SetId(Guid.NewGuid());
			return (T) (object) entity;
		}
	}

	public class TestCurrentScenario : ICurrentScenario
	{
		private readonly IScenario _scenario = new Scenario(" ");

		public IScenario Current()
		{
			return _scenario;
		}
	}

	public class WriteSideRepository<T> : IEnumerable<T>, IWriteSideRepository<T> where T : IAggregateRoot
	{
		private readonly IList<T> _entities = new List<T>();

		public void Add(T entity)
		{
			_entities.Add(entity);
		}

		public void Remove(T entity)
		{
			_entities.Remove(entity);
		}

		public T Get(Guid id)
		{
			return _entities.Single(e => e.Id.Equals(id));
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _entities.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _entities.GetEnumerator();
		}
	}
}