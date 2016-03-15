using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeRtaMapRepository : IRtaMapRepository
	{
		private readonly IList<IRtaMap> _data = new List<IRtaMap>();

		public void Add(IRtaMap entity)
		{
			_data.Add(entity);
		}

		public void Remove(IRtaMap entity)
		{
			throw new NotImplementedException();
		}

		public IRtaMap Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IRtaMap> LoadAll()
		{
			return _data;
		}

		public IRtaMap Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IRtaMap> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork
		{
			get { throw new NotImplementedException(); }
		}

		public IList<IRtaMap> LoadAllCompleteGraph()
		{
			return _data;
		}

		public void Clear()
		{
			_data.Clear();
		}
	}
}