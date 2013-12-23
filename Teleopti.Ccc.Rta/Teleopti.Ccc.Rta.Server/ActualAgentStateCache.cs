using System;
using System.Collections.Concurrent;
using System.Linq;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
    public class ActualAgentStateCache : IActualAgentStateCache
	{
		protected readonly ConcurrentDictionary<Guid, IActualAgentState> BatchedAgents = new ConcurrentDictionary<Guid, IActualAgentState>();
		private readonly object _lockObject = new object();
		private readonly ILog Log = LogManager.GetLogger(typeof(ActualAgentStateCache));
		private readonly IDatabaseWriter _databaseWriter;

		public ActualAgentStateCache(IDatabaseWriter databaseWriter)
		{
			_databaseWriter = databaseWriter;
		}

		public void AddAgentStateToCache(IActualAgentState state)
		{
			Log.InfoFormat("Added AgentState to cache: {0}", state);
			BatchedAgents.AddOrUpdate(state.PersonId, state,
									  (guid, oldState) => state.ReceivedTime >= oldState.ReceivedTime
															  ? state
															  : oldState);
		}

        public bool TryGetLatestState(Guid personId, out IActualAgentState actualAgentState)
        {
            return BatchedAgents.TryGetValue(personId, out actualAgentState);
        }

        public void FlushCacheToDatabase()
		{
			if (!BatchedAgents.Any())
				return;
			lock (_lockObject)
			{
				saveToDatabase();
				resetBatchedAgents();
			}
		}

		private void saveToDatabase()
		{
			Log.InfoFormat("Saving {0} AgentStates to database", BatchedAgents.Count);
			_databaseWriter.AddOrUpdate(BatchedAgents.Values.ToList());
		}

		private void resetBatchedAgents()
		{
			foreach (var agentState in BatchedAgents.Values)
			{
				IActualAgentState outAgentState;
				if (!BatchedAgents.TryGetValue(agentState.PersonId, out outAgentState) ||
					agentState.ReceivedTime < outAgentState.ReceivedTime)
					continue;

				Log.DebugFormat("Removing AgentState from cache {0}", outAgentState);
				BatchedAgents.TryRemove(agentState.PersonId, out outAgentState);
			}
		}
	}
}
