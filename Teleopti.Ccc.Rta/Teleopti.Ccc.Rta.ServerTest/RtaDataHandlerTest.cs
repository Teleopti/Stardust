using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Net.Sockets;
using Teleopti.Ccc.Rta.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Interfaces;
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
		private rtaDataHandlerForTest _target;
		private IActualAgentAssembler _agentAssembler;
		private ILog _loggingSvc;
		private IMessageSender _messageSender;
		private IDatabaseConnectionFactory _databaseConnectionFactory;
		private const string connectionString = "connection";		
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
			_loggingSvc = _mocks.DynamicMock<ILog>();
			_messageSender = _mocks.StrictMock<IMessageSender>();
			_databaseConnectionFactory = _mocks.StrictMock<IDatabaseConnectionFactory>();
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
			_messageSender = MockRepository.GenerateMock<IMessageSender>();

			_messageSender.InstantiateBrokerService();
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

			_target = new rtaDataHandlerForTest(_loggingSvc, _messageSender, connectionString, _databaseConnectionFactory, _dataSourceResolver, _personResolver,  _agentAssembler,_stateCache);
			_target.ProcessRtaData(_logOn, _stateCode, _timeInState, _timestamp, _platformTypeId, _sourceId, _batchId, _isSnapshot);

			_stateCache.AssertWasCalled(a => a.AddAgentStateToCache(agentState));
			_messageSender.AssertWasNotCalled(a => a.QueueRtaNotification(agentState.PersonId, agentState.BusinessUnit, agentState));
			_mocks.VerifyAll();

		}

		[Test]
		public void VerifyCreateInstanceUsingEmptyConstructorFailsBecauseNoConfigurationAvailable()
		{
			new RtaDataHandler(_agentAssembler, _stateCache, _messageSender);
		}

		[Test]
		public void ShouldClearCacheWhenCheckScheduleIsCalled()
		{
			var agentHandler = MockRepository.GenerateMock<IActualAgentAssembler>();
			var target = new RtaDataHandler(_loggingSvc, _messageSender, connectionString, _databaseConnectionFactory, _dataSourceResolver, _personResolver, agentHandler, _stateCache);
			var personId = Guid.NewGuid();
			var timeStamp = new DateTime(2000, 1, 1);

			target.ProcessScheduleUpdate(personId, Guid.NewGuid(), timeStamp);
			agentHandler.AssertWasCalled(x => x.InvalidateReadModelCache(personId));
		}

		[Test]
		public void VerifyProtectedConstructorWorks()
		{
			_messageSender.Expect(e => e.InstantiateBrokerService());
			_mocks.ReplayAll();
			new rtaDataHandlerForTest(_loggingSvc, _messageSender, connectionString, _databaseConnectionFactory, _dataSourceResolver,
									  _personResolver, _stateCache);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyProtectedConstructorCatchBrokerException()
		{
			_messageSender.Expect(e => e.InstantiateBrokerService()).Throw(new BrokerNotInstantiatedException());
			_loggingSvc.Expect(l => l.Error("", new BrokerNotInstantiatedException())).IgnoreArguments();
			_mocks.ReplayAll();
			new rtaDataHandlerForTest(_loggingSvc, _messageSender, connectionString, _databaseConnectionFactory, _dataSourceResolver,
			                          _personResolver,  _stateCache);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyErrorWhenMessageBrokerNotInstantiated()
		{
			_loggingSvc.Error("", new BrokerNotInstantiatedException());
			LastCall.IgnoreArguments();
			_messageSender.InstantiateBrokerService();
			LastCall.Throw(new BrokerNotInstantiatedException());
			_mocks.ReplayAll();
			_target = new rtaDataHandlerForTest(_loggingSvc, _messageSender, connectionString, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver,  _agentAssembler, _stateCache);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyIsAlive()
		{
			_messageSender.InstantiateBrokerService();
			Expect.Call(_messageSender.IsAlive).Return(true);
			_mocks.ReplayAll();
			_target = new rtaDataHandlerForTest(_loggingSvc, _messageSender, connectionString, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver,  _agentAssembler, _stateCache);
			Assert.IsTrue(_target.IsAlive);
			_mocks.VerifyAll();
		}

		[Test]
		public void	VerifyWarningWithNoConnectionString()
		{
			_messageSender.InstantiateBrokerService();
			_loggingSvc.Expect(l => l.Error("No connection information available in configuration file."));
			_mocks.ReplayAll();

			_target = new rtaDataHandlerForTest(_loggingSvc, _messageSender, string.Empty, _databaseConnectionFactory, _dataSourceResolver, _personResolver,  _agentAssembler, _stateCache);
			_target.ProcessRtaData(_logOn, _stateCode, _timeInState, _timestamp, _platformTypeId, _sourceId, _batchId,
			                       _isSnapshot);
			_mocks.VerifyAll();
		}
		
		[Test]
		public void ShouldNotSendWhenWrongDataSource()
		{
			_messageSender.InstantiateBrokerService();
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

			_messageSender.InstantiateBrokerService();
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

			_messageSender.InstantiateBrokerService();
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

			_messageSender.InstantiateBrokerService();
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

			_messageSender.InstantiateBrokerService();
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_agentAssembler.Expect(
				r => r.GetAgentState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState, new DateTime(), "")).
				IgnoreArguments().Return(agentState);
			_stateCache.Expect(s => s.AddAgentStateToCache(agentState));
			_messageSender.Expect(m => m.QueueRtaNotification(Guid.Empty, Guid.Empty, agentState));
			
			_mocks.ReplayAll();

			_target = new rtaDataHandlerForTest(_loggingSvc, _messageSender, "connectionStringDataStore", _databaseConnectionFactory,
												_dataSourceResolver, _personResolver,  _agentAssembler, _stateCache);
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

			_messageSender.InstantiateBrokerService();
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_agentAssembler.Expect(
				r => r.GetAgentState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState, new DateTime(), "")).
				IgnoreArguments().Return(agentState);
			_stateCache.Expect(s => s.AddAgentStateToCache(agentState));
			_messageSender.Expect(m => m.QueueRtaNotification(Guid.Empty, Guid.Empty, agentState)).Throw(new SocketException());
			_loggingSvc.Expect(l => l.Error("", new SocketException())).IgnoreArguments();

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

			_messageSender.InstantiateBrokerService();
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_agentAssembler.Expect(
				r => r.GetAgentState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState, new DateTime(), "")).
				IgnoreArguments().Return(agentState);
			_stateCache.Expect(s => s.AddAgentStateToCache(agentState));
			_messageSender.Expect(m => m.QueueRtaNotification(Guid.Empty, Guid.Empty, agentState)).Throw(new BrokerNotInstantiatedException());
			_loggingSvc.Expect(l => l.Error("", new SocketException())).IgnoreArguments();

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

			_messageSender.InstantiateBrokerService();
			_agentAssembler.Expect(a => a.InvalidateReadModelCache(_personId));
			_agentAssembler.Expect(a => a.GetAgentStateForScheduleUpdate(_personId, _businessUnitId, _timestamp)).IgnoreArguments().Return(
				agentState);
			var copyState = agentState;
			_stateCache.Expect(s => s.AddAgentStateToCache(copyState));
			_messageSender.Expect(m => m.QueueRtaNotification(_personId, _businessUnitId, agentState));
			_mocks.ReplayAll();

			_target = new rtaDataHandlerForTest(_loggingSvc, _messageSender, connectionString, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver,  _agentAssembler, _stateCache);
			_target.ProcessScheduleUpdate(_personId, _businessUnitId, _timestamp);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotSendWhenConnectionStringIsNull()
		{
			_messageSender.InstantiateBrokerService();
			_loggingSvc.Expect(l => l.Error("No connection information avaiable in configuration file."));
			_mocks.ReplayAll();

			_target = new rtaDataHandlerForTest(_loggingSvc, _messageSender, null, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver,  _agentAssembler, _stateCache);
			_target.ProcessScheduleUpdate(_personId, _businessUnitId, _timestamp);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotSendWhenStateHaveNotChangedForScheduleUpdate()
		{

			_messageSender.InstantiateBrokerService();
			_agentAssembler.Expect(a => a.GetAgentStateForScheduleUpdate(_personId, _businessUnitId, _timestamp)).IgnoreArguments().Return(null);
			_agentAssembler.Expect(t => t.InvalidateReadModelCache(_personId));
			_mocks.ReplayAll();


			_target = new rtaDataHandlerForTest(_loggingSvc, _messageSender, connectionString, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver, _agentAssembler, _stateCache);
			_target.ProcessScheduleUpdate(_personId, _businessUnitId, _timestamp);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFromConstructorWhenNoMessageSender()
		{
			_target = new rtaDataHandlerForTest(_loggingSvc, null, connectionString, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver,  _stateCache);
		}
		
		private void assignTargetAndRun()
		{
			_target = new rtaDataHandlerForTest(_loggingSvc, _messageSender, connectionString, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver,  _agentAssembler, _stateCache);
			_target.ProcessRtaData(_logOn, _stateCode, _timeInState, _timestamp, _platformTypeId, _sourceId, _batchId,
			                       _isSnapshot);
		}

		[Test]
		public void ProcessRtaData_HandleLastOfBatch()
		{
			_messageSender.InstantiateBrokerService();
			_isSnapshot = true;
			_logOn = "";
			var agentState = new ActualAgentState();

			int dataSource;
			_dataSourceResolver.Expect(d => d.TryResolveId(_sourceId, out dataSource)).Return(true);

			_stateCache.Expect(s => s.FlushCacheToDatabase());
			_agentAssembler.Expect(a => a.GetAgentStatesForMissingAgents(_batchId, _sourceId))
			               .Return(new List<IActualAgentState> {agentState, null});
			_stateCache.Expect(s => s.AddAgentStateToCache(agentState));
			_messageSender.Expect(m => m.QueueRtaNotification(Guid.Empty, Guid.Empty, agentState));
			_mocks.ReplayAll();
			
			assignTargetAndRun();
			_mocks.VerifyAll();
		}

		private class rtaDataHandlerForTest : RtaDataHandler
		{
			public rtaDataHandlerForTest(ILog loggingSvc,
			                             IMessageSender messageSender,
			                             string connectionStringDataStore,
			                             IDatabaseConnectionFactory databaseConnectionFactory,
			                             IDataSourceResolver dataSourceResolver,
			                             IPersonResolver personResolver,
			                             IActualAgentAssembler agentAssembler,
			                             IActualAgentStateCache stateCache)
				: base(
					loggingSvc, messageSender, connectionStringDataStore, databaseConnectionFactory, dataSourceResolver, personResolver, agentAssembler, stateCache)
			{
			}

			public rtaDataHandlerForTest(ILog loggingSvc,
			                             IMessageSender messageSender,
			                             string connectionStringDataStore,
			                             IDatabaseConnectionFactory databaseConnectionFactory,
			                             IDataSourceResolver dataSourceResolver,
			                             IPersonResolver personResolver,
			                             IActualAgentStateCache stateCache)
				: base(
					loggingSvc, messageSender, connectionStringDataStore, databaseConnectionFactory, dataSourceResolver, personResolver, stateCache)
			{
			}
		}
	}
}