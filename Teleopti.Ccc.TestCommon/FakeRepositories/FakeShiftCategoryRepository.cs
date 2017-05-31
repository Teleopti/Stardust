using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

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
			return storage;
		}

		public IShiftCategory Load(Guid id)
		{
			return Get(id);
		}

		public IList<IShiftCategory> FindAll()
		{
			return storage;
		}
	}
}