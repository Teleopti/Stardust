using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Teleopti.Ccc.Rta.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class RtaDataHandlerTest
	{
		private MockRepository _mocks;
		private RtaDataHandler _target;
		private IActualAgentAssembler _agentAssembler;
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
		public void ShouldClearCacheWhenCheckScheduleIsCalled()
		{
			var agentHandler = MockRepository.GenerateMock<IActualAgentAssembler>();
			var target = new RtaDataHandler(_dataSourceResolver, _personResolver, agentHandler, MockRepository.GenerateMock<IDatabaseWriter>());
			var personId = Guid.NewGuid();
			var timeStamp = new DateTime(2000, 1, 1);

			target.ProcessScheduleUpdate(personId, Guid.NewGuid(), timeStamp);
			agentHandler.AssertWasCalled(x => x.InvalidateReadModelCache(personId));
		}

		[Test]
		public void VerifyIsAlive()
		{
			_target = new RtaDataHandler(_dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>());
			_mocks.ReplayAll();
			Assert.IsTrue(_target.IsAlive);
			_mocks.VerifyAll();
		}
		
		[Test]
		public void ShouldNotSendWhenWrongDataSource()
		{
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
		public void ShouldCheckSchedule()
		{
			var agentState = new ActualAgentState
				{
					SendOverMessageBroker = true,
					PersonId = _personId,
					BusinessUnit = _businessUnitId
				};

			_agentAssembler.Expect(a => a.InvalidateReadModelCache(_personId));
			_agentAssembler.Expect(a => a.GetAgentStateForScheduleUpdate(_personId, _businessUnitId, _timestamp)).IgnoreArguments().Return(
				agentState);
			_mocks.ReplayAll();

			var databaseWriter = MockRepository.GenerateMock<IDatabaseWriter>();
			databaseWriter.Expect(x => x.AddOrUpdate(agentState));
			_target = new RtaDataHandler(_dataSourceResolver, _personResolver, _agentAssembler, databaseWriter);
			_target.ProcessScheduleUpdate(_personId, _businessUnitId, _timestamp);
			_mocks.VerifyAll();
		}
		
		
		private void assignTargetAndRun()
		{
			_target = new RtaDataHandler(_dataSourceResolver, _personResolver, _agentAssembler, MockRepository.GenerateMock<IDatabaseWriter>());
			_target.ProcessRtaData(_logOn, _stateCode, _timeInState, _timestamp, _platformTypeId, _sourceId, _batchId,
			                       _isSnapshot);
		}

		[Test]
		public void ProcessRtaData_HandleLastOfBatch()
		{
			_isSnapshot = true;
			_logOn = "";
			var agentState = new ActualAgentState();

			int dataSource;
			_dataSourceResolver.Expect(d => d.TryResolveId(_sourceId, out dataSource)).Return(true);

			_agentAssembler.Expect(a => a.GetAgentStatesForMissingAgents(_batchId, _sourceId))
			               .Return(new List<IActualAgentState> {agentState, null});
			_mocks.ReplayAll();
			
			assignTargetAndRun();
			_mocks.VerifyAll();
		}
	}
}