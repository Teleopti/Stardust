﻿using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.SystemCheck;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck
{
    public class CheckMessageBrokerTest
    {
        private CheckMessageBroker target;
        private MockRepository mocks;
        private IMessageBrokerComposite mb;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            mb = mocks.StrictMock<IMessageBrokerComposite>();
            target = new CheckMessageBroker(mb);
        }

        [Test]
        public void VerifyWarningText()
        {
            Assert.AreEqual(UserTexts.Resources.CheckMessageBrokerWarning, target.WarningText);
        }

        [Test]
        public void VerifyMessageBrokerWorks()
        {
            using(mocks.Record())
            {
                Expect.Call(mb.ServerUrl).Return("Any");
                Expect.Call(mb.IsAlive)
                    .Return(true);
            }
            using(mocks.Playback())
            {
                Assert.IsTrue(target.IsRunningOk());                
            }
        }

        [Test]
        public void VerifyNoMessageBrokerWorks()
        {
            using (mocks.Record())
            {
                Expect.Call(mb.ServerUrl).Return(null);
            }
            using (mocks.Playback())
            {
                Assert.IsTrue(target.IsRunningOk());
            }
        }

        [Test]
        public void VerifyMessageBrokerDown()
        {
            using (mocks.Record())
            {
                Expect.Call(mb.ServerUrl).Return("Any");
                Expect.Call(mb.IsAlive)
                    .Return(false);
            }
            using (mocks.Playback())
            {
                Assert.IsFalse(target.IsRunningOk());
            }
        }
    }
}
