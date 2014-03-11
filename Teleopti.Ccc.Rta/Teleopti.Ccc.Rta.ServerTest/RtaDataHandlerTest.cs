using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Net.Sockets;
using Teleopti.Ccc.Rta.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
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
		private IActualAgentStateCache _stateCache;
	    
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
			_stateCache = _mocks.StrictMock<IActualAgentStateCache>();
			
		    
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
			_stateCache = MockRepository.GenerateMock<IActualAgentStateCache>();
			_asyncMessageSender = MockRepository.GenerateMock<IMessageSender>();

			_asyncMessageSender.StartBrokerService();
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
			_stateCache.Expect(s => s.AddAgentStateToCache(agentState));
			_mocks.ReplayAll();


			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, _stateCache);
			_target.ProcessRtaData(_logOn, _stateCode, _timeInState, _timestamp, _platformTypeId, _sourceId, _batchId, _isSnapshot);

			_stateCache.AssertWasCalled(a => a.AddAgentStateToCache(agentState));
			_asyncMessageSender.AssertWasNotCalled(a => a.SendNotification(null), a => a.IgnoreArguments());
			_mocks.VerifyAll();

		}

		[Test]
		public void ShouldClearCacheWhenCheckScheduleIsCalled()
		{
			var agentHandler = MockRepository.GenerateMock<IActualAgentAssembler>();
			var target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, agentHandler, _stateCache);
			var personId = Guid.NewGuid();
			var timeStamp = new DateTime(2000, 1, 1);

			target.ProcessScheduleUpdate(personId, Guid.NewGuid(), timeStamp);
			agentHandler.AssertWasCalled(x => x.InvalidateReadModelCache(personId));
		}

		[Test]
		public void VerifyProtectedConstructorWorks()
		{
			_asyncMessageSender.Expect(e => e.StartBrokerService());
			_mocks.ReplayAll();

			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, _stateCache);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyProtectedConstructorCatchBrokerException()
		{
			_asyncMessageSender.Expect(e => e.StartBrokerService()).Throw(new BrokerNotInstantiatedException());
			_mocks.ReplayAll();

			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, _stateCache);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyErrorWhenMessageBrokerNotInstantiated()
		{
			_asyncMessageSender.StartBrokerService();
			LastCall.Throw(new BrokerNotInstantiatedException());
			_mocks.ReplayAll();

			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, _stateCache);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyIsAlive()
		{
			_asyncMessageSender.StartBrokerService();
			Expect.Call(_asyncMessageSender.IsAlive).Return(true);
			_mocks.ReplayAll();

			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, _stateCache);
			Assert.IsTrue(_target.IsAlive);
			_mocks.VerifyAll();
		}
		
		[Test]
		public void ShouldNotSendWhenWrongDataSource()
		{
			_asyncMessageSender.StartBrokerService();
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

			_asyncMessageSender.StartBrokerService();
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

			_asyncMessageSender.StartBrokerService();
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_agentAssembler.Expect(
				a => a.GetAgentState(Guid.Empty, Guid.Empty, Guid.Empty, _stateCode, _timestamp, _timeInState, null, _sourceId)).Return(agentState);
			_stateCache.AddAgentStateToCache(agentState);

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

			_asyncMessageSender.StartBrokerService();
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

			_asyncMessageSender.StartBrokerService();
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_agentAssembler.Expect(
				r => r.GetAgentState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState, new DateTime(), "")).
				IgnoreArguments().Return(agentState);
			_stateCache.Expect(s => s.AddAgentStateToCache(agentState));
			_asyncMessageSender.Expect(m => m.SendNotification(null)).IgnoreArguments();
			
			_mocks.ReplayAll();

			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, _stateCache);
			_target.ProcessRtaData(_logOn, _stateCode, _timeInState, _timestamp, _platformTypeId, _sourceId, _batchId,
								   _isSnapshot);
			
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCatchSocketException()
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

			_asyncMessageSender.StartBrokerService();
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_agentAssembler.Expect(
				r => r.GetAgentState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState, new DateTime(), "")).
				IgnoreArguments().Return(agentState);
			_stateCache.Expect(s => s.AddAgentStateToCache(agentState));
			_asyncMessageSender.Expect(m => m.SendNotification(null)).IgnoreArguments().Throw(new SocketException());			

			_mocks.ReplayAll();
			assignTargetAndRun();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCatchBrokerNotInstantiatedException()
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

			_asyncMessageSender.StartBrokerService();
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_agentAssembler.Expect(
				r => r.GetAgentState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState, new DateTime(), "")).
				IgnoreArguments().Return(agentState);
			_stateCache.Expect(s => s.AddAgentStateToCache(agentState));
			_asyncMessageSender.Expect(m => m.SendNotification(null)).IgnoreArguments().Throw(new BrokerNotInstantiatedException());
			

			_mocks.ReplayAll();
			assignTargetAndRun();
			_mocks.VerifyAll();
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

			_asyncMessageSender.StartBrokerService();
			_agentAssembler.Expect(a => a.InvalidateReadModelCache(_personId));
			_agentAssembler.Expect(a => a.GetAgentStateForScheduleUpdate(_personId, _businessUnitId, _timestamp)).IgnoreArguments().Return(
				agentState);
			var copyState = agentState;
			_stateCache.Expect(s => s.AddAgentStateToCache(copyState));
			_asyncMessageSender.Expect(m => m.SendNotification(null)).IgnoreArguments();
			_mocks.ReplayAll();

			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, _stateCache);
			_target.ProcessScheduleUpdate(_personId, _businessUnitId, _timestamp);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotSendWhenStateHaveNotChangedForScheduleUpdate()
		{

			_asyncMessageSender.StartBrokerService();
			_agentAssembler.Expect(a => a.GetAgentStateForScheduleUpdate(_personId, _businessUnitId, _timestamp)).IgnoreArguments().Return(null);
			_agentAssembler.Expect(t => t.InvalidateReadModelCache(_personId));
			_mocks.ReplayAll();


			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, _stateCache);
			_target.ProcessScheduleUpdate(_personId, _businessUnitId, _timestamp);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFromConstructorWhenNoMessageSender()
		{
			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver, _agentAssembler, _stateCache);
		}
		
		private void assignTargetAndRun()
		{
			_target = new RtaDataHandler(_asyncMessageSender, _dataSourceResolver, _personResolver,  _agentAssembler, _stateCache);
			_target.ProcessRtaData(_logOn, _stateCode, _timeInState, _timestamp, _platformTypeId, _sourceId, _batchId,
			                       _isSnapshot);
		}

		[Test]
		public void ProcessRtaData_HandleLastOfBatch()
		{
			_asyncMessageSender.StartBrokerService();
			_isSnapshot = true;
			_logOn = "";
			var agentState = new ActualAgentState();

			int dataSource;
			_dataSourceResolver.Expect(d => d.TryResolveId(_sourceId, out dataSource)).Return(true);

			_stateCache.Expect(s => s.FlushCacheToDatabase());
			_agentAssembler.Expect(a => a.GetAgentStatesForMissingAgents(_batchId, _sourceId))
			               .Return(new List<IActualAgentState> {agentState, null});
			_stateCache.Expect(s => s.AddAgentStateToCache(agentState));
			_asyncMessageSender.Expect(m => m.SendNotification(null)).IgnoreArguments();
			_mocks.ReplayAll();
			
			assignTargetAndRun();
			_mocks.VerifyAll();
		}
	}
}