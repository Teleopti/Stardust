using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAlarmTypeRepository : IAlarmTypeRepository
	{
		private readonly IList<IAlarmType> _data = new List<IAlarmType>();

		public void Add(IAlarmType entity)
		{
			_data.Add(entity);
		}

		public void Remove(IAlarmType entity)
		{
			throw new NotImplementedException();
		}

		public IAlarmType Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IAlarmType> LoadAll()
		{
			return _data;
		}

		public IAlarmType Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IAlarmType> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}