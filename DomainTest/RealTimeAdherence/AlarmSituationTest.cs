using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
    [TestFixture]
    public class AlarmSituationTest
    {
        private MockRepository mocks;
        private IAlarmType payload;
        private DateTimePeriod period;
        private AlarmSituation target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            payload = mocks.StrictMock<IAlarmType>();
            period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 11, 17, 0, 0, 0, DateTimeKind.Utc), 0);
            target = new AlarmSituation(payload, period, PersonFactory.CreatePerson());
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(period, target.Period);
            Assert.AreEqual(payload, target.Payload);
            Assert.AreEqual("Dummy", target.HighestPriorityActivity.Description.Name);
            Assert.IsInstanceOf<IVisualLayer>(target);
        }
    }
}
