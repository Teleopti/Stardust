using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeShiftCategoryRepository : IShiftCategoryRepository
	{
		private readonly IList<IShiftCategory> storage = new List<IShiftCategory>();

		public void Add(IShiftCategory root)
		{
			storage.Add(root);
		}

		public void Remove(IShiftCategory root)
		{
			throw new NotImplementedException();
		}

		public IShiftCategory Get(Guid id)
		{
			return storage.FirstOrDefault(s => id == s.Id);
		}

		public IList<IShiftCategory> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IShiftCategory Load(Guid id)
		{
			return Get(id);
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IShiftCategory> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IList<IShiftCategory> FindAll()
		{
			return storage;
		}
	}
}