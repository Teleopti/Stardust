﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class ActualAgentStateCacheTest
	{
		private actualAgentStateCacheForTest _target;
		private IDatabaseHandler _databaseHandler;
		private IActualAgentState _agentState;
		private readonly Guid _personId = Guid.NewGuid();

		[SetUp]
		public void Setup()
		{
			_databaseHandler = MockRepository.GenerateStrictMock<IDatabaseHandler>();
			_target = new actualAgentStateCacheForTest(_databaseHandler);
			_agentState = new ActualAgentState
				{
					PersonId = _personId
				};
		}

		[Test]
		public void AddAgentStateToCache_AddAgent()
		{
			_target.AddAgentStateToCache(_agentState);
			_target.batchedAgents.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void AddAgentStateToCache_AddNewerStateForAgent_ShouldTakeMostRecent()
		{
			_agentState.ReceivedTime = new DateTime(2000, 01, 01);
			var stateId = Guid.NewGuid();
			var newerState = new ActualAgentState
				{
					PersonId = _personId,
					ReceivedTime = new DateTime(2013, 09, 04),
					StateId = stateId
				};

			_target.AddAgentStateToCache(_agentState);
			_target.AddAgentStateToCache(newerState);
			_target.batchedAgents[_personId].StateId.Should().Be.EqualTo(stateId);
		}

		[Test]
		public void AddagentStateToCache_AddOlderStateForAgent_ShouldTakeMostRecent()
		{
			var stateId = Guid.NewGuid();
			_agentState.ReceivedTime = new DateTime(2103,09,04);
			_agentState.StateId = stateId;

			var olderState = new ActualAgentState
				{
					PersonId = _personId,
					ReceivedTime = new DateTime(2001,01,01)
				};
			_target.AddAgentStateToCache(_agentState);
			_target.AddAgentStateToCache(olderState);
			_target.batchedAgents[_personId].StateId.Should().Be.EqualTo(stateId);
		}

		[Test]
		public void FlushCacheToDataBase_NoBatchedAgents_ReturnNoAction()
		{
			_target.FlushCacheToDatabase();
			_databaseHandler.AssertWasNotCalled(d => d.AddOrUpdate(null), a => a.IgnoreArguments());
		}

		[Test]
		public void FlushCacheToDataBase_ArgumentsToDataHandler()
		{
			var stateId = Guid.NewGuid();
			_agentState.StateId = stateId;
			_target.AddAgentStateToCache(_agentState);

			_databaseHandler.Expect(d => d.AddOrUpdate(null)).IgnoreArguments();

			_target.FlushCacheToDatabase();
			var args = _databaseHandler.GetArgumentsForCallsMadeOn(d => d.AddOrUpdate(null));
			((List<IActualAgentState>) args[0][0])[0].StateId.Should().Be.EqualTo(stateId);
		}

		[Test]
		public void FlushCacheToDataBase_RemoveSavedStates()
		{
			_target.AddAgentStateToCache(_agentState);

			_databaseHandler.Expect(d => d.AddOrUpdate(null)).IgnoreArguments();
			_target.FlushCacheToDatabase();

			_target.batchedAgents.Count.Should().Be.EqualTo(0);
		}

		private class actualAgentStateCacheForTest :ActualAgentStateCache
		{
			public actualAgentStateCacheForTest(IDatabaseHandler databaseHandler) : base(databaseHandler)
			{
			}

			public ConcurrentDictionary<Guid, IActualAgentState> batchedAgents
			{
				get { return BatchedAgents; }
			}
				
			
		}
	}
}
