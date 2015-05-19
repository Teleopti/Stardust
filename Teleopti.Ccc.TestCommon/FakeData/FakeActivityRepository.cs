using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeActivityRepository : IActivityRepository
	{
		public void Add(IActivity entity)
		{
			throw new NotImplementedException();
		}

		public void Remove(IActivity entity)
		{
			throw new NotImplementedException();
		}

		public IActivity Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IActivity> LoadAll()
		{
			return new List<IActivity> { ActivityFactory.CreateActivity("phone") };
		}

		public IActivity Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IActivity> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IList<IActivity> LoadAllSortByName()
		{
			throw new NotImplementedException();
		}
	}
}