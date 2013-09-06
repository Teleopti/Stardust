using System;
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
		private MockRepository _mock;
		private IDatabaseHandler _dataHandler;
		private IActualAgentAssembler _target;
		private IMbCacheFactory _cacheFactory;

		private RtaAlarmLight _rtaAlarmLight;
		private ConcurrentDictionary<Guid, List<RtaAlarmLight>> _activityAlarms;

		private Guid _platformTypeId;
		private string _stateCode;
		private ScheduleLayer _scheduleLayer;
		private Guid _businessUnitId;

		private DateTime _dateTime;
		private Guid _guid;
		private Guid _payloadId;
		private Guid _stateGroupId;

		private DateTime _batchId;
		private string _sourceId;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_dataHandler = MockRepository.GenerateMock<IDatabaseHandler>();
			_cacheFactory = MockRepository.GenerateMock<IMbCacheFactory>();
			_target = new ActualAgentAssembler(_dataHandler, _cacheFactory);

			_stateCode = "AUX2";
			_platformTypeId = Guid.NewGuid();
			_businessUnitId = Guid.NewGuid();
			_payloadId = Guid.NewGuid();
			_stateGroupId = Guid.NewGuid();
			_dateTime = DateTime.UtcNow;
			_guid = Guid.NewGuid();

			_batchId = DateTime.UtcNow;
			_sourceId = "2";

			_scheduleLayer = new ScheduleLayer
				{
					PayloadId = _payloadId
				};
			_rtaAlarmLight = new RtaAlarmLight
				{
					StateGroupId = _stateGroupId,
					Name = "SomeName",
					StateGroupName = "SomeStateGroupName",
					AlarmTypeId = _guid
				};
			var temp = new Dictionary<Guid, List<RtaAlarmLight>>
				{
					{_payloadId, new List<RtaAlarmLight> {_rtaAlarmLight}}
				};
			_activityAlarms = new ConcurrentDictionary<Guid, List<RtaAlarmLight>>(temp);

		}

		[Test]
		public void VerifyGetState()
		{
			var currentLayer = new ScheduleLayer
				{
					Name = "CurrentLayer",
					StartDateTime = _dateTime,
					PayloadId = _payloadId
				};

			var nextLayer = new ScheduleLayer
				{
					Name = "NextLayer"
				};

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
			var dictionary = new Dictionary<string, List<RtaStateGroupLight>>
				{
					{
						_stateCode, new List<RtaStateGroupLight>
							{
								new RtaStateGroupLight
									{
										PlatformTypeId = _platformTypeId,
										StateCode = _stateCode,
										BusinessUnitId = _businessUnitId,
										StateGroupId = _stateGroupId
									}
							}
					}
				};

			_dataHandler.Expect(s => s.GetReadModel(_guid)).Return(new List<ScheduleLayer>());
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, new List<ScheduleLayer>()))
						.Return(new List<ScheduleLayer> { currentLayer, nextLayer }).IgnoreArguments();
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(previousState);
			_dataHandler.Expect(s => s.StateGroups())
			            .Return(new ConcurrentDictionary<string, List<RtaStateGroupLight>>(dictionary));

			_dataHandler.Expect(s => s.ActivityAlarms())
			            .Return(_activityAlarms);
			_mock.ReplayAll();

			var result = _target.GetAgentState(_guid, _businessUnitId, _platformTypeId, _stateCode, _dateTime, new TimeSpan(), new DateTime(), "");
			Assert.That(result.AlarmName, Is.EqualTo(alarmLight.Name));
			Assert.That(result.StateStart, Is.EqualTo(currentLayer.StartDateTime));
			Assert.That(result.Scheduled, Is.EqualTo(currentLayer.Name));
			Assert.That(result.ScheduledNext, Is.EqualTo(nextLayer.Name));
			_mock.VerifyAll();
		}

		[Test]
		public void VerifyGetStateNextAndCurrentAreEqual()
		{
			var currentLayer = new ScheduleLayer
				{
					Name = "SameName",
					PayloadId = _stateGroupId,
					StartDateTime = _dateTime,
					EndDateTime = _dateTime
				};
			var nextLayer = new ScheduleLayer
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
			var temp = new Dictionary<Guid, List<RtaAlarmLight>>
				{
					{_stateGroupId, new List<RtaAlarmLight> {_rtaAlarmLight}}
				};

			_activityAlarms = new ConcurrentDictionary<Guid, List<RtaAlarmLight>>(temp);
				
			var dictionary = new Dictionary<string, List<RtaStateGroupLight>>
				{
					{
						_stateCode, new List<RtaStateGroupLight>
							{
								new RtaStateGroupLight
									{
										PlatformTypeId = _platformTypeId,
										StateCode = _stateCode,
										BusinessUnitId = _businessUnitId,
										StateGroupId = _stateGroupId
									}
							}
					}
				};

			var resetEvent = new AutoResetEvent(false);
			_dataHandler.Expect(s => s.GetReadModel(_guid)).Return(new List<ScheduleLayer>());
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, new List<ScheduleLayer>()))
						.Return(new List<ScheduleLayer> { currentLayer, nextLayer }).IgnoreArguments();
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(previousState);
			_dataHandler.Expect(s => s.StateGroups())
			            .Return(new ConcurrentDictionary<string, List<RtaStateGroupLight>>(dictionary));
			_dataHandler.Expect(s => s.ActivityAlarms()).Return(_activityAlarms);
			_mock.ReplayAll();

			var result = _target.GetAgentState(_guid, _businessUnitId, _platformTypeId, _stateCode, _dateTime, new TimeSpan(), new DateTime(), "");
			Assert.IsNull(result);
			_mock.VerifyAll();
			resetEvent.Dispose();
		}

		[Test]
		public void ShouldReturnRtaAlarm()
		{
			var rtaStateGroupLight = new RtaStateGroupLight
				{
					PlatformTypeId = _platformTypeId,
					StateCode = _stateCode,
					BusinessUnitId = _businessUnitId,
					StateGroupId = _stateGroupId
				};
			var dictionary = new Dictionary<string, List<RtaStateGroupLight>>
				{
					{
						_stateCode, new List<RtaStateGroupLight>
							{
								rtaStateGroupLight
							}
					}
				};
			_dataHandler.Expect(d => d.StateGroups()).Return(new ConcurrentDictionary<string, List<RtaStateGroupLight>>(dictionary));
			_dataHandler.Expect(d => d.ActivityAlarms()).Return(_activityAlarms);

			_mock.ReplayAll();
			var result = _target.GetAlarm(_platformTypeId, _stateCode, _scheduleLayer, rtaStateGroupLight.StateGroupId);
			_mock.VerifyAll();
			Assert.That(result, Is.EqualTo(_rtaAlarmLight));
		}

		[Test]
		public void ShouldReturnNullWhenScheduleLayerIsNull()
		{
			_dataHandler.Expect(d => d.StateGroups()).Return(new ConcurrentDictionary<string, List<RtaStateGroupLight>>());
			_dataHandler.Expect(d => d.ActivityAlarms()).Return(_activityAlarms);

			_mock.ReplayAll();
			var result = _target.GetAlarm(_platformTypeId, _stateCode, null, Guid.Empty);
			_mock.VerifyAll();
			Assert.That(result, Is.Null);
		}

		[Test]
		public void ShouldReturnNullWhenNoMatchingStateGroup()
		{
			var dictionary = new Dictionary<string, List<RtaStateGroupLight>>
				{
					{
						_stateCode, new List<RtaStateGroupLight>
							{
								new RtaStateGroupLight()
							}
					}
				};
			_dataHandler.Expect(d => d.StateGroups()).Return(new ConcurrentDictionary<string, List<RtaStateGroupLight>>(dictionary));
			_dataHandler.Expect(d => d.ActivityAlarms()).Return(_activityAlarms);

			_mock.ReplayAll();
			var result = _target.GetAlarm(_platformTypeId, _stateCode, _scheduleLayer, Guid.Empty);
			_mock.VerifyAll();
			Assert.That(result, Is.Null);
		}

		[Test]
		public void ShouldCheckSchedule()
		{
			var currentLayer = new ScheduleLayer
				{
					Name = "CurrentLayer",
					StartDateTime = _dateTime,
					PayloadId = _payloadId
				};

			var nextLayer = new ScheduleLayer
				{
					Name = "NextLayer"
				};

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

			var dictionary = new Dictionary<string, List<RtaStateGroupLight>>
				{
					{
						_stateCode, new List<RtaStateGroupLight>
							{
								new RtaStateGroupLight
									{
										PlatformTypeId = _platformTypeId,
										StateCode = _stateCode,
										BusinessUnitId = _businessUnitId,
										StateGroupId = _stateGroupId
									}
							}
					}
				};
			var resetEvent = new AutoResetEvent(false);

			_dataHandler.Expect(s => s.GetReadModel(_guid)).Return(new List<ScheduleLayer>());
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, new List<ScheduleLayer>()))
			            .Return(new List<ScheduleLayer> {currentLayer, nextLayer}).IgnoreArguments();
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(previousState);
			_dataHandler.Expect(s => s.StateGroups()).Return(new ConcurrentDictionary<string, List<RtaStateGroupLight>>(dictionary));

			_dataHandler.Expect(s => s.ActivityAlarms())
			            .Return(_activityAlarms);
			_dataHandler.Expect(s => s.AddOrUpdate(new List<IActualAgentState> {new ActualAgentState()})).IgnoreArguments();
			_mock.ReplayAll();

			var result = _target.GetAgentStateForScheduleUpdate(_guid, _businessUnitId, _dateTime);
			Assert.That(result.AlarmName, Is.EqualTo(alarmLight.Name));
			Assert.That(result.StateStart, Is.EqualTo(currentLayer.StartDateTime));
			Assert.That(result.Scheduled, Is.EqualTo(currentLayer.Name));
			Assert.That(result.ScheduledNext, Is.EqualTo(nextLayer.Name));
			_mock.VerifyAll();
			resetEvent.Dispose();
		}
		
		[Test]
		public void ShouldCheckScheduleNoPreviousState()
		{
			var currentLayer = new ScheduleLayer
				{
					Name = "CurrentLayer",
					StartDateTime = _dateTime,
					PayloadId = _payloadId
				};

			var nextLayer = new ScheduleLayer
				{
					Name = "NextLayer"
				};
			
			var resetEvent = new AutoResetEvent(false);

			var dictionary = new Dictionary<string, List<RtaStateGroupLight>>
				{
					{
						_stateCode, new List<RtaStateGroupLight>
							{
								new RtaStateGroupLight()
							}
					}
				};

			_dataHandler.Expect(s => s.GetReadModel(_guid)).Return(new List<ScheduleLayer>());
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, new List<ScheduleLayer>()))
			            .Return(new List<ScheduleLayer> {currentLayer, nextLayer}).IgnoreArguments();
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(null);
			_dataHandler.Expect(s => s.StateGroups()).Return(new ConcurrentDictionary<string, List<RtaStateGroupLight>>(dictionary));

			_dataHandler.Expect(s => s.ActivityAlarms())
			            .Return(_activityAlarms);
			_dataHandler.Expect(s => s.AddOrUpdate(new List<IActualAgentState> {new ActualAgentState()})).IgnoreArguments();
			_mock.ReplayAll();

			var result = _target.GetAgentStateForScheduleUpdate(_guid, _businessUnitId, _dateTime);
			Assert.That(result.AlarmName, Is.EqualTo(string.Empty));
			Assert.That(result.Scheduled, Is.EqualTo(currentLayer.Name));
			Assert.That(result.ScheduledNext, Is.EqualTo(nextLayer.Name));
			_mock.VerifyAll();
			resetEvent.Dispose();
		}

		[Test]
		public void ShouldCheckScheduleSameActivity()
		{
			var currentLayer = new ScheduleLayer
				{
					PayloadId = _payloadId,
					StartDateTime = new DateTime(2013, 02, 21, 00, 00, 00, DateTimeKind.Utc),
					EndDateTime = new DateTime(2013, 02, 21, 12, 00, 00, DateTimeKind.Utc)
				};
			var resetEvent = new AutoResetEvent(false);

			_dataHandler.Expect(s => s.GetReadModel(_guid)).Return(new List<ScheduleLayer>());
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, new List<ScheduleLayer>()))
						.Return(new List<ScheduleLayer> { currentLayer, new ScheduleLayer() }).IgnoreArguments();
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(
				new ActualAgentState
					{
						ScheduledId = _payloadId,
						StateStart = new DateTime(2013, 02, 21, 00, 00, 00, DateTimeKind.Utc),
						NextStart = new DateTime(2013, 02, 21, 12, 00, 00, DateTimeKind.Utc)
					});
			_mock.ReplayAll();

			var result = _target.GetAgentStateForScheduleUpdate(_guid, _businessUnitId, _dateTime);
			Assert.IsNull(result);
			_mock.VerifyAll();
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
			_dataHandler.Expect(d => d.ActivityAlarms()).Return(new ConcurrentDictionary<Guid, List<RtaAlarmLight>>());
			_dataHandler.Expect(d => d.StateGroups()).Return(new ConcurrentDictionary<string, List<RtaStateGroupLight>>());
			_cacheFactory.Expect(c => c.Invalidate(_dataHandler, d => d.StateGroups(), false));

			var result = _target.GetAgentStatesForMissingAgents(_batchId, _sourceId);
			result.First().StateCode.Should().Be.EqualTo("LOGGED-OFF");
		}

		[Test]
		public void GetAgentStateForMissingAgent_NoLoggedOutState_ReturnDefaultState()
		{
			var missingAgents = new List<IActualAgentState> {initializeAgentStateWithDefaults()};
			var rtaAlarmLight = new RtaAlarmLight {ActivityId = _guid, StateGroupName = "SomethingSomething"};
			var alarmDictionary = new ConcurrentDictionary<Guid, List<RtaAlarmLight>>();
			alarmDictionary.TryAdd(_guid, new List<RtaAlarmLight> {rtaAlarmLight});

			_dataHandler.Expect(d => d.GetMissingAgentStatesFromBatch(_batchId, _sourceId)).Return(missingAgents);
			_dataHandler.Expect(d => d.ActivityAlarms()).Return(alarmDictionary);
			_dataHandler.Expect(d => d.StateGroups()).Return(new ConcurrentDictionary<string, List<RtaStateGroupLight>>());
			_cacheFactory.Expect(c => c.Invalidate(_dataHandler, d => d.StateGroups(), false));

			var result = _target.GetAgentStatesForMissingAgents(_batchId, _sourceId);
			result.First().StateCode.Should().Be.EqualTo("LOGGED-OFF");
		}

		[Test]
		public void GetAgentStateForMissingAgent_AlreadyLoggedOut_ReturnEmptyList()
		{
			var missingAgents = new List<IActualAgentState> {initializeAgentStateWithDefaults()};
			var rtaAlarmLight = new RtaAlarmLight {ActivityId = _guid, IsLogOutState = true, StateGroupId = _guid};
			var alarmDictionary = new ConcurrentDictionary<Guid, List<RtaAlarmLight>>();
			alarmDictionary.TryAdd(_guid, new List<RtaAlarmLight> {rtaAlarmLight});

			_dataHandler.Expect(d => d.GetMissingAgentStatesFromBatch(_batchId, _sourceId)).Return(missingAgents);
			_dataHandler.Expect(d => d.ActivityAlarms()).Return(alarmDictionary);
			_dataHandler.Expect(d => d.StateGroups()).Return(new ConcurrentDictionary<string, List<RtaStateGroupLight>>());
			_cacheFactory.Expect(c => c.Invalidate(_dataHandler,d => d.StateGroups(), false));
			

			var result = _target.GetAgentStatesForMissingAgents(_batchId, _sourceId);
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void GetAgentStateForMissingAgent_UpdateState_ReturnValidList()
		{
			var missingAgents = new List<IActualAgentState> {initializeAgentStateWithDefaults()};
			var rtaAlarmLight = new RtaAlarmLight {ActivityId = _guid, IsLogOutState = true,StateGroupName = "StateGroupName"};
			var alarmDictionary = new ConcurrentDictionary<Guid, List<RtaAlarmLight>>();
			alarmDictionary.TryAdd(_guid, new List<RtaAlarmLight> {rtaAlarmLight});

			_dataHandler.Expect(d => d.GetMissingAgentStatesFromBatch(_batchId, _sourceId)).Return(missingAgents);
			_dataHandler.Expect(d => d.ActivityAlarms()).Return(alarmDictionary);
			_dataHandler.Expect(d => d.StateGroups()).Return(new ConcurrentDictionary<string, List<RtaStateGroupLight>>());
			_cacheFactory.Expect(c => c.Invalidate(_dataHandler, d => d.StateGroups(), false));

			var result = _target.GetAgentStatesForMissingAgents(_batchId, _sourceId);
			result.First().State.Should().Be.EqualTo("StateGroupName");
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
