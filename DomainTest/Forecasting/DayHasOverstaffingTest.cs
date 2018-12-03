using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class DayHasOverstaffingTest
    {
        private DayHasOverstaffing target;
        private ISkill skill;
        private StaffingThresholds staffingThresholds;
        private IList<ISkillStaffPeriod> skillStaffPeriods;
        private ISkillStaffPeriod skillStaffPeriodMocked;
        private MockRepository mocks;


        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            staffingThresholds = new StaffingThresholds(new Percent(), new Percent(), new Percent(0.3));
            skill = SkillFactory.CreateSkill("skillName");
            skill.StaffingThresholds = staffingThresholds;
            target = new DayHasOverstaffing(skill);
            skillStaffPeriodMocked = mocks.StrictMock<ISkillStaffPeriod>();
            skillStaffPeriods = new List<ISkillStaffPeriod> { skillStaffPeriodMocked };
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnTrue()
        {
            //skillStaffPeriods[0].RelativeDifference = 0.31;
            Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(0.31);
            mocks.ReplayAll();
            Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriods));
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnFalse()
        {
            //skillStaffPeriods[0].RelativeDifference = 0.29;
            Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(0.29);
            mocks.ReplayAll();
            skill.StaffingThresholds = staffingThresholds;
            Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriods));
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyIsSatisfiedByCanHandleNoValue()
        {
            //skillStaffPeriods[0].RelativeDifference = 0.31;
            Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(0.31);
            mocks.ReplayAll();
            skill.StaffingThresholds =  new StaffingThresholds(new Percent(), new Percent(), new Percent());
            Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriods));
            mocks.VerifyAll();
        }
    }
}
