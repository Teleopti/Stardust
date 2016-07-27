using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeRtaStateGroupRepository : IRtaStateGroupRepository
	{
		private readonly List<IRtaStateGroup> _data = new List<IRtaStateGroup>();

		public void Has(IRtaStateGroup stateGroup)
		{
			Add(stateGroup);
		}

		public void Add(IRtaStateGroup entity)
		{
			_data.Add(entity);
		}

		public void Remove(IRtaStateGroup entity)
		{
			throw new NotImplementedException();
		}

		public IRtaStateGroup Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IRtaStateGroup> LoadAll()
		{
			return _data;
		}

		public IRtaStateGroup Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IRtaStateGroup> entityCollection)
		{
			_data.AddRange(entityCollection);
		}

		public IUnitOfWork UnitOfWork
		{
			get { throw new NotImplementedException(); }
		}

		public IList<IRtaStateGroup> LoadAllCompleteGraph()
		{
			return _data;
		}

		public IEnumerable<IRtaStateGroup> LoadAllExclusive()
		{
			return _data;
		}

		public void Clear()
		{
			_data.Clear();
		}

	}
}