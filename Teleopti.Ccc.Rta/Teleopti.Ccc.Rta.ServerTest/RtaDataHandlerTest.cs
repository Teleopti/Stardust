using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Teleopti.Ccc.Rta.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Server; 
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class RtaDataHandlerTest
	{
		private MockRepository _mocks;
		private RtaDataHandler _target;
		private IActualAgentAssembler _agentAssembler;
		private IMessageSender _asyncMessageSender;
		private IDataSourceResolver _dataSourceResolver;
		private IPersonResolver _personResolver;
	    
		private string _logOn;
		private string _stateCode;
		private TimeSpan _timeInState;
		private DateTime _timestamp;
		private Guid _platformTypeId;
		private string _sourceId;
		private DateTime _batchId;
		private bool _isSnapshot;

		private Guid _personId;
		private Guid _businessUnitId;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_agentAssembler = _mocks.StrictMock<IActualAgentAssembler>();
			_mocks.DynamicMock<ILog>();
			_asyncMessageSender = _mocks.StrictMock<IMessageSender>();
			_dataSourceResolver = _mocks.StrictMock<IDataSourceResolver>();
			_personResolver = _mocks.DynamicMock<IPersonResolver>();
			
		    
			_logOn = "002";
			_stateCode = "AUX2";
			_timeInState = TimeSpan.FromSeconds(45);
			_timestamp = DateTime.Now;
			_platformTypeId = Guid.Empty;
			_sourceId = "1";
			_batchId = SqlDateTime.MinValue.Value;
			_isSnapshot = false;

			_personId = Guid.NewGuid();
			_businessUnitId = Guid.NewGuid();
		}

		[Test]
		public void ProcessRtaData_SameStateCode_ShouldAddToDatabaseCache_ButNotSendOverMessageBroker()
		{
			_asyncMessageSender = MockRepository.GenerateMock<IMessageSender>();

			_asyncMessageSender.StartBrokerService(useLongPolling:true);
			var agentState = new ActualAgentState {SendOverMessageBroker = false};
			int datasourceId;
			IEnumerable<PersonWithBusinessUnit> outEnumerable;
			var retPersonBusinessUnits = new List<PersonWithBusinessUnit>
				{
					new PersonWithBusinessUnit {BusinessUnitId = _businessUnitId, PersonId = _personId}
				};
			

			_dataSourceResolver.Expect(d => d.TryResolveId(_sourceId, out datasourceId)).OutRef(1).Return(true);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outEnumerable)).OutRef(retPersonBusinessUnits).Return(true);
			_agentAssembler.Expect(
				a =>
				a.GetAgentState(_personId, _businessUnitId, _platformTypeId, _stateCode, _timestamp, _timeInState, null,
				                _sourceId)).Return(agentState);
			_mocks.ReplayAll();


			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>(), null);
			_target.ProcessRtaData(_logOn, _stateCode, _timeInState, _timestamp, _platformTypeId, _sourceId, _batchId, _isSnapshot);

			_asyncMessageSender.AssertWasNotCalled(a => a.SendNotification(null), a => a.IgnoreArguments());
			_mocks.VerifyAll();

		}

		[Test]
		public void ShouldClearCacheWhenCheckScheduleIsCalled()
		{
			var agentHandler = MockRepository.GenerateMock<IActualAgentAssembler>();
			var target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, agentHandler, MockRepository.GenerateMock<IDatabaseWriter>(), null);
			var personId = Guid.NewGuid();
			var timeStamp = new DateTime(2000, 1, 1);

			target.ProcessScheduleUpdate(personId, Guid.NewGuid(), timeStamp);
			agentHandler.AssertWasCalled(x => x.InvalidateReadModelCache(personId));
		}

		[Test]
		public void VerifyProtectedConstructorWorks()
		{
			_asyncMessageSender.Expect(e => e.StartBrokerService(useLongPolling: true));
			_mocks.ReplayAll();

			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>(), null);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyProtectedConstructorCatchBrokerException()
		{
			_asyncMessageSender.Expect(e => e.StartBrokerService(useLongPolling: true)).Throw(new BrokerNotInstantiatedException());
			_mocks.ReplayAll();

			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>(), null);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyErrorWhenMessageBrokerNotInstantiated()
		{
			_asyncMessageSender.StartBrokerService(useLongPolling: true);
			LastCall.Throw(new BrokerNotInstantiatedException());
			_mocks.ReplayAll();

			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>(), null);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyIsAlive()
		{
			_asyncMessageSender.StartBrokerService(useLongPolling: true);
			Expect.Call(_asyncMessageSender.IsAlive).Return(true);
			_mocks.ReplayAll();

			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>(), null);
			Assert.IsTrue(_target.IsAlive);
			_mocks.VerifyAll();
		}
		
		[Test]
		public void ShouldNotSendWhenWrongDataSource()
		{
			_asyncMessageSender.StartBrokerService(useLongPolling: true);
			int dataSource;
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(false).OutRef(1);
			
			_mocks.ReplayAll();
			assignTargetAndRun();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotSendWhenWrongPerson()
		{
			int dataSource;
			IEnumerable<PersonWithBusinessUnit> outPersonBusinessUnits;

			_asyncMessageSender.StartBrokerService(useLongPolling: true);
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(false).OutRef(new object[1]);
			
			_mocks.ReplayAll();
			assignTargetAndRun();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotSendWhenStateHaveNotChanged()
		{
			int dataSource;
			IEnumerable<PersonWithBusinessUnit> outPersonBusinessUnits;
			var retPersonBusinessUnits = new List<PersonWithBusinessUnit>
			                             	{
			                             		new PersonWithBusinessUnit
			                             			{
			                             				BusinessUnitId = Guid.Empty,
			                             				PersonId = Guid.Empty
			                             			}
			                             	};
			var agentState = new ActualAgentState();

			_asyncMessageSender.StartBrokerService(useLongPolling: true);
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_agentAssembler.Expect(
				a => a.GetAgentState(Guid.Empty, Guid.Empty, Guid.Empty, _stateCode, _timestamp, _timeInState, null, _sourceId)).Return(agentState);

			_mocks.ReplayAll();
			assignTargetAndRun();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotSendWhenAgentStateIsNull()
		{
			int dataSource;
			IEnumerable<PersonWithBusinessUnit> outPersonBusinessUnits;
			var retPersonBusinessUnits = new List<PersonWithBusinessUnit>
			                             	{
			                             		new PersonWithBusinessUnit
			                             			{
			                             				BusinessUnitId = Guid.Empty,
			                             				PersonId = Guid.Empty
			                             			}
			                             	};

			_asyncMessageSender.StartBrokerService(useLongPolling: true);
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_agentAssembler.Expect(
				r => r.GetAgentState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState, new DateTime(), "")).
				IgnoreArguments().Return(
					null);
			
			_mocks.ReplayAll();
			assignTargetAndRun();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSend()
		{
			int dataSource;
			IEnumerable<PersonWithBusinessUnit> outPersonBusinessUnits;
			var retPersonBusinessUnits = new List<PersonWithBusinessUnit>
			                             	{
			                             		new PersonWithBusinessUnit
			                             			{
			                             				BusinessUnitId = Guid.Empty,
			                             				PersonId = Guid.Empty
			                             			}
			                             	};
			var agentState = new ActualAgentState{SendOverMessageBroker = true};

			_asyncMessageSender.StartBrokerService(useLongPolling: true);
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_agentAssembler.Expect(
				r => r.GetAgentState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState, new DateTime(), "")).
				IgnoreArguments().Return(agentState);
			_asyncMessageSender.Expect(m => m.SendNotification(null)).IgnoreArguments();
			
			_mocks.ReplayAll();

			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>(), null);
			_target.ProcessRtaData(_logOn, _stateCode, _timeInState, _timestamp, _platformTypeId, _sourceId, _batchId,
								   _isSnapshot);
			
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCallThingWhenStateChanges()
		{
			int dataSource;
			IEnumerable<PersonWithBusinessUnit> outPersonBusinessUnits;
			var retPersonBusinessUnits = new List<PersonWithBusinessUnit>
			                             	{
			                             		new PersonWithBusinessUnit
			                             			{
			                             				BusinessUnitId = Guid.Empty,
			                             				PersonId = Guid.Empty
			                             			}
			                             	};
			var agentState = new ActualAgentState { SendOverMessageBroker = true };
			var afterSend = MockRepository.GenerateMock<IActualAgentStateHasBeenSent>();

			var asyncMessageSender = MockRepository.GenerateStub<IMessageSender>();
			_dataSourceResolver.Stub(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Stub(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_agentAssembler.Stub(
				r => r.GetAgentState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState, new DateTime(), "")).
				IgnoreArguments().Return(agentState);
			
			
			_mocks.ReplayAll();

			_target = new RtaDataHandler(asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>(), afterSend);
			_target.ProcessRtaData(_logOn, _stateCode, _timeInState, _timestamp, _platformTypeId, _sourceId, _batchId,
								   _isSnapshot);

			_mocks.VerifyAll();

			afterSend.AssertWasCalled(s => s.Invoke(agentState));
		}

		[Test]
		public void ShouldCheckSchedule()
		{
			var agentState = new ActualAgentState
				{
					SendOverMessageBroker = true,
					PersonId = _personId,
					BusinessUnit = _businessUnitId
				};

			_asyncMessageSender.StartBrokerService(useLongPolling: true);
			_agentAssembler.Expect(a => a.InvalidateReadModelCache(_personId));
			_agentAssembler.Expect(a => a.GetAgentStateForScheduleUpdate(_personId, _businessUnitId, _timestamp)).IgnoreArguments().Return(
				agentState);
			_asyncMessageSender.Expect(m => m.SendNotification(null)).IgnoreArguments();
			_mocks.ReplayAll();

			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>(), null);
			_target.ProcessScheduleUpdate(_personId, _businessUnitId, _timestamp);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotSendWhenStateHaveNotChangedForScheduleUpdate()
		{

			_asyncMessageSender.StartBrokerService(useLongPolling: true);
			_agentAssembler.Expect(a => a.GetAgentStateForScheduleUpdate(_personId, _businessUnitId, _timestamp)).IgnoreArguments().Return(null);
			_agentAssembler.Expect(t => t.InvalidateReadModelCache(_personId));
			_mocks.ReplayAll();


			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>(), null);
			_target.ProcessScheduleUpdate(_personId, _businessUnitId, _timestamp);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFromConstructorWhenNoMessageSender()
		{
			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>(), null);
		}
		
		private void assignTargetAndRun()
		{
			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>(), null);
			_target.ProcessRtaData(_logOn, _stateCode, _timeInState, _timestamp, _platformTypeId, _sourceId, _batchId,
			                       _isSnapshot);
		}

		[Test]
		public void ProcessRtaData_HandleLastOfBatch()
		{
			_asyncMessageSender.StartBrokerService(useLongPolling: true);
			_isSnapshot = true;
			_logOn = "";
			var agentState = new ActualAgentState();

			int dataSource;
			_dataSourceResolver.Expect(d => d.TryResolveId(_sourceId, out dataSource)).Return(true);

			_agentAssembler.Expect(a => a.GetAgentStatesForMissingAgents(_batchId, _sourceId))
			               .Return(new List<IActualAgentState> {agentState, null});
			_asyncMessageSender.Expect(m => m.SendNotification(null)).IgnoreArguments();
			_mocks.ReplayAll();
			
			assignTargetAndRun();
			_mocks.VerifyAll();
		}
	}

}