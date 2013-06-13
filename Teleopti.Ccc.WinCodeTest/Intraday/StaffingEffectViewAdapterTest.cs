using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class StaffingEffectViewAdapterTest
    {
        private StaffingEffectViewAdapter _target;
        private MockRepository _mocks;
        private IRtaStateHolder _rtaStateHolder;
        private IPerson _person;
        private DateTimePeriod _period;
        private ITeam _team;
        private IDayLayerViewModel _dayLayerViewAdapter;
        private DayLayerModel _model;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _rtaStateHolder = _mocks.DynamicMock<IRtaStateHolder>();
            _person = _mocks.DynamicMock<IPerson>();
            _period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 8, 0, 0, 0, DateTimeKind.Utc), 0);
            _team = _mocks.DynamicMock<ITeam>();

            _model = new DayLayerModel(_person, _period, _team,
										  new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null), null);
            _dayLayerViewAdapter = new DayLayerViewModel(_rtaStateHolder, null, null, null, new TestDispatcher());
            _dayLayerViewAdapter.Models.Add(_model);

            _target = new StaffingEffectViewAdapter(_dayLayerViewAdapter);
        }

        [Test]
        public void TestProperties()
        {
            Assert.AreEqual(0, _target.PositiveEffect);
            Assert.AreEqual(0, _target.NegativeEffect);
            Assert.AreEqual(0, _target.NegativeEffectPercent);
            Assert.AreEqual(0, _target.PositiveEffectPercent);
            Assert.AreEqual(0, _target.Total);
            Assert.AreEqual(0, _target.TotalPercent);
			
			_target.NegativeEffect = 1;
        	_target.Total = 1;
        	_target.PositiveEffect = 1;
        }

		[Test]
		public void VerifyCalculateEffects()
		{
			_model.StaffingEffect = 10;
			var target = new StaffingEffectViewAdapterForTest(_dayLayerViewAdapter);
			target.CallPropertyChanged(this, new PropertyChangedEventArgs("AlarmDescription"));
			_model.StaffingEffect = -10;
			target = new StaffingEffectViewAdapterForTest(_dayLayerViewAdapter);
			target.CallPropertyChanged(this, new PropertyChangedEventArgs("AlarmDescription"));
		}
		
		[Test]
		public void VerifyPropertyChangedWhenAdapterIsRemovedFromCollection()
		{
			Assert.AreEqual(1, _model.HookedEvents());
			_dayLayerViewAdapter.Models.Remove(_model);
			Assert.AreEqual(0, _model.HookedEvents());
			_dayLayerViewAdapter.Models.Add(_model);
			Assert.AreEqual(1, _model.HookedEvents());
		}

		//[Test]
		//public void ShouldTriggerEvent()
		//{
		//    var total = _target.Total;
		//    _dayLayerViewAdapter.Models.FirstOrDefault().StaffingEffect = 10;
		//    _dayLayerViewAdapter.Models.FirstOrDefault().AlarmDescription = "NewAlarm";
		//    Assert.That(_target.Total > total);
		//}

        //[Test]
        //public void VerifyCanRefreshPositiveStaffingValuesAreUpdated()
        //{
        //    DateTime now = DateTime.UtcNow;

        //    IAgentState agentState = mocks.StrictMock<IAgentState>();
        //    IVisualLayer alarmLayer = mocks.StrictMock<IVisualLayer>();
        //    IRtaVisualLayer stateLayer = mocks.StrictMock<IRtaVisualLayer>();

        //    IAlarmType payload = mocks.StrictMock<IAlarmType>();
        //    IDictionary<IPerson, IAgentState> agentStates = new Dictionary<IPerson, IAgentState>();
        //    agentStates.Add(person, agentState);

        //    Expect.Call(rtaStateHolder.AgentStates).Return(agentStates);
        //    Expect.Call(agentState.FindCurrentAlarm(now)).Return(alarmLayer);
        //    Expect.Call(agentState.FindCurrentState(now)).Return(stateLayer);
        //    Expect.Call(agentState.FindCurrentSchedule(now)).Return(null);
        //    Expect.Call(agentState.FindNextSchedule(now)).Return(null);
        //    Expect.Call(alarmLayer.Payload).Return(payload).Repeat.Any();
        //    Expect.Call(payload.StaffingEffect).Return(1).Repeat.Any();

        //    mocks.ReplayAll();
        //    var updatedProperties = new List<string>();
        //    _target.PropertyChanged += (sender, e) => updatedProperties.Add(e.PropertyName);
        //    dayLayerViewAdapter.Refresh(now);

        //    Assert.IsTrue(updatedProperties.Contains("PositiveEffect"));
        //    Assert.IsTrue(updatedProperties.Contains("Total"));
        //    Assert.IsTrue(updatedProperties.Contains("TotalPercent"));
        //    Assert.IsTrue(updatedProperties.Contains("PositiveEffectPercent"));

        //    mocks.VerifyAll();
        //}


        //[Test]
        //public void VerifyCanRefreshNegativeStaffingValuesAreUpdated()
        //{
        //    //IVisualLayer alarmLayer = mocks.StrictMock<IVisualLayer>();
        //    //IRtaVisualLayer stateLayer = mocks.StrictMock<IRtaVisualLayer>();
        //    //IAlarmType payload = mocks.StrictMock<IAlarmType>();
        //    //Expect.Call(rtaStateHolder.AgentStates).Return(agentStates);
        //    //Expect.Call(agentState.FindCurrentAlarm(now)).Return(alarmLayer);
        //    //Expect.Call(agentState.FindCurrentState(now)).Return(stateLayer);
        //    //Expect.Call(agentState.FindCurrentSchedule(now)).Return(null);
        //    //Expect.Call(agentState.FindNextSchedule(now)).Return(null);
        //    //Expect.Call(alarmLayer.Payload).Return(payload).Repeat.Any();
        //    //Expect.Call(payload.StaffingEffect).Return(-1).Repeat.Any();

        //    var now = DateTime.UtcNow;
        //    var agentState = mocks.StrictMock<IAgentState>();
        //    IDictionary<IPerson, IAgentState> agentStates = new Dictionary<IPerson, IAgentState>();
        //    agentStates.Add(person, agentState);
        //    var actualAgentState = mocks.DynamicMock<IActualAgentState>();
        //    var actualAgentStates = new Dictionary<IPerson, IActualAgentState> {{person, actualAgentState}};

        //    Expect.Call(rtaStateHolder.ActualAgentStates).Return(actualAgentStates);

        //    mocks.ReplayAll();
        //    var updatedProperties = new List<string>();
        //    _target.PropertyChanged += (sender, e) => updatedProperties.Add(e.PropertyName);
        //    dayLayerViewAdapter.Refresh(now);

        //    Assert.IsTrue(updatedProperties.Contains("NegativeEffect"));
        //    Assert.IsTrue(updatedProperties.Contains("Total"));
        //    Assert.IsTrue(updatedProperties.Contains("TotalPercent"));
        //    Assert.IsTrue(updatedProperties.Contains("NegativeEffectPercent"));

        //    mocks.VerifyAll();
        //}
    }

	public class StaffingEffectViewAdapterForTest : StaffingEffectViewAdapter
	{
		public StaffingEffectViewAdapterForTest(IDayLayerViewModel dayLayerViewModel)
			: base(dayLayerViewModel)
		{	
		}

		public void CallPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			AdapterPropertyChanged(sender, e);
		}

	}
}
