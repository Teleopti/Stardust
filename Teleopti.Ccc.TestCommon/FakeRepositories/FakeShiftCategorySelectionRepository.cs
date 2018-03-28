using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeShiftCategorySelectionRepository : IRepository<IShiftCategorySelection>
	{
		private readonly FakeStorage _storage;

		public FakeShiftCategorySelectionRepository(FakeStorage storage)
		{
			_storage = storage;
		}

		public void Add(IShiftCategorySelection root)
		{
			_storage.Add(root);
		}

		public void Remove(IShiftCategorySelection root)
		{
			_storage.Remove(root);
		}

		public IShiftCategorySelection Get(Guid id)
		{
			return _storage.Get<IShiftCategorySelection>(id);
		}

		public IShiftCategorySelection Load(Guid id)
		{
			return Get(id);
		}

		public IEnumerable<IShiftCategorySelection> LoadAll()
		{
			return _storage.LoadAll<IShiftCategorySelection>();
		}
	}
}