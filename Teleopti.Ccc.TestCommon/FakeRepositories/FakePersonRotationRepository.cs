using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonRotationRepository : IPersonRotationRepository
	{
		public void Add(IPersonRotation root)
		{
			throw new NotImplementedException();
		}

		public void Remove(IPersonRotation root)
		{
			throw new NotImplementedException();
		}

		public IPersonRotation Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonRotation> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IPersonRotation Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IPersonRotation> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IList<IPersonRotation> Find(IPerson person)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonRotation> Find(IList<IPerson> persons)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPersonRotation> LoadPersonRotationsWithHierarchyData(IEnumerable<IPerson> persons, DateOnly startDate)
		{
			//impl when needed
			return Enumerable.Empty<IPersonRotation>();
		}
	}
}