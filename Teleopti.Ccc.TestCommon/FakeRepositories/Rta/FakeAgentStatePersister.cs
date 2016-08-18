using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAgentStatePersister : IAgentStatePersister
	{
		private readonly ConcurrentDictionary<Guid, AgentState> _data = new ConcurrentDictionary<Guid, AgentState>();

		public void Has(AgentState model)
		{
			Persist(model);
		}

		public void Persist(AgentState model)
		{
			_data.AddOrUpdate(model.PersonId, model, (k, v) => model);
		}

		void IAgentStatePersister.Delete(Guid personId)
		{
			AgentState state;
			_data.TryRemove(personId, out state);
		}

		public IEnumerable<AgentState> GetNotInSnapshot(DateTime batchId, string sourceId)
		{
			return (from s in _data.Values
				where s.SourceId == sourceId &&
					  (s.BatchId < batchId ||
					   s.BatchId == null)
				select s)
				.ToArray();
		}

		public AgentState Get(Guid personId)
		{
			return _data.Values.SingleOrDefault(x => x.PersonId == personId);
		}

		public IEnumerable<AgentState> Get(IEnumerable<Guid> personIds)
		{
			return _data.Values.Where(x => personIds.Contains(x.PersonId));
		}

		public IEnumerable<AgentState> GetAll()
		{
			return _data.Values.ToArray();
		}

	}
}