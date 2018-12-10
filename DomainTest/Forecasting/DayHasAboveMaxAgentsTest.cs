using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class DayHasAboveMaxAgentsTest
    {
        private DayHasAboveMaxAgents target;
        private IList<ISkillStaffPeriod> skillStaffPeriods;

        [SetUp]
        public void Setup()
        {
            target = new DayHasAboveMaxAgents();

            DateTimePeriod period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 9, 16, 0, 0, 0, DateTimeKind.Utc), 0);
            ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period, new Task(), ServiceAgreement.DefaultValues());
            skillStaffPeriods = new List<ISkillStaffPeriod>{skillStaffPeriod};
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnTrue()
        {
            skillStaffPeriods[0].Payload.CalculatedLoggedOn = 12;
            skillStaffPeriods[0].Payload.SkillPersonData = new SkillPersonData(0, 10);
            Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriods));
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnFalse()
        {
            skillStaffPeriods[0].Payload.CalculatedLoggedOn = 9;
            skillStaffPeriods[0].Payload.SkillPersonData = new SkillPersonData(0, 9);
            Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriods));

            skillStaffPeriods[0].Payload.CalculatedLoggedOn = 8;
            Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriods));
        }

        [Test]
        public void VerifyIsSatisfiedByCanHandleNoValue()
        {
            skillStaffPeriods[0].Payload.CalculatedLoggedOn = 2;
            skillStaffPeriods[0].Payload.SkillPersonData = new SkillPersonData();
            Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriods));
        }
    }
}
