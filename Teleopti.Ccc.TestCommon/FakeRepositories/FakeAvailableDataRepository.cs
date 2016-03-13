using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAvailableDataRepository : IAvailableDataRepository
	{
		private List<IAvailableData> _roles = new List<IAvailableData>();

		public void Add(IAvailableData entity)
		{
			_roles.Add(entity);
		}

		public void Remove(IAvailableData entity)
		{
			throw new NotImplementedException();
		}

		public IAvailableData Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IAvailableData> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IAvailableData Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IAvailableData> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IList<IAvailableData> LoadAllAvailableData()
		{
			throw new NotImplementedException();
		}

		public IAvailableData LoadAllCollectionsInAvailableData(IAvailableData availableData)
		{
			throw new NotImplementedException();
		}
	}
}