using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class PayloadTimeTest
    {
        private Activity _payload;
        private TimeSpan _time;
        private PayloadTime<Activity> _payloadTime;

        [SetUp]
        public void Setup()
        {
            _payload = ActivityFactory.CreateActivity("phone");
            _time= new TimeSpan(0,44,0);
            _payloadTime = new PayloadTime<Activity>(_payload,_time);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_payload,_payloadTime.Payload);
            Assert.AreEqual(_time, _payloadTime.Time);
        }

        [Test]
        public void VerifyProtectedConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_payloadTime.GetType()));
        }

    }
}
