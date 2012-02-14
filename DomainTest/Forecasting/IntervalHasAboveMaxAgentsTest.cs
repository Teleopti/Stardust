using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class IntervalHasAboveMaxAgentsTest
    {
        private IntervalHasAboveMaxAgents target;
        private ISkillStaffPeriod skillStaffPeriod;

        [SetUp]
        public void Setup()
        {
            target = new IntervalHasAboveMaxAgents();

            skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 9, 16, 0, 0, 0, DateTimeKind.Utc), 0),
                new Task(), ServiceAgreement.DefaultValues());
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnTrue()
        {
            skillStaffPeriod.Payload.CalculatedLoggedOn = 12;
            skillStaffPeriod.Payload.SkillPersonData = new SkillPersonData(0,10);
            Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriod));
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnFalse()
        {
            skillStaffPeriod.Payload.CalculatedLoggedOn = 9;
            skillStaffPeriod.Payload.SkillPersonData = new SkillPersonData(0, 9);
            Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriod));

            skillStaffPeriod.Payload.CalculatedLoggedOn = 8;
            Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriod));
        }

        [Test]
        public void VerifyIsSatisfiedByCanHandleNoValue()
        {
            skillStaffPeriod.Payload.CalculatedLoggedOn = 2;
            skillStaffPeriod.Payload.SkillPersonData = new SkillPersonData();
            Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriod));
        }
    }
}
