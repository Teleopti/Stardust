﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Net.Sockets;
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
		private RtaDataHandlerForTest _target;
		private IActualAgentHandler _agentHandler;
		private ILog _loggingSvc;
		private IMessageSender _messageSender;
		private IDatabaseConnectionFactory _databaseConnectionFactory;
		private const string ConnectionString = "connection";		
		private IDataSourceResolver _dataSourceResolver;
		private IPersonResolver _personResolver;
		private IStateResolver _stateResolver;
	    
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
			_agentHandler = _mocks.DynamicMock<IActualAgentHandler>();
			_loggingSvc = _mocks.DynamicMock<ILog>();
			_messageSender = _mocks.StrictMock<IMessageSender>();
			_databaseConnectionFactory = _mocks.StrictMock<IDatabaseConnectionFactory>();
			_dataSourceResolver = _mocks.StrictMock<IDataSourceResolver>();
			_personResolver = _mocks.DynamicMock<IPersonResolver>();
			_stateResolver = _mocks.DynamicMock<IStateResolver>();
		    
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
		public void VerifyCreateInstanceUsingEmptyConstructorFailsBecauseNoConfigurationAvailable()
		{
			new RtaDataHandler(_agentHandler);
		}

		[Test]
		public void ShouldClearCacheWhenCheckScheduleIsCalled()
		{
			var agentHandler = MockRepository.GenerateMock<IActualAgentHandler>();
			var target = new RtaDataHandler(_loggingSvc, _messageSender, ConnectionString, _databaseConnectionFactory, _dataSourceResolver, _personResolver, _stateResolver, agentHandler);
			var personId = Guid.NewGuid();
			var timeStamp = new DateTime(2000, 1, 1);

			target.ProcessScheduleUpdate(personId, Guid.NewGuid(), timeStamp);
			agentHandler.AssertWasCalled(x => x.InvalidateReadModelCache(personId, timeStamp));
		}

		[Test]
		public void VerifyProtectedConstructorWorks()
		{
			_messageSender.Expect(e => e.InstantiateBrokerService());
			_mocks.ReplayAll();
			new RtaDataHandlerForTest(_loggingSvc, _messageSender, ConnectionString, _databaseConnectionFactory, _dataSourceResolver,
			                          _personResolver, _stateResolver);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyProtectedConstructorCatchBrokerException()
		{
			_messageSender.Expect(e => e.InstantiateBrokerService()).Throw(new BrokerNotInstantiatedException());
			_loggingSvc.Expect(l => l.Error("", new BrokerNotInstantiatedException())).IgnoreArguments();
			_mocks.ReplayAll();
			new RtaDataHandlerForTest(_loggingSvc, _messageSender, ConnectionString, _databaseConnectionFactory, _dataSourceResolver,
			                          _personResolver, _stateResolver);
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
			_target = new RtaDataHandlerForTest(_loggingSvc, _messageSender, ConnectionString, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver, _stateResolver, _agentHandler);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyIsAlive()
		{
			_messageSender.InstantiateBrokerService();
			Expect.Call(_messageSender.IsAlive).Return(true);
			_mocks.ReplayAll();
			_target = new RtaDataHandlerForTest(_loggingSvc, _messageSender, ConnectionString, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver, _stateResolver, _agentHandler);
			Assert.IsTrue(_target.IsAlive);
			_mocks.VerifyAll();
		}

		[Test]
		public void	VerifyWarningWithNoConnectionString()
		{
			_messageSender.InstantiateBrokerService();
			_loggingSvc.Expect(l => l.Error("No connection information available in configuration file."));
			_mocks.ReplayAll();

			_target = new RtaDataHandlerForTest(_loggingSvc, _messageSender, string.Empty, _databaseConnectionFactory, _dataSourceResolver, _personResolver, _stateResolver, _agentHandler);
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
			AssignTargetAndRun();
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
			AssignTargetAndRun();
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
			
			_messageSender.InstantiateBrokerService();
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_messageSender.Expect(m => m.IsAlive).Return(true);
			_stateResolver.Expect(s => s.HaveStateCodeChanged(Guid.Empty, _stateCode, _timestamp)).Return(false);
			
			_mocks.ReplayAll();
			AssignTargetAndRun();
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
			_messageSender.Expect(m => m.IsAlive).Return(true);
			_stateResolver.Expect(s => s.HaveStateCodeChanged(Guid.Empty, _stateCode, _timestamp)).Return(true);
			_agentHandler.Expect(
				r => r.GetAndSaveState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState)).
				IgnoreArguments().Return(
					null);
			
			_mocks.ReplayAll();
			AssignTargetAndRun();
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
			var agentState = new ActualAgentState();

			_messageSender.InstantiateBrokerService();
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_messageSender.Expect(m => m.IsAlive).Return(true);
			_stateResolver.Expect(s => s.HaveStateCodeChanged(Guid.Empty, _stateCode, _timestamp)).Return(true);
			_agentHandler.Expect(
				r => r.GetAndSaveState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState)).
				IgnoreArguments().Return(agentState);
			_messageSender.Expect(m => m.SendRtaData(Guid.Empty, Guid.Empty, agentState));
			
			_mocks.ReplayAll();

			_target = new RtaDataHandlerForTest(_loggingSvc, _messageSender, "connectionStringDataStore", _databaseConnectionFactory,
												_dataSourceResolver, _personResolver, _stateResolver, _agentHandler);
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
			var agentState = new ActualAgentState();

			_messageSender.InstantiateBrokerService();
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_messageSender.Expect(m => m.IsAlive).Return(true);
			_stateResolver.Expect(s => s.HaveStateCodeChanged(Guid.Empty, _stateCode, _timestamp)).Return(true);
			_agentHandler.Expect(
				r => r.GetAndSaveState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState)).
				IgnoreArguments().Return(agentState);
			_messageSender.Expect(m => m.SendRtaData(Guid.Empty, Guid.Empty, agentState)).Throw(new SocketException());
			_loggingSvc.Expect(l => l.Error("", new SocketException())).IgnoreArguments();

			_mocks.ReplayAll();
			AssignTargetAndRun();
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
			var agentState = new ActualAgentState();

			_messageSender.InstantiateBrokerService();
			_dataSourceResolver.Expect(d => d.TryResolveId("1", out dataSource)).Return(true).OutRef(1);
			_personResolver.Expect(p => p.TryResolveId(1, _logOn, out outPersonBusinessUnits)).Return(true).OutRef(
				retPersonBusinessUnits);
			_messageSender.Expect(m => m.IsAlive).Return(true);
			_stateResolver.Expect(s => s.HaveStateCodeChanged(Guid.Empty, _stateCode, _timestamp)).Return(true);
			_agentHandler.Expect(
				r => r.GetAndSaveState(Guid.Empty, Guid.Empty, _platformTypeId, _stateCode, _timestamp, _timeInState)).
				IgnoreArguments().Return(agentState);
			_messageSender.Expect(m => m.SendRtaData(Guid.Empty, Guid.Empty, agentState)).Throw(new BrokerNotInstantiatedException());
			_loggingSvc.Expect(l => l.Error("", new SocketException())).IgnoreArguments();

			_mocks.ReplayAll();
			AssignTargetAndRun();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCheckSchedule()
		{
			var agentState = new ActualAgentState();

			_messageSender.InstantiateBrokerService();
			_agentHandler.Expect(a => a.CheckSchedule(_personId, _businessUnitId, _timestamp)).IgnoreArguments().Return(
				agentState);
			_messageSender.Expect(m => m.SendRtaData(_personId, _businessUnitId, agentState));
			_mocks.ReplayAll();

			_target = new RtaDataHandlerForTest(_loggingSvc, _messageSender, ConnectionString, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver, _stateResolver, _agentHandler);
			_target.ProcessScheduleUpdate(_personId, _businessUnitId, _timestamp);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotSendWhenConnectionStringIsNull()
		{
			_messageSender.InstantiateBrokerService();
			_loggingSvc.Expect(l => l.Error("No connection information avaiable in configuration file."));
			_mocks.ReplayAll();

			_target = new RtaDataHandlerForTest(_loggingSvc, _messageSender, null, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver, _stateResolver, _agentHandler);
			_target.ProcessScheduleUpdate(_personId, _businessUnitId, _timestamp);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotSendWhenStateHaveNotChangedForScheduleUpdate()
		{
			_messageSender.InstantiateBrokerService();
			_agentHandler.Expect(a => a.CheckSchedule(_personId, _businessUnitId, _timestamp)).IgnoreArguments().Return(null);
			_loggingSvc.Expect(l => l.InfoFormat("Schedule for {0} has not changed", _personId));
			_mocks.ReplayAll();

			_target = new RtaDataHandlerForTest(_loggingSvc, _messageSender, ConnectionString, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver, _stateResolver, _agentHandler);
			_target.ProcessScheduleUpdate(_personId, _businessUnitId, _timestamp);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFromConstructorWhenNoMessageSender()
		{
			_target = new RtaDataHandlerForTest(_loggingSvc, null, ConnectionString, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver, _stateResolver);
		}

		private void AssignTargetAndRun()
		{
			_target = new RtaDataHandlerForTest(_loggingSvc, _messageSender, ConnectionString, _databaseConnectionFactory,
												_dataSourceResolver, _personResolver, _stateResolver, _agentHandler);
			_target.ProcessRtaData(_logOn, _stateCode, _timeInState, _timestamp, _platformTypeId, _sourceId, _batchId,
			                       _isSnapshot);
		}

		private class RtaDataHandlerForTest : RtaDataHandler
		{
			public RtaDataHandlerForTest(ILog loggingSvc, IMessageSender messageSender, string connectionStringDataStore,
			                             IDatabaseConnectionFactory databaseConnectionFactory,
			                             IDataSourceResolver dataSourceResolver, IPersonResolver personResolver,
										 IStateResolver stateResolver, IActualAgentHandler agentHandler)
				: base(
					loggingSvc, messageSender, connectionStringDataStore, databaseConnectionFactory, dataSourceResolver, personResolver,
                    stateResolver, agentHandler)
			{
			}

			public RtaDataHandlerForTest(ILog loggingSvc, IMessageSender messageSender, string connectionStringDataStore,
			                             IDatabaseConnectionFactory databaseConnectionFactory,
			                             IDataSourceResolver dataSourceResolver, IPersonResolver personResolver,
			                             IStateResolver stateResolver)
				: base(
					loggingSvc, messageSender, connectionStringDataStore, databaseConnectionFactory, dataSourceResolver, personResolver,
					stateResolver)
			{
			}
		}
	}
}