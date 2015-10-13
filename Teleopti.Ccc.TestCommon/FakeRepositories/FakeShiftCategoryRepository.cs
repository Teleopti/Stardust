using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeShiftCategoryRepository : IShiftCategoryRepository
	{
		private IList<IShiftCategory> _absences = new List<IShiftCategory>();

		public void Add(IShiftCategory root)
		{
			throw new NotImplementedException();
		}

		public void Remove(IShiftCategory root)
		{
			throw new NotImplementedException();
		}

		public IShiftCategory Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IShiftCategory> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IShiftCategory Load(Guid id)
		{
			throw new NotImplementedException();
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
			return _absences;
		}
	}
}