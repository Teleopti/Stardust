using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAgentDayScheduleTagRepository : IAgentDayScheduleTagRepository
	{
		private readonly IFakeStorage _storage;

		public FakeAgentDayScheduleTagRepository(IFakeStorage storage)
		{
			_storage = storage;
		}

		public void Add(IAgentDayScheduleTag root)
		{
			_storage.Add(root);
		}

		public void Remove(IAgentDayScheduleTag root)
		{
			_storage.Remove(root);
		}

		public IAgentDayScheduleTag Get(Guid id)
		{
			return _storage.Get<IAgentDayScheduleTag>(id);
		}

		public IEnumerable<IAgentDayScheduleTag> LoadAll()
		{
			return _storage.LoadAll<IAgentDayScheduleTag>().ToArray();
		}

		public IAgentDayScheduleTag Load(Guid id)
		{
			return Get(id);
		}

		IAgentDayScheduleTag ILoadAggregateByTypedId<IAgentDayScheduleTag, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public IAgentDayScheduleTag LoadAggregate(Guid id)
		{
			return Get(id);
		}

		public IList<IAgentDayScheduleTag> Find(DateTimePeriod period, IScenario scenario)
		{
			//change when needed
			return new List<IAgentDayScheduleTag>();
		}

		public ICollection<IAgentDayScheduleTag> Find(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario)
		{
			return _storage.LoadAll<IAgentDayScheduleTag>().Where(a => dateOnlyPeriod.Contains(a.TagDate) && personCollection.Contains(a.Person) && scenario.Equals(a.Scenario)).ToArray();
		}
	}
}