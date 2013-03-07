﻿using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class ActualAgentHandlerTest
	{
		private MockRepository _mock;
		private IActualAgentDataHandler _dataHandler;
		private IActualAgentHandler _target;

		private RtaStateGroupLight _stateGroups;
		private RtaAlarmLight _rtaAlarmLight;
		private Dictionary<Guid, List<RtaAlarmLight>> _activityAlarms;

		private Guid _platformTypeId;
		private string _stateCode;
		private ScheduleLayer _scheduleLayer;
		private Guid _businessUnitId;

		private DateTime _dateTime;
		private Guid _guid;
		private Guid _payloadId;
		private Guid _stateGroupId;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_dataHandler = MockRepository.GenerateMock<IActualAgentDataHandler>();
			_target = new ActualAgentHandler(_dataHandler);

			_stateCode = "AUX2";
			_platformTypeId = Guid.NewGuid();
			_businessUnitId = Guid.NewGuid();
			_payloadId = Guid.NewGuid();
			_stateGroupId = Guid.NewGuid();
			_dateTime = DateTime.UtcNow;
			_guid = Guid.NewGuid();

			_scheduleLayer = new ScheduleLayer
				{
					PayloadId = _payloadId
				};
			_stateGroups = new RtaStateGroupLight
				{
					BusinessUnitId = _businessUnitId,
					PlatformTypeId = _platformTypeId,
					StateCode = _stateCode,
					StateGroupId = _stateGroupId
				};
			_rtaAlarmLight = new RtaAlarmLight
				{
					StateGroupId = _stateGroupId,
					Name = "SomeName",
					StateGroupName = "SomeStateGroupName",
					AlarmTypeId = _guid
				};
			_activityAlarms = new Dictionary<Guid, List<RtaAlarmLight>>
				{
					{_payloadId, new List<RtaAlarmLight> {_rtaAlarmLight}}
				};


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
			var resetEvent = new AutoResetEvent(false);

			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, _guid))
			            .Return(new List<ScheduleLayer> {currentLayer, nextLayer});
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(previousState);

			_dataHandler.Expect(s => s.StateGroups()).Return(new List<RtaStateGroupLight>
				{
					new RtaStateGroupLight
						{
							PlatformTypeId = _platformTypeId,
							StateCode = _stateCode,
							BusinessUnitId = _businessUnitId,
							StateGroupId = _stateGroupId
						}
				});

			_dataHandler.Expect(s => s.ActivityAlarms())
			            .Return(_activityAlarms);
			_mock.ReplayAll();

			var result = _target.GetState(_guid, _businessUnitId, _platformTypeId, _stateCode, _dateTime, new TimeSpan());
			Assert.That(result.AlarmName, Is.EqualTo(alarmLight.Name));
			Assert.That(result.StateStart, Is.EqualTo(currentLayer.StartDateTime));
			Assert.That(result.Scheduled, Is.EqualTo(currentLayer.Name));
			Assert.That(result.ScheduledNext, Is.EqualTo(nextLayer.Name));
			_mock.VerifyAll();
			resetEvent.Dispose();
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
			_activityAlarms = new Dictionary<Guid, List<RtaAlarmLight>>
				{
					{_stateGroupId, new List<RtaAlarmLight> {_rtaAlarmLight}}
				};

			var resetEvent = new AutoResetEvent(false);
			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, _guid))
			            .Return(new List<ScheduleLayer> {currentLayer, nextLayer});
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(previousState);
			_dataHandler.Expect(s => s.StateGroups()).Return(new List<RtaStateGroupLight>
				{
					new RtaStateGroupLight
						{
							PlatformTypeId = _platformTypeId,
							StateCode = _stateCode,
							BusinessUnitId = _businessUnitId,
							StateGroupId = _stateGroupId
						}
				});
			_dataHandler.Expect(s => s.ActivityAlarms()).Return(_activityAlarms);
			_mock.ReplayAll();

			var result = _target.GetState(_guid, _businessUnitId, _platformTypeId, _stateCode, _dateTime, new TimeSpan());
			Assert.IsNull(result);
			_mock.VerifyAll();
			resetEvent.Dispose();
		}

		[Test]
		public void ShouldReturnRtaAlarm()
		{
			_dataHandler.Expect(d => d.StateGroups()).Return(new List<RtaStateGroupLight> {_stateGroups});
			_dataHandler.Expect(d => d.ActivityAlarms()).Return(_activityAlarms);

			_mock.ReplayAll();
			var result = _target.GetAlarm(_platformTypeId, _stateCode, _scheduleLayer, _businessUnitId);
			_mock.VerifyAll();
			Assert.That(result, Is.EqualTo(_rtaAlarmLight));
		}

		[Test]
		public void ShouldReturnNullWhenScheduleLayerIsNull()
		{
			_dataHandler.Expect(d => d.StateGroups()).Return(new List<RtaStateGroupLight> {_stateGroups});
			_dataHandler.Expect(d => d.ActivityAlarms()).Return(_activityAlarms);

			_mock.ReplayAll();
			var result = _target.GetAlarm(_platformTypeId, _stateCode, null, _businessUnitId);
			_mock.VerifyAll();
			Assert.That(result, Is.Null);
		}

		[Test]
		public void ShouldReturnNullWhenNoMatchingStateGroup()
		{
			_dataHandler.Expect(d => d.StateGroups()).Return(new List<RtaStateGroupLight> {new RtaStateGroupLight()});
			_dataHandler.Expect(d => d.ActivityAlarms()).Return(_activityAlarms);

			_mock.ReplayAll();
			var result = _target.GetAlarm(_platformTypeId, _stateCode, _scheduleLayer, _businessUnitId);
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

			var resetEvent = new AutoResetEvent(false);

			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, _guid))
			            .Return(new List<ScheduleLayer> {currentLayer, nextLayer});
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(previousState);
			_dataHandler.Expect(s => s.StateGroups()).Return(new List<RtaStateGroupLight>
				{
					new RtaStateGroupLight
						{
							PlatformTypeId = _platformTypeId,
							StateCode = _stateCode,
							BusinessUnitId = _businessUnitId,
							StateGroupId = _stateGroupId
						}
				});

			_dataHandler.Expect(s => s.ActivityAlarms())
			            .Return(_activityAlarms);
			_dataHandler.Expect(s => s.AddOrUpdate(new ActualAgentState())).IgnoreArguments();
			_mock.ReplayAll();

			var result = _target.CheckSchedule(_guid, _businessUnitId, _dateTime);
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

			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, _guid))
			            .Return(new List<ScheduleLayer> {currentLayer, nextLayer});
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(null);
			_dataHandler.Expect(s => s.StateGroups()).Return(new List<RtaStateGroupLight>
				{
					new RtaStateGroupLight()
				});

			_dataHandler.Expect(s => s.ActivityAlarms())
			            .Return(_activityAlarms);
			_dataHandler.Expect(s => s.AddOrUpdate(new ActualAgentState())).IgnoreArguments();
			_mock.ReplayAll();

			var result = _target.CheckSchedule(_guid, _businessUnitId, _dateTime);
			Assert.That(result.AlarmName, Is.EqualTo(string.Empty));
			Assert.That(result.StateStart, Is.EqualTo(new DateTime(1900, 01 ,01, 00, 00, 00, 000, DateTimeKind.Utc)));
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

			_dataHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, _guid))
			            .Return(new List<ScheduleLayer> {currentLayer, new ScheduleLayer()});
			_dataHandler.Expect(s => s.LoadOldState(_guid)).Return(
				new ActualAgentState
					{
						ScheduledId = _payloadId,
						StateStart = new DateTime(2013, 02, 21, 00, 00, 00, DateTimeKind.Utc),
						NextStart = new DateTime(2013, 02, 21, 12, 00, 00, DateTimeKind.Utc)
					});
			_mock.ReplayAll();

			var result = _target.CheckSchedule(_guid, _businessUnitId, _dateTime);
			Assert.IsNull(result);
			_mock.VerifyAll();
			resetEvent.Dispose();
		}
	}
}
