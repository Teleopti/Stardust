using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAgentDayScheduleTagRepository : IAgentDayScheduleTagRepository
	{
		private readonly IList<IAgentDayScheduleTag> agentDayScheduleTags = new List<IAgentDayScheduleTag>();

		public void Add(IAgentDayScheduleTag root)
		{
			agentDayScheduleTags.Add(root);
		}

		public void Remove(IAgentDayScheduleTag root)
		{
			agentDayScheduleTags.Remove(root);
		}

		public IAgentDayScheduleTag Get(Guid id)
		{
			return agentDayScheduleTags.FirstOrDefault(x => x.Id == id);
		}

		public IList<IAgentDayScheduleTag> LoadAll()
		{
			return agentDayScheduleTags;
		}

		public IAgentDayScheduleTag Load(Guid id)
		{
			throw new NotImplementedException();
		}

		IAgentDayScheduleTag ILoadAggregateByTypedId<IAgentDayScheduleTag, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public IAgentDayScheduleTag LoadAggregate(Guid id)
		{
			return agentDayScheduleTags.First(x => x.Id == id);
		}

		public IList<IAgentDayScheduleTag> Find(DateTimePeriod period, IScenario scenario)
		{
			//change when needed
			return new List<IAgentDayScheduleTag>();
		}

		public ICollection<IAgentDayScheduleTag> Find(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario)
		{
			//change when needed
			return new List<IAgentDayScheduleTag>();
		}
	}
}