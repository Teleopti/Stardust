using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonRotationRepository : IPersonRotationRepository
	{
		public IList<IPersonRotation> _storage = new List<IPersonRotation>(); 

		public void Add(IPersonRotation root)
		{
			_storage.Add(root);
		}
		
		public void Has(IPersonRotation personRotation)
		{
			Add(personRotation.WithId());
		}

		public void Remove(IPersonRotation root)
		{
			_storage.Remove(root);
		}

		public IPersonRotation Get(Guid id)
		{
			return _storage.FirstOrDefault(r => r.Id == id);
		}

		public IEnumerable<IPersonRotation> LoadAll()
		{
			return _storage;
		}

		public IPersonRotation Load(Guid id)
		{
			return _storage.FirstOrDefault(r => r.Id == id);
		}

		public IList<IPersonRotation> Find(IPerson person)
		{
			return _storage.Where(r => r.Person == person).ToList();
		}

		public IList<IPersonRotation> Find(IList<IPerson> persons)
		{
			return _storage.Where(r => persons.Contains(r.Person)).ToList();
		}

		public IEnumerable<IPersonRotation> LoadPersonRotationsWithHierarchyData(IEnumerable<IPerson> persons, DateOnly startDate)
		{
			return _storage.Where(r => persons.Contains(r.Person));
		}
	}
}