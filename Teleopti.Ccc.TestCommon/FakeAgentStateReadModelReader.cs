using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeAgentStateReadModelReader : IAgentStateReadModelReader
	{
		private readonly IList<AgentStateReadModel> _data = new List<AgentStateReadModel>();

		public FakeAgentStateReadModelReader()
		{
		}

		public FakeAgentStateReadModelReader(IEnumerable<AgentStateReadModel> data)
		{
			data.ForEach(_data.Add);
		}

		public void Has(AgentStateReadModel model)
		{
			var previousState = (from s in _data where s.PersonId == model.PersonId select s).FirstOrDefault();
			if (previousState != null)
				_data.Remove(previousState);
			_data.Add(model);
		}

		public IList<AgentStateReadModel> Load(IEnumerable<IPerson> persons)
		{
			throw new NotImplementedException();
		}

		public IList<AgentStateReadModel> Load(IEnumerable<Guid> personGuids)
		{
			throw new NotImplementedException();
		}

		public IList<AgentStateReadModel> LoadForTeam(Guid teamId)
		{
			return _data.Where(x => x.TeamId == teamId).ToArray();
		}

		public IEnumerable<AgentStateReadModel> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId)
		{
			return (from s in _data
				where s.OriginalDataSourceId == dataSourceId &&
				      (s.BatchId < batchId ||
				       s.BatchId == null)
				select s).ToArray();
		}

		public AgentStateReadModel GetCurrentActualAgentState(Guid personId)
		{
			return _data.SingleOrDefault(x => x.PersonId == personId);
		}

		public IEnumerable<AgentStateReadModel> GetActualAgentStates()
		{
			return _data.ToArray();
		}

	}
}