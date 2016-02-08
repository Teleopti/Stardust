using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
			throw new NotImplementedException();
		}

		public IAgentDayScheduleTag Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IAgentDayScheduleTag> LoadAll()
		{
			return agentDayScheduleTags;
		}

		public IAgentDayScheduleTag Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IAgentDayScheduleTag> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		IAgentDayScheduleTag ILoadAggregateByTypedId<IAgentDayScheduleTag, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public IAgentDayScheduleTag LoadAggregate(Guid id)
		{
			throw new NotImplementedException();
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