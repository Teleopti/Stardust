using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAgentStatePersister : IAgentStatePersister
	{
		private readonly Dictionary<Guid, AgentState> _data = new Dictionary<Guid, AgentState>();

		public void Has(AgentState model)
		{
			Persist(model);
		}

		public void Persist(AgentState model)
		{
			var existing = _data.Values.SingleOrDefault(x => x.PersonId == model.PersonId);
			if (existing != null)
				_data.Remove(existing.PersonId);
			_data.Add(model.PersonId, model);
		}

		void IAgentStatePersister.Delete(Guid personId)
		{
			_data.Remove(personId);
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

		public IEnumerable<AgentState> GetAll()
		{
			return _data.Values.ToArray();
		}

	}
}