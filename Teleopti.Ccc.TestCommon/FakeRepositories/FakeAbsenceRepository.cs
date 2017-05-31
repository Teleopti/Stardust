using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAbsenceRepository : IAbsenceRepository
	{
		private readonly List<IAbsence> _absences = new List<IAbsence>();

		public void Add(IAbsence root)
		{
			_absences.Add(root);
		}

		public void Has(IAbsence absence)
		{
			Add(absence);
		}

		public void Remove(IAbsence root)
		{
			throw new NotImplementedException();
		}

		public IAbsence Get(Guid id)
		{
			return _absences.FirstOrDefault(a => id == a.Id);
		}

		public IList<IAbsence> LoadAll()
		{
			return _absences.ToArray();
		}

		public IAbsence Load(Guid id)
		{
			return Get(id);
		}

		public IEnumerable<IAbsence> LoadAllSortByName()
		{
			return _absences;
		}

		public IList<IAbsence> LoadRequestableAbsence()
		{
			return _absences.Where(a => a.Requestable).ToArray();
		}

		public IList<IAbsence> FindAbsenceTrackerUsedByPersonAccount()
		{
			throw new NotImplementedException();
		}
	}
}