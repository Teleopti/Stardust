using System;
using System.Collections.Generic;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Intraday;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class StaffingEffectViewAdapterTest
    {
        private StaffingEffectViewAdapter _target;
        private MockRepository mocks;
        private IRtaStateHolder rtaStateHolder;
        private IPerson person;
        private DateTimePeriod period;
        private ITeam team;
        private IDayLayerViewModel dayLayerViewAdapter;
        private DayLayerModel model;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();

            rtaStateHolder = mocks.StrictMock<IRtaStateHolder>();
            person = mocks.StrictMock<IPerson>();
            period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 8, 0, 0, 0, DateTimeKind.Utc), 0);
            team = mocks.StrictMock<ITeam>();
            
            model = new DayLayerModel(person, period, team,
                                          new WinCode.Common.LayerViewModelCollection(), null);
            dayLayerViewAdapter = new DayLayerViewModel(rtaStateHolder, null, null,null,new TestDispatcher());
            dayLayerViewAdapter.Models.Add(model);

            _target = new StaffingEffectViewAdapter(dayLayerViewAdapter);
        }

        [Test]
        public void TestProperties()
        {
            Assert.AreEqual(0,_target.PositiveEffect);
            Assert.AreEqual(0, _target.NegativeEffect);
            Assert.AreEqual(0,_target.NegativeEffectPercent );
            Assert.AreEqual(0,_target.PositiveEffectPercent );
            Assert.AreEqual(0,_target.Total );
            Assert.AreEqual(0,_target.TotalPercent );
        }

        [Test]
        public void VerifyCanRefreshPositiveStaffingValuesAreUpdated()
        {
            DateTime now = DateTime.UtcNow;

            IAgentState agentState = mocks.StrictMock<IAgentState>();
            IVisualLayer alarmLayer = mocks.StrictMock<IVisualLayer>();
            IRtaVisualLayer stateLayer = mocks.StrictMock<IRtaVisualLayer>();

            IAlarmType payload = mocks.StrictMock<IAlarmType>();
            IDictionary<IPerson, IAgentState> agentStates = new Dictionary<IPerson, IAgentState>();
            agentStates.Add(person, agentState);

            Expect.Call(rtaStateHolder.AgentStates).Return(agentStates);
            Expect.Call(agentState.FindCurrentAlarm(now)).Return(alarmLayer);
            Expect.Call(agentState.FindCurrentState(now)).Return(stateLayer);
            Expect.Call(agentState.FindCurrentSchedule(now)).Return(null);
            Expect.Call(agentState.FindNextSchedule(now)).Return(null);
            Expect.Call(alarmLayer.Payload).Return(payload).Repeat.Any();
            Expect.Call(payload.StaffingEffect).Return(1).Repeat.Any();

            mocks.ReplayAll();
            var updatedProperties = new List<string>();
            _target.PropertyChanged += (sender, e) => updatedProperties.Add(e.PropertyName);
            dayLayerViewAdapter.Refresh(now);

            Assert.IsTrue(updatedProperties.Contains("PositiveEffect"));
            Assert.IsTrue(updatedProperties.Contains("Total"));
            Assert.IsTrue(updatedProperties.Contains("TotalPercent"));
            Assert.IsTrue(updatedProperties.Contains("PositiveEffectPercent"));

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyPropertyChangedWhenAdapterIsRemovedFromCollection()
        {
            Assert.AreEqual(1,model.HookedEvents());
            dayLayerViewAdapter.Models.Remove(model);
            Assert.AreEqual(0,model.HookedEvents());   
            dayLayerViewAdapter.Models.Add(model);
            Assert.AreEqual(1, model.HookedEvents());
        }

        [Test]
        public void VerifyCanRefreshNegativeStaffingValuesAreUpdated()
        {
            DateTime now = DateTime.UtcNow;

            IAgentState agentState = mocks.StrictMock<IAgentState>();
            IVisualLayer alarmLayer = mocks.StrictMock<IVisualLayer>();
            IRtaVisualLayer stateLayer = mocks.StrictMock<IRtaVisualLayer>();

            IAlarmType payload = mocks.StrictMock<IAlarmType>();
            IDictionary<IPerson, IAgentState> agentStates = new Dictionary<IPerson, IAgentState>();
            agentStates.Add(person, agentState);

            Expect.Call(rtaStateHolder.AgentStates).Return(agentStates);
            Expect.Call(agentState.FindCurrentAlarm(now)).Return(alarmLayer);
            Expect.Call(agentState.FindCurrentState(now)).Return(stateLayer);
            Expect.Call(agentState.FindCurrentSchedule(now)).Return(null);
            Expect.Call(agentState.FindNextSchedule(now)).Return(null);
            Expect.Call(alarmLayer.Payload).Return(payload).Repeat.Any();
            Expect.Call(payload.StaffingEffect).Return(-1).Repeat.Any();

            mocks.ReplayAll();
            var updatedProperties = new List<string>();
            _target.PropertyChanged += (sender, e) => updatedProperties.Add(e.PropertyName);
            dayLayerViewAdapter.Refresh(now);

            Assert.IsTrue(updatedProperties.Contains("NegativeEffect"));
            Assert.IsTrue(updatedProperties.Contains("Total"));
            Assert.IsTrue(updatedProperties.Contains("TotalPercent"));
            Assert.IsTrue(updatedProperties.Contains("NegativeEffectPercent"));

            mocks.VerifyAll();
        }
    }
}
