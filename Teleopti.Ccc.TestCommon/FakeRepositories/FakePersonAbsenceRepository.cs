using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
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
			throw new NotImplementedException();
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

		public void AddRange(IEnumerable<IPersonAbsence> entityCollection)
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

		public ICollection<IPersonAbsence> Find(IEnumerable<Guid> personAbsenceIds)
		{
			var idList = personAbsenceIds.ToList();
			return _personAbsences.Where(x => idList.Contains(x.Id.GetValueOrDefault())).ToArray();
		}

		public IList<IPersonAbsence> Find (IAbsenceRequest absenceRequest)
		{
			return _personAbsences.Where(personAbsence => personAbsence.AbsenceRequest == absenceRequest).ToList();
		}

		public ICollection<DateTimePeriod> AffectedPeriods(IPerson person, IScenario scenario, DateTimePeriod period, IAbsence absence)
		{
			throw new NotImplementedException();
		}

	}
}
