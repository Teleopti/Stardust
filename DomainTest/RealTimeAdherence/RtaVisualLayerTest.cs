using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
    [TestFixture]
    public class RtaVisualLayerTest
    {
        private MockRepository mocks;
        private IRtaStateGroup payload;
        private DateTimePeriod period;
        private RtaVisualLayer target;
        private IRtaState state;
        private IActivity activity;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            payload = mocks.StrictMock<IRtaStateGroup>();
            state = mocks.StrictMock<IRtaState>();
            activity = mocks.StrictMock<IActivity>();
            period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 11, 17, 0, 0, 0, DateTimeKind.Utc), 0);

            Expect.Call(state.StateGroup).Return(payload);
            mocks.ReplayAll();
            target = new RtaVisualLayer(state,period,activity);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(period,target.Period);
            Assert.AreEqual(payload,target.Payload);
            Assert.AreEqual(activity, target.HighestPriorityActivity);
            Assert.IsInstanceOf<IVisualLayer>(target);
        }

        [Test]
        public void VerifyCanSetLogOff()
        {
            Assert.IsFalse(target.IsLoggedOut);
            target.IsLoggedOut = true;
            Assert.IsTrue(target.IsLoggedOut);
        }

        [TearDown]
        public void Teardown()
        {
            mocks.VerifyAll();
        }
    }
}
