using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class IntervalHasBelowMinAgentsTest
    {
        private IntervalHasBelowMinAgents target;
        private ISkillStaffPeriod skillStaffPeriod;

        [SetUp]
        public void Setup()
        {
            target = new IntervalHasBelowMinAgents();

            skillStaffPeriod =SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 9, 16, 0, 0, 0, DateTimeKind.Utc), 0),
                new Task(), ServiceAgreement.DefaultValues());
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnTrue()
        {
            skillStaffPeriod.Payload.CalculatedLoggedOn = 2;
            skillStaffPeriod.Payload.SkillPersonData = new SkillPersonData(3,0);
            Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriod));
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnFalse()
        {
            skillStaffPeriod.Payload.CalculatedLoggedOn = 3;
            skillStaffPeriod.Payload.SkillPersonData = new SkillPersonData(3, 0);
            Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriod));

            skillStaffPeriod.Payload.CalculatedLoggedOn = 4;
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
