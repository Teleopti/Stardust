using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeStateGroupActivityAlarmRepository : IStateGroupActivityAlarmRepository
	{
		private readonly IList<IStateGroupActivityAlarm> _data = new List<IStateGroupActivityAlarm>();

		public void Add(IStateGroupActivityAlarm entity)
		{
			_data.Add(entity);
		}

		public void Remove(IStateGroupActivityAlarm entity)
		{
			throw new NotImplementedException();
		}

		public IStateGroupActivityAlarm Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IStateGroupActivityAlarm> LoadAll()
		{
			return _data;
		}

		public IStateGroupActivityAlarm Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IStateGroupActivityAlarm> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork
		{
			get { throw new NotImplementedException(); }
		}

		public IList<IStateGroupActivityAlarm> LoadAllCompleteGraph()
		{
			throw new NotImplementedException();
		}
	}
}