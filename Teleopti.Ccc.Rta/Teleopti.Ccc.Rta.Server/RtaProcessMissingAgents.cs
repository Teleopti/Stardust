using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
	public class RtaProcessMissingAgents
	{
		private readonly string _loggedOutCode;
		private readonly ProcessWhenMissing _processAgentState;
		private readonly IRtaBatchHandler _batchHandler;
		private readonly HashSet<RtaAgentState> _currentBatch = new HashSet<RtaAgentState>();
		private DateTime _currentBatchId;
		private bool _firstInBatch = true;
		private readonly ISpecification<RtaAgentState> _isLast = new isLastInBatch();

		public delegate void ProcessWhenMissing(RtaAgentState agentState);

		public RtaProcessMissingAgents(string loggedOutCode, ProcessWhenMissing processAgentState, IRtaBatchHandler batchHandler)
		{
			_loggedOutCode = loggedOutCode;
			_processAgentState = processAgentState;
			_batchHandler = batchHandler;
		}

		public int Check(RtaAgentState rtaAgentState)
		{
			if (!rtaAgentState.IsSnapshot || !rtaAgentState.IsLoggedOn) 
				return 0;

			if (_firstInBatch)
			{
				_currentBatchId = rtaAgentState.BatchId;
				_firstInBatch = false;
			}

			if (_isLast.IsSatisfiedBy(rtaAgentState))
			{
				int dataSourceId;
				if (!int.TryParse(rtaAgentState.SourceId, out dataSourceId))
					return 0;

				var allUsersOnDataSource = _batchHandler.PeopleOnDataSource(dataSourceId);
				var usersInCurrentBatch1 = _currentBatch.Select(a => a.UserCode).ToList();
				var missingUsers =
					allUsersOnDataSource.Except(usersInCurrentBatch1)
											   .Select(a => new RtaAgentState
												   {
													   AuthenticationKey = rtaAgentState.AuthenticationKey,
													   BatchId = _currentBatchId,
													   IsLoggedOn = false,
													   IsSnapshot = rtaAgentState.IsSnapshot,
													   PlatformTypeId = rtaAgentState.PlatformTypeId,
													   SourceId = dataSourceId.ToString(CultureInfo.InvariantCulture),
													   StateCode = _loggedOutCode,
													   StateDescription = rtaAgentState.StateDescription,
													   Timestamp = _currentBatchId,
													   UserCode = a
												   })
												   .ToList();

				missingUsers.ForEach(u => _processAgentState(u));

				_currentBatch.Clear();
				_firstInBatch = true;
			}
			else
			{
				_currentBatch.Add(rtaAgentState);
			}
			return 1;
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