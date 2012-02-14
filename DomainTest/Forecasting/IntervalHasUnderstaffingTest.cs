using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class IntervalHasUnderstaffingTest
    {
        private IntervalHasUnderstaffing target;
        private ISkill skill;
        private MockRepository mocks;
        private StaffingThresholds staffingThresholds;
        private ISkillStaffPeriod skillStaffPeriodMocked;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            skill = mocks.StrictMock<ISkill>();
            staffingThresholds = new StaffingThresholds(new Percent(), new Percent(-0.2), new Percent());
            target = new IntervalHasUnderstaffing(skill);
            skillStaffPeriodMocked = mocks.StrictMock<ISkillStaffPeriod>();
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnTrue()
        {
            Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(-0.21);
            Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();
            mocks.ReplayAll();
            Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriodMocked));
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnFalse()
        {
            Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(-0.19);
            Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();
            mocks.ReplayAll();
            Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriodMocked));
        }

		[Test]
		public void ShouldConsiderActualValueOfThresholdNotTrue()
		{
			Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(-0.2);
			Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();
			mocks.ReplayAll();
			Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriodMocked));
		}

        [Test]
        public void VerifyIsSatisfiedByCanHandleNoValue()
        {
            staffingThresholds = new StaffingThresholds(new Percent(), new Percent(), new Percent());
            Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(-0.21);
            Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();
            mocks.ReplayAll();
            Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriodMocked));
        }
    }
}
