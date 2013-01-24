using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class ActualAgentHandlerTest
	{
		private MockRepository _mock;
		private IActualAgentStateDataHandler _dataHandler;
		private IActualAgentHandler _target;

		private RtaStateGroupLight _stateGroups;
		private RtaAlarmLight _rtaAlarmLight;
		private Dictionary<Guid, List<RtaAlarmLight>> _activityAlarms;

		private Guid _platformTypeId;
		private string _stateCode;
		private ScheduleLayer _scheduleLayer;
		private Guid _businessUnitId;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_dataHandler = _mock.StrictMock<IActualAgentStateDataHandler>();
			_target = new ActualAgentHandler(_dataHandler);

			_stateCode = "AUX2";
			_platformTypeId = Guid.NewGuid();
			_businessUnitId = Guid.NewGuid();
			var payloadId = Guid.NewGuid();
			var stateGroupId = Guid.NewGuid();

			_scheduleLayer = new ScheduleLayer
			                 	{
			                 		PayloadId = payloadId
			                 	};
			_stateGroups = new RtaStateGroupLight
			                      	{
			                      		BusinessUnitId = _businessUnitId,
			                      		PlatformTypeId = _platformTypeId,
			                      		StateCode = _stateCode,
			                      		StateGroupId = stateGroupId
			                      	};
			_rtaAlarmLight = new RtaAlarmLight
			                 	{
			                 		StateGroupId = stateGroupId,
			                 		Name = "SomeName",
			                 		StateGroupName = "SomeStateGroupName"
			                 	};
			_activityAlarms = new Dictionary<Guid, List<RtaAlarmLight>>
			              	{
			              		{payloadId, new List<RtaAlarmLight> {_rtaAlarmLight}}
			              	};
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
	}
}
