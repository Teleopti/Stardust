﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class ActualAgentAssemblerTest
	{
		private IDatabaseHandler _dataHandler;
		private IActualAgentAssembler _target;
		private IMbCacheFactory _cacheFactory;
		private IAlarmMapper _alarmMapper;

		private RtaAlarmLight _rtaAlarmLight;

		private Guid _platformTypeId, _businessUnitId, _guid, _payloadId, _stateGroupId;
		private DateTime _dateTime, _batchId;
		private ScheduleLayer currentLayer, nextLayer;
		private string _stateCode, _sourceId;
		private const string loggedOutStateCode = "CCC Logged out";

		[SetUp]
		public void Setup()
		{
			_dataHandler = MockRepository.GenerateStrictMock<IDatabaseHandler>();
			_cacheFactory = MockRepository.GenerateStrictMock<IMbCacheFactory>();
			_alarmMapper = MockRepository.GenerateStrictMock<IAlarmMapper>();
			_target = new ActualAgentAssembler(_dataHandler, _cacheFactory, _alarmMapper);

			_stateCode = "AUX2";
			_platformTypeId = Guid.NewGuid();
			_businessUnitId = Guid.NewGuid();
			_payloadId = Guid.NewGuid();
			_stateGroupId = Guid.NewGuid();
			_dateTime = DateTime.UtcNow;
			_guid = Guid.NewGuid();

			_batchId = DateTime.UtcNow;
			_sourceId = "2";

			_rtaAlarmLight = new RtaAlarmLight
				{
					StateGroupId = _stateGroupId,
					Name = "SomeName",
					StateGroupName = "SomeStateGroupName",
					AlarmTypeId = _guid
				};

			currentLayer = new ScheduleLayer
			{
				Name = "CurrentLayer",
				StartDateTime = _dateTime,
				PayloadId = _payloadId
			};

			nextLayer = new ScheduleLayer {Name = "NextLayer"};
		}

		[Test]
		public void GetState_ReturnValidState()
		{

			var previousState = new ActualAgentState
				{
					AlarmId = _guid,
					StateStart = _dateTime
				};

			var alarmLight = new RtaAlarmLight
				{
					Name = "SomeName",
					AlarmTypeId = _guid
				};
			var stateGroup = new RtaStateGroupLight
									{
										PlatformTypeId = _platformTypeId,
										StateCode = _stateCode,
										BusinessUnitId = _businessUnitId,
										StateGroupId = _stateGroupId
									};

			_dataHandler.Expect(s => s.GetReadModel(_guid)).Return(new List<ScheduleLayer>());
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, new List<ScheduleLayer>()))
						.Return(new List<ScheduleLayer> { currentLayer, nextLayer }).IgnoreArguments();
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(previousState);
			_alarmMapper.Expect(a => a.GetStateGroup("AUX2", _platformTypeId, _businessUnitId)).Return(stateGroup);
			_alarmMapper.Expect(a => a.GetAlarm(currentLayer.PayloadId, stateGroup.StateGroupId)).Return(alarmLight);

			var result = _target.GetAgentState(_guid, _businessUnitId, _platformTypeId, _stateCode, _dateTime, new TimeSpan(), new DateTime(), "");
			Assert.That(result.AlarmName, Is.EqualTo(alarmLight.Name));
			Assert.That(result.StateStart, Is.EqualTo(currentLayer.StartDateTime));
			Assert.That(result.Scheduled, Is.EqualTo(currentLayer.Name));
			Assert.That(result.ScheduledNext, Is.EqualTo(nextLayer.Name));
		}

		[Test]
		public void GetAgentState_GetsReadModel_ReturnValidState()
		{
			var previousState = new ActualAgentState
			{
				AlarmId = _guid,
				StateStart = _dateTime
			};

			var alarmLight = new RtaAlarmLight
			{
				Name = "SomeName",
				AlarmTypeId = _guid
			};
			var stateGroup = new RtaStateGroupLight
				{
					PlatformTypeId = _platformTypeId,
					StateCode = _stateCode,
					BusinessUnitId = _businessUnitId,
					StateGroupId = _stateGroupId
				};
					

			_dataHandler.Expect(s => s.GetReadModel(_guid)).Return(new List<ScheduleLayer>{currentLayer, nextLayer});
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, new List<ScheduleLayer>()))
						.Return(new List<ScheduleLayer> { currentLayer, nextLayer }).IgnoreArguments();
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(previousState);
			_alarmMapper.Expect(a => a.GetStateGroup(_stateCode, _platformTypeId, _businessUnitId)).Return(stateGroup);
			_alarmMapper.Expect(a => a.GetAlarm(currentLayer.PayloadId, stateGroup.StateGroupId)).Return(alarmLight);

			var result = _target.GetAgentState(_guid, _businessUnitId, _platformTypeId, _stateCode, _dateTime, new TimeSpan(), new DateTime(), "");
			Assert.That(result.AlarmName, Is.EqualTo(alarmLight.Name));
			Assert.That(result.StateStart, Is.EqualTo(currentLayer.StartDateTime));
			Assert.That(result.Scheduled, Is.EqualTo(currentLayer.Name));
			Assert.That(result.ScheduledNext, Is.EqualTo(nextLayer.Name));
		}
		
		[Test]
		public void GetState_NextAndCurrentStateAreEqual_ReturnNull()
		{
			currentLayer = new ScheduleLayer
				{
					Name = "SameName",
					PayloadId = _stateGroupId,
					StartDateTime = _dateTime,
					EndDateTime = _dateTime
				};

			nextLayer = new ScheduleLayer
				{
					Name = "SameName",
					PayloadId = _stateGroupId,
					StartDateTime = _dateTime,
					EndDateTime = _dateTime,
				};

			var previousState = new ActualAgentState
				{
					ScheduledId = _stateGroupId,
					ScheduledNextId = _stateGroupId,
					AlarmId = _guid,
					StateId = _stateGroupId,
					ScheduledNext = "SameName",
					NextStart = _dateTime
				};

			_rtaAlarmLight = new RtaAlarmLight
				{
					StateGroupId = _stateGroupId,
					Name = "SomeName",
					StateGroupName = "SomeStateGroupName",
					AlarmTypeId = _guid
				};

			var stateGroup = new RtaStateGroupLight
									{
										PlatformTypeId = _platformTypeId,
										StateCode = _stateCode,
										BusinessUnitId = _businessUnitId,
										StateGroupId = _stateGroupId
									};
			var resetEvent = new AutoResetEvent(false);

			_dataHandler.Expect(s => s.GetReadModel(_guid)).Return(new List<ScheduleLayer>());
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, new List<ScheduleLayer>()))
						.Return(new List<ScheduleLayer> { currentLayer, nextLayer }).IgnoreArguments();
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(previousState);
			_alarmMapper.Expect(a => a.GetStateGroup("AUX2", _platformTypeId, _businessUnitId)).Return(stateGroup);
			_alarmMapper.Expect(a => a.GetAlarm(currentLayer.PayloadId, stateGroup.StateGroupId)).Return(_rtaAlarmLight);

			var result = _target.GetAgentState(_guid, _businessUnitId, _platformTypeId, _stateCode, _dateTime, new TimeSpan(), new DateTime(), "");
			Assert.IsNull(result);
			resetEvent.Dispose();
		}

		[Test]
		public void GetAgentStateForScheduleUdpate_ReturnValidState()
		{
			var previousState = new ActualAgentState
				{
					AlarmId = _guid,
					StateStart = _dateTime,
					StateCode = _stateCode,
					PlatformTypeId = _platformTypeId
				};

			var alarmLight = new RtaAlarmLight
				{
					Name = "SomeName",
					AlarmTypeId = _guid
				};

			var stateGroup = new RtaStateGroupLight
									{
										PlatformTypeId = _platformTypeId,
										StateCode = _stateCode,
										BusinessUnitId = _businessUnitId,
										StateGroupId = _stateGroupId
									};
			var resetEvent = new AutoResetEvent(false);

			_dataHandler.Expect(s => s.GetReadModel(_guid)).Return(new List<ScheduleLayer>());
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, new List<ScheduleLayer>()))
			            .Return(new List<ScheduleLayer> {currentLayer, nextLayer}).IgnoreArguments();
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(previousState);
			_alarmMapper.Expect(a => a.GetStateGroup("AUX2", _platformTypeId, _businessUnitId)).Return(stateGroup);
			_alarmMapper.Expect(a => a.GetAlarm(currentLayer.PayloadId, stateGroup.StateGroupId)).Return(alarmLight);

			var result = _target.GetAgentStateForScheduleUpdate(_guid, _businessUnitId, _dateTime);
			Assert.That(result.AlarmName, Is.EqualTo(alarmLight.Name));
			Assert.That(result.StateStart, Is.EqualTo(currentLayer.StartDateTime));
			Assert.That(result.Scheduled, Is.EqualTo(currentLayer.Name));
			Assert.That(result.ScheduledNext, Is.EqualTo(nextLayer.Name));
			resetEvent.Dispose();
		}

		[Test]
		public void GetAgentStateForScheduleUdpate_FoundReadModel_ReturnValidState()
		{
			var previousState = new ActualAgentState
				{
					AlarmId = _guid,
					StateStart = _dateTime,
					StateCode = _stateCode,
					PlatformTypeId = _platformTypeId
				};

			var alarmLight = new RtaAlarmLight
				{
					Name = "SomeName",
					AlarmTypeId = _guid
				};

			var stategroup = new RtaStateGroupLight
				{
					PlatformTypeId = _platformTypeId,
					StateCode = _stateCode,
					BusinessUnitId = _businessUnitId,
					StateGroupId = _stateGroupId
				};
			var resetEvent = new AutoResetEvent(false);

			_dataHandler.Expect(s => s.GetReadModel(_guid)).Return(new List<ScheduleLayer> {currentLayer, nextLayer});
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, new List<ScheduleLayer>()))
			            .Return(new List<ScheduleLayer> {currentLayer, nextLayer}).IgnoreArguments();
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(previousState);
			_alarmMapper.Expect(a => a.GetStateGroup("AUX2", _platformTypeId, _businessUnitId)).Return(stategroup);
			_alarmMapper.Expect(a => a.GetAlarm(currentLayer.PayloadId, stategroup.StateGroupId)).Return(alarmLight);
			
			var result = _target.GetAgentStateForScheduleUpdate(_guid, _businessUnitId, _dateTime);
			Assert.That(result.AlarmName, Is.EqualTo(alarmLight.Name));
			Assert.That(result.StateStart, Is.EqualTo(currentLayer.StartDateTime));
			Assert.That(result.Scheduled, Is.EqualTo(currentLayer.Name));
			Assert.That(result.ScheduledNext, Is.EqualTo(nextLayer.Name));
			resetEvent.Dispose();
		}

		[Test]
		public void GetAgentStateForScheduleUdpate_NoPreviousState_ReturnValidState()
		{
			var resetEvent = new AutoResetEvent(false);
			
			_dataHandler.Expect(s => s.GetReadModel(_guid)).Return(new List<ScheduleLayer>());
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, new List<ScheduleLayer>()))
			            .Return(new List<ScheduleLayer> {currentLayer, nextLayer}).IgnoreArguments();
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(null);
			_alarmMapper.Expect(a => a.GetStateGroup("", Guid.Empty, _businessUnitId)).Return(null);
			_alarmMapper.Expect(a => a.GetAlarm(currentLayer.PayloadId, Guid.Empty)).Return(null);
			
			var result = _target.GetAgentStateForScheduleUpdate(_guid, _businessUnitId, _dateTime);
			Assert.That(result.AlarmName, Is.EqualTo(string.Empty));
			Assert.That(result.Scheduled, Is.EqualTo(currentLayer.Name));
			Assert.That(result.ScheduledNext, Is.EqualTo(nextLayer.Name));
			resetEvent.Dispose();
		}

		[Test]
		public void GetAgentStateForScheduleUdpate_ActivityNotChanged_ReturnNull()
		{
			currentLayer = new ScheduleLayer
				{
					PayloadId = _payloadId,
					StartDateTime = new DateTime(2013, 02, 21, 00, 00, 00, DateTimeKind.Utc),
					EndDateTime = new DateTime(2013, 02, 21, 12, 00, 00, DateTimeKind.Utc)
				};
			var agentState = new ActualAgentState
				{
					ScheduledId = _payloadId,
					StateStart = new DateTime(2013, 02, 21, 00, 00, 00, DateTimeKind.Utc),
					NextStart = new DateTime(2013, 02, 21, 12, 00, 00, DateTimeKind.Utc)
				};
			var resetEvent = new AutoResetEvent(false);

			_dataHandler.Expect(s => s.GetReadModel(_guid)).Return(new List<ScheduleLayer>());
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, new List<ScheduleLayer>()))
						.Return(new List<ScheduleLayer> { currentLayer, new ScheduleLayer() }).IgnoreArguments();
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(agentState);

			var result = _target.GetAgentStateForScheduleUpdate(_guid, _businessUnitId, _dateTime);
			Assert.IsNull(result);
			resetEvent.Dispose();
		}

		[Test]
		public void GetAgentStateForMissingAgent_NoMissingAgents_ReturnEmptyCollection()
		{
			_dataHandler.Expect(d => d.GetMissingAgentStatesFromBatch(_batchId, _sourceId)).Return(new List<IActualAgentState>());

			var result = _target.GetAgentStatesForMissingAgents(_batchId, _sourceId);
			result.Count().Should().Be.EqualTo(0);
		}
		
		[Test]
		public void GetAgentStateForMissingAgent_NoMatchingAlarms_ReturnDefaultState()
		{
			_dataHandler.Expect(d => d.GetMissingAgentStatesFromBatch(_batchId, _sourceId)).Return(new List<IActualAgentState>
				{
					initializeAgentStateWithDefaults()
				});
			_alarmMapper.Expect(a => a.IsAgentLoggedOut(_guid, "StateCode", _guid, _guid)).Return(false);
			_alarmMapper.Expect(a => a.GetStateGroup(loggedOutStateCode, Guid.Empty, _guid)).Return(null);
			_alarmMapper.Expect(a => a.GetAlarm(_guid, Guid.Empty)).Return(null);
			_cacheFactory.Expect(c => c.Invalidate(_dataHandler, d => d.StateGroups(), false));

			var result = _target.GetAgentStatesForMissingAgents(_batchId, _sourceId);
			result.First().StateCode.Should().Be.EqualTo(loggedOutStateCode);
		}

		[Test]
		public void GetAgentStateForMissingAgent_AlreadyInAlarm_ReturnNull()
		{
			var oldState = initializeAgentStateWithDefaults();
			var stateGroup = new RtaStateGroupLight {StateGroupId = oldState.StateId};
			var alarm = new RtaAlarmLight {ActivityId = oldState.ScheduledId, StateGroupId = oldState.StateId};

			_dataHandler.Expect(d => d.GetMissingAgentStatesFromBatch(_batchId, _sourceId))
			            .Return(new List<IActualAgentState> {oldState});
			_alarmMapper.Expect(a => a.IsAgentLoggedOut(_guid, "StateCode", _guid, _guid)).Return(false);
			_alarmMapper.Expect(a => a.GetStateGroup(loggedOutStateCode, Guid.Empty, _guid)).Return(stateGroup);
			_alarmMapper.Expect(a => a.GetAlarm(oldState.ScheduledId, stateGroup.StateGroupId)).Return(alarm);

			var result = _target.GetAgentStatesForMissingAgents(_batchId, _sourceId);
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void GetAgentStateForMissingAgent_AlreadyLoggedOut_ReturnEmptyList()
		{
			var missingAgents = new List<IActualAgentState> {initializeAgentStateWithDefaults()};
			var rtaAlarmLight = new RtaAlarmLight {ActivityId = _guid, IsLogOutState = true, StateGroupId = _guid};
			var alarmDictionary = new ConcurrentDictionary<Guid, List<RtaAlarmLight>>();
			alarmDictionary.TryAdd(_guid, new List<RtaAlarmLight> {rtaAlarmLight});

			_dataHandler.Expect(d => d.GetMissingAgentStatesFromBatch(_batchId, _sourceId)).Return(missingAgents);
			_alarmMapper.Expect(a => a.IsAgentLoggedOut(_guid, "StateCode", _guid, _guid)).Return(true);
			_cacheFactory.Expect(c => c.Invalidate(_dataHandler,d => d.StateGroups(), false));
			
			var result = _target.GetAgentStatesForMissingAgents(_batchId, _sourceId);
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void GetAgentStateForMissingAgent_UpdateState_ReturnValidList()
		{
			var missingAgents = new List<IActualAgentState> {initializeAgentStateWithDefaults()};
			var stateGroup = new RtaStateGroupLight {StateGroupId = _guid};
			var rtaAlarmLight = new RtaAlarmLight {ActivityId = _guid, IsLogOutState = true,StateGroupName = "StateGroupName", StateGroupId = Guid.NewGuid()};

			_dataHandler.Expect(d => d.GetMissingAgentStatesFromBatch(_batchId, _sourceId)).Return(missingAgents);
			_alarmMapper.Expect(a => a.IsAgentLoggedOut(_guid, "StateCode", _guid, _guid)).Return(false);
			_alarmMapper.Expect(a => a.GetStateGroup(loggedOutStateCode, Guid.Empty, _guid)).Return(stateGroup);
			_alarmMapper.Expect(a => a.GetAlarm(_guid, _guid)).Return(rtaAlarmLight);
			_cacheFactory.Expect(c => c.Invalidate(_dataHandler, d => d.StateGroups(), false));

			var result = _target.GetAgentStatesForMissingAgents(_batchId, _sourceId);
			result.First().State.Should().Be.EqualTo("StateGroupName");
		}

		[Test]
		public void GetAgentStateForMissingAgent_UnknownScheduledActivity_ShouldSetAlarmToNoScheduledActivity()
		{
			var agent = initializeAgentStateWithDefaults();
			agent.ScheduledId = Guid.NewGuid();
			var missingAgents = new List<IActualAgentState> {agent};
			
			var noActivityAlarm = new RtaAlarmLight
				{
					ActivityId = Guid.Empty,
					Name = "NoScheduledActivity",
					IsLogOutState = true
				};
			var alarmDictionary = new ConcurrentDictionary<Guid, List<RtaAlarmLight>>();
			alarmDictionary.TryAdd(Guid.Empty, new List<RtaAlarmLight> {noActivityAlarm});			

			var stateGroup = new RtaStateGroupLight
				{
					BusinessUnitId = _guid,
					PlatformTypeId = _guid,
					StateCode = loggedOutStateCode,
					StateGroupName = "Logged Off",
					StateGroupId = _guid
				};
			var stateDictionary = new ConcurrentDictionary<string, List<RtaStateGroupLight>>();
			stateDictionary.TryAdd(loggedOutStateCode, new List<RtaStateGroupLight> {stateGroup});


			_dataHandler.Expect(d => d.GetMissingAgentStatesFromBatch(_batchId, _sourceId)).Return(missingAgents);
			_alarmMapper.Expect(a => a.IsAgentLoggedOut(agent.ScheduledId, "StateCode", _guid, _guid)).Return(false);
			_alarmMapper.Expect(a => a.GetStateGroup(loggedOutStateCode, Guid.Empty, _guid)).Return(stateGroup);
			_alarmMapper.Expect(a => a.GetAlarm(agent.ScheduledId, _guid)).Return(noActivityAlarm);

			var result = _target.GetAgentStatesForMissingAgents(_batchId, _sourceId).Single();
			result.AlarmName.Should().Be.EqualTo("NoScheduledActivity");
		}

		[Test]
		public void GetAgentState_UnKnownScheduledActivity_ShouldSetAlarmToNoSchuledActivity()
		{
			var stateGroup = new RtaStateGroupLight { BusinessUnitId = _guid, PlatformTypeId = _guid, StateGroupId = _stateGroupId};
			var activityAlarm = new RtaAlarmLight {ActivityId = Guid.Empty, StateGroupId = _stateGroupId, StateGroupName = "Logged out"};
			var scheduleLayer = new ScheduleLayer { PayloadId = Guid.NewGuid() };

			_dataHandler.Expect(d => d.GetReadModel(_guid)).Return(new List<ScheduleLayer>());
			_dataHandler.Expect(d => d.CurrentLayerAndNext(_dateTime, null)).IgnoreArguments().Return(new List<ScheduleLayer> {scheduleLayer, scheduleLayer});
			_dataHandler.Expect(d => d.LoadOldState(_guid)).Return(initializeAgentStateWithDefaults());
			_alarmMapper.Expect(a => a.GetStateGroup("stateCode", _guid, _guid)).Return(stateGroup);
			_alarmMapper.Expect(a => a.GetAlarm(scheduleLayer.PayloadId, _stateGroupId)).Return(activityAlarm);

			var result = _target.GetAgentState(_guid, _guid, _guid, "stateCode", _dateTime, new TimeSpan(), new DateTime(), "2");
			result.State.Should().Be.EqualTo("Logged out");
		}
		
		private IActualAgentState initializeAgentStateWithDefaults()
		{
			return new ActualAgentState
				{
					PersonId = _guid,
					BatchId = _batchId,
					ReceivedTime = _dateTime,
					StateId = _guid,
					Scheduled = "Scheduled",
					State = "State",
					StateCode = "StateCode",
					OriginalDataSourceId = "2",
					BusinessUnit = _guid,
					PlatformTypeId = _guid,
					Color = 0,
					StaffingEffect = 0,
					StateStart = _dateTime,
					ScheduledNext = "ScheduledNext",
					AlarmName = "AlarmName",
					AlarmId = _guid,
					AlarmStart = _dateTime,
					ScheduledId = _guid,
					ScheduledNextId = _guid,
					NextStart = _dateTime,
					TimeInState = new TimeSpan(0)
				};
		}
	}
}
