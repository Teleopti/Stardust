using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleTagRepository : IScheduleTagRepository
	{
		private readonly IList<IScheduleTag> _scheduleTags = new List<IScheduleTag>();

		public void Add(IScheduleTag root)
		{
			_scheduleTags.Add(root);
		}

		public void Remove(IScheduleTag root)
		{
			throw new NotImplementedException();
		}

		public IScheduleTag Get(Guid id)
		{
			return _scheduleTags.FirstOrDefault(s => s.Id == id);
		}

		public IList<IScheduleTag> LoadAll()
		{
			return _scheduleTags.ToArray();
		}

		public IScheduleTag Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IScheduleTag> FindAllScheduleTags()
		{
			throw new NotImplementedException();
		}
	}
}