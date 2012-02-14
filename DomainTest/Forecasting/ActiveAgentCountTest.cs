using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class ActiveAgentCountTest
    {
        private ActiveAgentCount target;

        [SetUp]
        public void Setup()
        {
            target = new ActiveAgentCount();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(new DateTime(),target.Interval);
            Assert.AreEqual(0, target.ActiveAgents);

            DateTime interval = new DateTime(2008,9,19,0,0,0,DateTimeKind.Utc);
            target.Interval = interval;
            target.ActiveAgents = 6;

            Assert.AreEqual(interval, target.Interval);
            Assert.AreEqual(6,target.ActiveAgents);
        }
    }
}
