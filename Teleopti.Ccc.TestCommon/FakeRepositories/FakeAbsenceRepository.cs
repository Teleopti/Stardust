using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAbsenceRepository : IAbsenceRepository
	{
		private readonly List<IAbsence> _absences = new List<IAbsence>();

		public void Add(IAbsence root)
		{
			_absences.Add(root);
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
			_absences.AddRange(entityCollection);
		}

		public IUnitOfWork UnitOfWork { get; private set; }
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