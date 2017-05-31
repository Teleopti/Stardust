using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAvailableDataRepository : IAvailableDataRepository
	{
		private readonly List<IAvailableData> _roles = new List<IAvailableData>();

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
			return _roles;
		}

		public IAvailableData Load(Guid id)
		{
			throw new NotImplementedException();
		}

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