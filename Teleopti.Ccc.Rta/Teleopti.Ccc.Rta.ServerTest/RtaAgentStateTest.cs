using System;
using NUnit.Framework;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
    [TestFixture]
    public class RtaAgentStateTest
    {
        private RtaAgentState target;
        private DateTime _date;

        [SetUp]
        public void Setup()
        {
            _date = DateTime.UtcNow;
            target = new RtaAgentState("45","67",TimeSpan.Zero,_date,Guid.Empty);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("45",target.ExternalLogOn);
            Assert.AreEqual("67",target.StateCode);
            Assert.AreEqual(_date,target.Timestamp);
            Assert.AreEqual(Guid.Empty,target.PlatformTypeId);
            Assert.AreEqual(TimeSpan.Zero,target.TimeInState);

        }
    }
}
