using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAbsenceRepository : IAbsenceRepository
	{
		private IList<IAbsence> _absences = new List<IAbsence>();

		public void Add(IAbsence root)
		{
			throw new NotImplementedException();
		}

		public void Remove(IAbsence root)
		{
			throw new NotImplementedException();
		}

		public IAbsence Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IAbsence> LoadAll()
		{
			return _absences.ToArray();
		}

		public IAbsence Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IAbsence> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; }
		public IEnumerable<IAbsence> LoadAllSortByName()
		{
			throw new NotImplementedException();
		}

		public IList<IAbsence> LoadRequestableAbsence()
		{
			throw new NotImplementedException();
		}

		public IList<IAbsence> FindAbsenceTrackerUsedByPersonAccount()
		{
			throw new NotImplementedException();
		}
	}
}