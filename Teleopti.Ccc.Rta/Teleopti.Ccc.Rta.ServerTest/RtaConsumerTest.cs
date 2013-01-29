using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class RtaConsumerTest
	{
		private MockRepository _mock;
		private RtaConsumer _target;
		private IActualAgentHandler _agentHandler;
		private IActualAgentStateDataHandler _stateHandler;

		private Guid _guid;
		private DateTime _dateTime;
		private TimeSpan _timeSpan;
		private string _stateCode;
		
		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_agentHandler = _mock.StrictMock<IActualAgentHandler>();
			_stateHandler = _mock.StrictMock<IActualAgentStateDataHandler>();

			_target = new RtaConsumer(_stateHandler, _agentHandler);
			_guid = Guid.NewGuid();
			_dateTime = DateTime.UtcNow;
			_timeSpan = TimeSpan.FromMinutes(15);
			_stateCode = "AUX5";
		}

		[Test]
		public void VerifyConsume()
		{
			var currentLayer = new ScheduleLayer
			                   	{
			                   		Name = "CurrentLayer",
									StartDateTime = _dateTime
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
			                 		Name = "AlarmName",
									AlarmTypeId = _guid
			                 	};
			var resetEvent = new AutoResetEvent(false);
			
			_stateHandler.Expect(s => s.CurrentLayerAndNext(_dateTime, _guid)).Return(new List<ScheduleLayer>
			                                                                          	{currentLayer, nextLayer});
			_stateHandler.Expect(s => s.LoadOldState(_guid)).Return(previousState);
			_agentHandler.Expect(a => a.GetAlarm(_guid, _stateCode, currentLayer, _guid)).Return(alarmLight);
			_stateHandler.Expect(s => s.AddOrUpdate(new ActualAgentState())).IgnoreArguments();
			_mock.ReplayAll();

			var result = _target.Consume(_guid, _guid, _guid, _stateCode, _dateTime, _timeSpan, resetEvent);
			Assert.That(result.AlarmName, Is.EqualTo(alarmLight.Name));
			Assert.That(result.StateStart, Is.EqualTo(currentLayer.StartDateTime));
			Assert.That(result.Scheduled, Is.EqualTo(currentLayer.Name));
			Assert.That(result.ScheduledNext, Is.EqualTo(nextLayer.Name));
			_mock.VerifyAll();
			resetEvent.Dispose();
		}
	}
}
