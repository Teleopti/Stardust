using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAvailabilityRepository : IPersonAvailabilityRepository
	{
		public IList<IPersonAvailability> _storage = new List<IPersonAvailability>(); 
		
		public void Add(IPersonAvailability root)
		{
			_storage.Add(root);
		}
		
		public void Has(IPersonAvailability personAvailability)
		{
			Add(personAvailability.WithId());
		}

		public void Remove(IPersonAvailability root)
		{
			throw new NotImplementedException();
		}

		public IPersonAvailability Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPersonAvailability> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IPersonAvailability Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPersonAvailability> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPersonAvailability> LoadPersonAvailabilityWithHierarchyData(IEnumerable<IPerson> persons, DateOnly startDate)
		{
			//startdate?
			return _storage.Where(x => persons.Contains(x.Person));
		}
	}
}