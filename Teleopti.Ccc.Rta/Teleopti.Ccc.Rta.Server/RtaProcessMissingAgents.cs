using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
	public class RtaProcessMissingAgents
	{
		private readonly string _loggedOutCode;
		private readonly ProcessWhenMissing _processAgentState;
		private HashSet<RtaAgentState> _lastBatch = new HashSet<RtaAgentState>();
		private HashSet<RtaAgentState> _currentBatch = new HashSet<RtaAgentState>();
		private DateTime _currentBatchId;
		private bool _firstInBatch = true;
		private readonly ISpecification<RtaAgentState> _isLast = new isLastInBatch();

		public delegate void ProcessWhenMissing(RtaAgentState agentState);

		public RtaProcessMissingAgents(string loggedOutCode, ProcessWhenMissing processAgentState)
		{
			_loggedOutCode = loggedOutCode;
			_processAgentState = processAgentState;
		}

		public void Check(RtaAgentState rtaAgentState)
		{
			if (!rtaAgentState.IsSnapshot || !rtaAgentState.IsLoggedOn) return;

			if (_firstInBatch)
			{
				_currentBatchId = rtaAgentState.BatchId;
				_firstInBatch = false;
			}

			if (_isLast.IsSatisfiedBy(rtaAgentState))
			{

				var usersInLastBatch = from a in _lastBatch
				                       select a.UserCode;

				var usersInCurrentBatch = from a in _currentBatch
				                          select a.UserCode;

				var onlyInLastBatch = usersInLastBatch.Except(usersInCurrentBatch);

				var usersToCheck = from a in _lastBatch
				                   where onlyInLastBatch.Contains(a.UserCode)
				                   select new RtaAgentState()
					                          {
						                          AuthenticationKey = a.AuthenticationKey,
						                          UserCode = a.UserCode,
						                          StateCode = _loggedOutCode,
						                          StateDescription = a.StateDescription,
						                          IsLoggedOn = false,
						                          Timestamp = _currentBatchId,
						                          PlatformTypeId = a.PlatformTypeId,
						                          SourceId = a.SourceId,
						                          BatchId = _currentBatchId,
						                          IsSnapshot = a.IsSnapshot
					                          };

				foreach (var agentState in usersToCheck)
				{
					_processAgentState(agentState);
				}

				_lastBatch = new HashSet<RtaAgentState>(_currentBatch);
				_currentBatch.Clear();
				_firstInBatch = true;

			}
			else
			{
				_currentBatch.Add(rtaAgentState);
			}
		}

		private class isLastInBatch : Specification<RtaAgentState>
		{
			public override bool IsSatisfiedBy(RtaAgentState obj)
			{
				return String.IsNullOrEmpty(obj.UserCode);
			}
		}
	}
}