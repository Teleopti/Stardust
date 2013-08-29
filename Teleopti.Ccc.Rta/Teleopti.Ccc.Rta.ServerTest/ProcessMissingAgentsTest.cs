using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class ProcessMissingAgentsTest
	{
		private const string loggedOutCode = "LOGGED_OFF";

		string _authenticationKey;
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
		private IRtaBatchHandler _batchHandler;
		private List<string> _allUsersOnDataSource;

		[SetUp]
		public void Setup()
		{
			_authenticationKey = "aa";
			_stateCode = "AUX1";
			_stateDescription = "Phone";
			_isLoggedOn = true;
			_timestamp1 = new DateTime(2001, 1, 1, 8, 0, 1);
			_timestamp2 = new DateTime(2001, 1, 1, 8, 0, 2);
			_timestamp3 = new DateTime(2001, 1, 1, 8, 0, 3);
			_platformTypeId = "Cisco";
			_sourceId = "123";
			_batchId1 = new DateTime(2001, 1, 1, 8, 1, 0);
			_batchId2 = new DateTime(2001, 1, 1, 8, 2, 0);
			_processedState = null;

			_agentState1 = new RtaAgentState()
				               {
					               AuthenticationKey = _authenticationKey,
					               UserCode = "user1",
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
					               UserCode = "user2",
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
					               UserCode = string.Empty,
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
					               UserCode = string.Empty,
					               StateCode = _stateCode,
					               StateDescription = _stateDescription,
					               IsLoggedOn = _isLoggedOn,
					               Timestamp = _timestamp2,
					               PlatformTypeId = _platformTypeId,
					               SourceId = _sourceId,
					               BatchId = _batchId2,
					               IsSnapshot = true,
				               };
			_batchHandler = MockRepository.GenerateMock<IRtaBatchHandler>();
			_allUsersOnDataSource = new List<string>();
			_target = new RtaProcessMissingAgents(loggedOutCode, a =>
				{
					_processedState = a;
				}, _batchHandler);
		}

		[Test]
		public void Check_WhenAgentOnlyExistsInFirstBatch_ShouldProcessMissingAgentAsLoggedOut()
		{
			makeAgentStateOnlyIncluededInFirstbatch();


			_allUsersOnDataSource.AddRange(new[]
				{
					_agentState1.UserCode,
					_agentState2.UserCode
				});
			_batchHandler.Expect(b => b.PeopleOnDataSource(123)).Return(_allUsersOnDataSource);

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
			
			_allUsersOnDataSource.AddRange(new[]
				{
					_agentState1.UserCode,
					_agentState2.UserCode
				});
			_batchHandler.Expect(b => b.PeopleOnDataSource(123)).Return(_allUsersOnDataSource);

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


			_allUsersOnDataSource.AddRange(new[]
				{
					_agentState1.UserCode,
					_agentState2.UserCode
				});
			_batchHandler.Expect(b => b.PeopleOnDataSource(123)).Return(_allUsersOnDataSource);

			_target.Check(_agentState1);
			_target.Check(_endOfState1);
			_target.Check(_agentState2);
			_target.Check(_agentState2);
			_target.Check(_agentState2);

			Assert.That(_processedState, Is.Not.Null, "Processed should have been called, second batch is not finished");

			_target.Check(_endOfState2);

			Assert.That(_processedState, Is.Not.Null, "Processed should have been called");
		}

		[Test]
		public void Check_WhenThereAreManyMissingAgentsInFirstBatchThatExistsInSecondBatch_ShouldProcessAllAgentsMissingInFirstBatch()
		{
			makeAgentStateOnlyIncluededInFirstbatch();

			var tempState = new RtaAgentState
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
				};
			_allUsersOnDataSource.AddRange(new[]
				{
					_agentState1.UserCode,
					_agentState2.UserCode,
					tempState.UserCode
				});
			_batchHandler.Expect(b => b.PeopleOnDataSource(123)).Return(_allUsersOnDataSource);

			
			var processedAgents = new List<RtaAgentState>();
			var target = new RtaProcessMissingAgents(loggedOutCode, processedAgents.Add, _batchHandler);

			target.Check(_agentState1);
			target.Check(tempState);
			target.Check(_endOfState1);
			target.Check(_agentState2);
			target.Check(_endOfState2);

			Assert.That(processedAgents.Count, Is.EqualTo(3));
		}

		[Test]
		public void Check_WhenAgentIsMissingInFirstBatch_ShouldProcessAgentWithTheOriginalValues()
		{
			makeAgentStateOnlyIncluededInFirstbatch();

			var tempState = new RtaAgentState
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
				};

			_allUsersOnDataSource.AddRange(new[]
				{
					_agentState1.UserCode,
					tempState.UserCode
				});

			_batchHandler.Expect(b => b.PeopleOnDataSource(123)).Return(_allUsersOnDataSource);

			_target.Check(_agentState1);
			_target.Check(_endOfState1);
			_target.Check(tempState);
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

			_batchHandler.Expect(b => b.PeopleOnDataSource(123)).Return(new List<string>());

			_target.Check(_agentState1);
			_target.Check(_endOfState1);
			
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