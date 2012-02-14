using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class AgentStateViewAdapterTest
    {
        private AgentStateViewAdapter target;
        private MockRepository mocks;
        private IRtaStateHolder rtaStateHolder;
        private IRtaStateGroup rtaStateGroup;


        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();

            rtaStateHolder = mocks.StrictMock<IRtaStateHolder>();
            rtaStateGroup = mocks.StrictMock<IRtaStateGroup>();

            target = new AgentStateViewAdapter(rtaStateHolder,rtaStateGroup  );
        }

        [Test]
        public void VerifyProperties()
        {
            mocks.ReplayAll();

            Assert.IsNotNull(target);
            Assert.AreEqual(0,target.TotalPersons);
            Assert.AreEqual(rtaStateGroup , target.StateGroup );
            mocks.VerifyAll();
        }
       


        [Test]
        public void VerifyCanRefreshAgentState()
        {
            DateTime now = DateTime.UtcNow;
            var agentStates = new Dictionary<IPerson, IAgentState>();
            var person = mocks.StrictMock<IPerson>();
            var agentState = mocks.StrictMock<IAgentState>();
            var visuallayer = mocks.StrictMock<IRtaVisualLayer >();
            agentStates[person] = agentState;
            Expect.Call(agentState.FindCurrentState(now)).Return(visuallayer);
            Expect.Call(rtaStateHolder.AgentStates).Return(agentStates );
            Expect.Call(visuallayer.Payload).Return(rtaStateGroup);
            mocks.ReplayAll();
           
            var updatedProperties = new List<string>();
            target.PropertyChanged += (sender, e) => updatedProperties.Add(e.PropertyName);
            target.Refresh(now);
            mocks.VerifyAll();
            Assert.AreEqual(1,target.TotalPersons );
        }
    }
}
