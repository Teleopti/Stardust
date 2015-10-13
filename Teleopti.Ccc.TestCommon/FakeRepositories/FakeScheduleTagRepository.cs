using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleTagRepository : IScheduleTagRepository
	{
		private IList<IScheduleTag> _scheduleTags = new List<IScheduleTag>();

		public void Add(IScheduleTag root)
		{
			throw new NotImplementedException();
		}

		public void Remove(IScheduleTag root)
		{
			throw new NotImplementedException();
		}

		public IScheduleTag Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IScheduleTag> LoadAll()
		{
			return _scheduleTags.ToArray();
		}

		public IScheduleTag Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IScheduleTag> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; }
		public IList<IScheduleTag> FindAllScheduleTags()
		{
			throw new NotImplementedException();
		}
	}
}