using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class ProcessMissingAgentsTest
	{
		private const string loggedOutCode = "LOGGED_OFF";

		string _authenticationKey;
		string _userCode;
		string _stateCode;
		string _stateDescription;
		bool _isLoggedOn;
		DateTime _timestamp1;
		DateTime _timestamp2;
		DateTime _timestamp3;
		string _platformTypeId;
		string _sourceId;
		DateTime _batchId1;
		DateTime _batchId2;
		private RtaAgentState _agentState1;
		private RtaAgentState _agentState2;
		private RtaAgentState _endOfState1;
		private RtaAgentState _endOfState2;
		private RtaAgentState? _processedState;
		private RtaProcessMissingAgents _target;

		[SetUp]
		public void Setup()
		{
			_authenticationKey = "aa";
			_userCode = "u1";
			_stateCode = "AUX1";
			_stateDescription = "Phone";
			_isLoggedOn = true;
			_timestamp1 = new DateTime(2001, 1, 1, 8, 0, 1);
			_timestamp2 = new DateTime(2001, 1, 1, 8, 0, 2);
			_timestamp3 = new DateTime(2001, 1, 1, 8, 0, 3);
			_platformTypeId = "Cisco";
			_sourceId = "xxx";
			_batchId1 = new DateTime(2001, 1, 1, 8, 1, 0);
			_batchId2 = new DateTime(2001, 1, 1, 8, 2, 0);
			_processedState = null;

			_agentState1 = new RtaAgentState()
				               {
					               AuthenticationKey = _authenticationKey,
					               UserCode = _userCode,
					               StateCode = _stateCode,
					               StateDescription = _stateDescription,
					               IsLoggedOn = _isLoggedOn,
					               Timestamp = _timestamp1,
					               PlatformTypeId = _platformTypeId,
					               SourceId = _sourceId,
					               BatchId = _batchId1,
					               IsSnapshot = true,
				               };

			_agentState2 = new RtaAgentState()
				               {
					               AuthenticationKey = _authenticationKey,
					               UserCode = _userCode,
					               StateCode = _stateCode,
					               StateDescription = _stateDescription,
					               IsLoggedOn = _isLoggedOn,
					               Timestamp = _timestamp2,
					               PlatformTypeId = _platformTypeId,
					               SourceId = _sourceId,
					               BatchId = _batchId2,
					               IsSnapshot = true,
				               };

			_endOfState1 = new RtaAgentState()
				               {
					               AuthenticationKey = _authenticationKey,
					               UserCode = String.Empty,
					               StateCode = _stateCode,
					               StateDescription = _stateDescription,
					               IsLoggedOn = _isLoggedOn,
					               Timestamp = _timestamp1,
					               PlatformTypeId = _platformTypeId,
					               SourceId = _sourceId,
					               BatchId = _batchId1,
					               IsSnapshot = true,
				               };

			_endOfState2 = new RtaAgentState()
				               {
					               AuthenticationKey = _authenticationKey,
					               UserCode = String.Empty,
					               StateCode = _stateCode,
					               StateDescription = _stateDescription,
					               IsLoggedOn = _isLoggedOn,
					               Timestamp = _timestamp2,
					               PlatformTypeId = _platformTypeId,
					               SourceId = _sourceId,
					               BatchId = _batchId2,
					               IsSnapshot = true,
				               };


			_target = new RtaProcessMissingAgents(loggedOutCode, (a) =>
				                                                     {
					                                                     _processedState = a;
				                                                     });
		}

		[Test]
		public void Check_WhenNotMissingFromNextBatch_ShouldNotProcessAnyAgents()
		{
			_target.Check(_agentState1);
			_target.Check(_endOfState1);
			_target.Check(_agentState2);
			_target.Check(_endOfState2);

			Assert.That(_processedState, Is.Null);
		}

		[Test]
		public void Check_WhenAgentOnlyExistsInFirstBatch_ShouldProcessMissingAgentAsLoggedOut()
		{
			makeAgentStateOnlyIncluededInFirstbatch();

			_target.Check(_agentState1);
			_target.Check(_endOfState1);
			_target.Check(_agentState2);
			_target.Check(_endOfState2);

			Assert.That(_processedState, Is.Not.Null, "Processed should have been called");
			Assert.That(ProcessedState.IsLoggedOn, Is.False, "Processed should have set the state to logged out");
			Assert.That(ProcessedState.UserCode, Is.EqualTo(_agentState1.UserCode), "Processed should have been called with the first agent");
			Assert.That(ProcessedState.StateCode, Is.EqualTo(loggedOutCode), "Processed should statuscode should have been set to logged out");
		}

		[Test]
		public void Check_WhenAgentOnlyExistsInFirstBatch_ShouldSetTheTimeStampToTheNextBatchId()
		{
			makeAgentStateOnlyIncluededInFirstbatch();

			_target.Check(_agentState1);
			_target.Check(_endOfState1);
			_target.Check(_agentState2);
			_target.Check(_endOfState2);

			Assert.That(ProcessedState.Timestamp, Is.EqualTo(_batchId2));
			Assert.That(ProcessedState.BatchId, Is.EqualTo(_batchId2));
		}

		[Test]
		public void Check_WhenThereAreManyAgentsInSecondBatch_ShouldProcessAgentsMissingInFirstBatchOnlyWhenSecondBatchIsFinished()
		{
			makeAgentStateOnlyIncluededInFirstbatch();

			_target.Check(_agentState1);
			_target.Check(_endOfState1);
			_target.Check(_agentState2);
			_target.Check(_agentState2);
			_target.Check(_agentState2);

			Assert.That(_processedState, Is.Null, "Processed should have been called, second batch is not finished");

			_target.Check(_endOfState2);

			Assert.That(_processedState, Is.Not.Null, "Processed should have been called");
		}

		[Test]
		public void Check_WhenThereAreManyMissingAgentsInFirstBatchThatExistsInSecondBatch_ShouldProcessAllAgentsMissingInFirstBatch()
		{
			makeAgentStateOnlyIncluededInFirstbatch();
			var processedAgents = new List<RtaAgentState>();
			var target = new RtaProcessMissingAgents(loggedOutCode, (a) => processedAgents.Add(a));

			target.Check(_agentState1);
			target.Check(new RtaAgentState()
				             {
					             AuthenticationKey = _agentState1.AuthenticationKey,
					             UserCode = _agentState1.UserCode + "something else",
					             StateCode = _agentState1.StateCode,
					             StateDescription = _agentState1.StateDescription,
					             IsLoggedOn = _isLoggedOn,
					             Timestamp = _timestamp3,
					             PlatformTypeId = _agentState1.PlatformTypeId,
					             SourceId = _agentState1.SourceId,
					             BatchId = _batchId1,
					             IsSnapshot = true
				             });

			target.Check(_endOfState1);
			target.Check(_agentState2);
			target.Check(_endOfState2);

			Assert.That(processedAgents.Count, Is.EqualTo(2));
		}

		[Test]
		public void Check_WhenAgentIsMissingInFirstBatch_ShouldProcessAgentWithTheOriginalValues()
		{
			makeAgentStateOnlyIncluededInFirstbatch();

			_target.Check(_agentState1);
			_target.Check(_endOfState1);
			_target.Check(new RtaAgentState()
				              {
					              AuthenticationKey = _agentState1.AuthenticationKey + "gdhaj",
					              UserCode = _agentState1.UserCode + "askldjasd sdasd",
					              StateDescription = _agentState1.StateDescription + "ajksnda asd",
					              IsLoggedOn = _isLoggedOn,
					              Timestamp = _timestamp3,
					              PlatformTypeId = _agentState1.PlatformTypeId + "ASKLd asldkaj sd",
					              SourceId = _agentState1.SourceId + "askldas dlas dljkasd",
					              BatchId = _batchId2,
					              IsSnapshot = true,
				              });
			_target.Check(_endOfState2);


			Assert.That(ProcessedState.AuthenticationKey, Is.EqualTo(_agentState1.AuthenticationKey), "AuthenticationKey");
			Assert.That(ProcessedState.UserCode, Is.EqualTo(_agentState1.UserCode), "UserCode");
			Assert.That(ProcessedState.StateDescription, Is.EqualTo(_agentState1.StateDescription), "StateDescription");
			Assert.That(ProcessedState.PlatformTypeId, Is.EqualTo(_agentState1.PlatformTypeId), "PlatformTypeId");
			Assert.That(ProcessedState.SourceId, Is.EqualTo(_agentState1.SourceId), "SourceId");
			Assert.That(ProcessedState.IsSnapshot, Is.EqualTo(_agentState1.IsSnapshot), "IsSnapshot");

		}

		[Test]
		public void Check_WhenNotSnapShot_ShouldNotProcessAgent()
		{
			_agentState1.IsSnapshot = false;
			makeAgentStateOnlyIncluededInFirstbatch();

			_target.Check(_agentState1);
			_target.Check(_endOfState1);
			_target.Check(_agentState2);
			_target.Check(_endOfState2);

			Assert.That(_processedState, Is.Null);
		}

		[Test]
		public void Check_WhenAlreadyLoggedOff_ShouldNotProcessAgent()
		{
			makeAgentStateOnlyIncluededInFirstbatch();
			_agentState1.IsLoggedOn = false;

			_target.Check(_agentState1);
			_target.Check(_endOfState1);
			_target.Check(_agentState2);
			_target.Check(_endOfState2);

			Assert.That(_processedState, Is.Null);
		}


		#region helpers

		private RtaAgentState ProcessedState
		{
			get { return (RtaAgentState)_processedState; }
		}

		private void makeAgentStateOnlyIncluededInFirstbatch()
		{
			_agentState2.UserCode = "somethingElse";
		}
		#endregion
	}
}