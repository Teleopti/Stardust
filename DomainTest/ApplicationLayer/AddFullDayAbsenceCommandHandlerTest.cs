using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
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
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var personRepository = new WriteSideRepository<IPerson> {person};

			var absence = new Absence();
			absence.SetId(Guid.NewGuid());
			var absenceRepository = new WriteSideRepository<IAbsence> {absence};

			var personAbsenceRepository = new WriteSideRepository<IPersonAbsence>();

			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = absence.Id.Value,
					PersonId = person.Id.Value,
					StartDate = DateTime.Today.Date,
					EndDate = DateTime.Today.Date
				};

			var target = new AddFullDayAbsenceCommandHandler(new TestCurrentScenario(), personRepository, absenceRepository, personAbsenceRepository);
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			personAbsence.PopAllEvents().Single().Should().Be.OfType<FullDayAbsenceAddedEvent>();
			personAbsence.PopAllEvents().Should().Have.Count.EqualTo(0);
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