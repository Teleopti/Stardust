﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IActualAgentStateCache
	{
		void FlushCacheToDatabase();
		void AddAgentStateToCache(IActualAgentState state);
	}

	public class ActualAgentStateCache : IActualAgentStateCache
	{
		protected readonly ConcurrentDictionary<Guid, IActualAgentState> BatchedAgents = new ConcurrentDictionary<Guid, IActualAgentState>();
		private readonly object _lockObject = new object();
		private readonly ILog Log = LogManager.GetLogger(typeof(ActualAgentStateCache));
		private readonly IDatabaseHandler _databaseHandler;
		
		public ActualAgentStateCache(IDatabaseHandler databaseHandler)
		{
			_databaseHandler = databaseHandler;
		}

		public void AddAgentStateToCache(IActualAgentState state)
		{
			Log.InfoFormat("Added state to cache: {0}", state);
			BatchedAgents.AddOrUpdate(state.PersonId, state,
			                          (guid, oldState) => state.ReceivedTime > oldState.ReceivedTime
				                                              ? state
				                                              : oldState);
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
			Log.InfoFormat("Saving {0} states to database", BatchedAgents.Count);
			_databaseHandler.AddOrUpdate(BatchedAgents.Values.ToList());
		}

		private void resetBatchedAgents()
		{
			foreach (var agentState in BatchedAgents.Values)
			{
				IActualAgentState outAgentState;
				if (!BatchedAgents.TryGetValue(agentState.PersonId, out outAgentState) ||
				    agentState.ReceivedTime < outAgentState.ReceivedTime) 
					continue;

				Log.InfoFormat("Trying to remove state from cache {0}", outAgentState);
				BatchedAgents.TryRemove(agentState.PersonId, out outAgentState);
			}
		}
	}
}
