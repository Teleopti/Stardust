using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAbsenceRepository : IPersonAbsenceRepository
	{

		private List<IPersonAbsence> _personAbsences = new List<IPersonAbsence>();

		public void Add(IPersonAbsence entity)
		{
			throw new NotImplementedException();
		}

		public void Remove(IPersonAbsence entity)
		{
			throw new NotImplementedException();
		}

		public IPersonAbsence Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonAbsence> LoadAll()
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public ICollection<IPersonAbsence> Find(IEnumerable<IPerson> persons, DateTimePeriod period)
		{
			return _personAbsences;
		}

		public ICollection<IPersonAbsence> Find(IEnumerable<IPerson> persons, DateTimePeriod period, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPersonAbsence> Find(DateTimePeriod period, IScenario scenario)
		{
			return _personAbsences.Where(x => x.BelongsToScenario(scenario) && x.Period.Intersect(period)).ToArray();
		}

		public ICollection<DateTimePeriod> AffectedPeriods(IPerson person, IScenario scenario, DateTimePeriod period, IAbsence absence)
		{
			throw new NotImplementedException();
		}
	}
}
