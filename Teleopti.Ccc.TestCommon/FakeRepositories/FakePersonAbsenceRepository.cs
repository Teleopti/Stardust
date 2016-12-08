﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAbsenceRepository : IPersonAbsenceRepository
	{

		private List<IPersonAbsence> _personAbsences = new List<IPersonAbsence>();


		public void Has(IPersonAbsence personAbsence)
		{
			Add(personAbsence);
		}

		public void Add(IPersonAbsence entity)
		{
			_personAbsences.Add (entity);
		}

		public void Remove(IPersonAbsence entity)
		{
			_personAbsences.Remove(entity);
		}

		public IPersonAbsence Get(Guid id)
		{
			return _personAbsences.SingleOrDefault(personAbsence => personAbsence.Id == id);
		}

		public IList<IPersonAbsence> LoadAll()
		{
			return _personAbsences;
		}

		public IPersonAbsence Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork
		{
			get { throw new NotImplementedException(); }
		}

		IPersonAbsence ILoadAggregateByTypedId<IPersonAbsence, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public IPersonAbsence LoadAggregate(Guid id)
		{
			return _personAbsences.Single(pa => pa.Id == id);
		}

		public ICollection<IPersonAbsence> Find(IEnumerable<IPerson> persons, DateTimePeriod period)
		{
			return _personAbsences;
		}

		public ICollection<IPersonAbsence> Find(IEnumerable<IPerson> persons, DateTimePeriod period, IScenario scenario)
		{
			return _personAbsences
					.Where(personAbsence => personAbsence.BelongsToScenario(scenario) && 
							personAbsence.Period.Intersect(period) &&
							persons.Contains(personAbsence.Person)).ToArray(); 
		}

		public ICollection<IPersonAbsence> Find(DateTimePeriod period, IScenario scenario)
		{
			return _personAbsences.Where(x => x.BelongsToScenario(scenario) && x.Period.Intersect(period)).ToArray();
		}

		public ICollection<IPersonAbsence> Find(IEnumerable<Guid> personAbsenceIds, IScenario scenario)
		{
			var idList = personAbsenceIds.ToList();
			return _personAbsences.Where(x => idList.Contains(x.Id.GetValueOrDefault())).ToArray();
		}

		public ICollection<DateTimePeriod> AffectedPeriods(IPerson person, IScenario scenario, DateTimePeriod period, IAbsence absence)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonAbsence> Find(IPersonRequest personRequest, IScenario scenario)
		{
			return _personAbsences.Where(personAbsence => personAbsence.PersonRequest == personRequest).ToList();
		}

		public ICollection<IPersonAbsence> FindExact(IPerson person, DateTimePeriod period, IAbsence absence, IScenario scenario)
		{
			return _personAbsences.Where(personAbsence => personAbsence.Person == person && personAbsence.Period == period && personAbsence.Layer.Payload == absence && personAbsence.Scenario == scenario
			).ToList();
		}
	}
}
