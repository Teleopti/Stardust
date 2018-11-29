using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class SkillStaffSegmentPeriodTest
    {
        private ISkillStaffSegment segment;
        private DateTimePeriod period;
        private ISkillStaffSegmentPeriod target;
        private ISkillStaffPeriod ssPeriod;

        [SetUp]
        public void Setup()
        {
            ssPeriod =SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                                     new DateTimePeriod(2000, 1, 1, 2001, 1, 1),
                                     new Task(3, new TimeSpan(3), new TimeSpan(4)),
                                     new ServiceAgreement(new ServiceLevel(new Percent(0.6), 3), new Percent(0.4),
                                                          new Percent(0.2)));
            segment = new SkillStaffSegment(4);
            period = new DateTimePeriod(2000, 1, 1, 2001, 1, 1);
            target = new SkillStaffSegmentPeriod(ssPeriod, ssPeriod, segment, period);

        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(period, target.Period);
            Assert.AreSame(segment, target.Payload);
            Assert.IsNotNull(target.ToString());
        }

        [Test]
        public void VerifyParent()
        {
            Assert.AreSame(ssPeriod, target.BelongsTo);
        }
    }
}
