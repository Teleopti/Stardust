using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class IntervalHasOverstaffingTest
    {
        private IntervalHasOverstaffing target;
        private ISkill skill;
        private MockRepository mocks;
        private StaffingThresholds staffingThresholds;
        private ISkillStaffPeriod skillStaffPeriodMocked;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            skill = mocks.StrictMock<ISkill>();
            staffingThresholds = new StaffingThresholds(new Percent(), new Percent(), new Percent(0.3));
            target = new IntervalHasOverstaffing(skill);
            skillStaffPeriodMocked = mocks.StrictMock<ISkillStaffPeriod>();
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnTrue()
        {
            Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(0.31);
            Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();
            mocks.ReplayAll();
            Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriodMocked));
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnFalse()
        {
            Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(0.29);
            Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();
            mocks.ReplayAll();
            Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriodMocked));
        }

		[Test]
		public void ShouldConsiderActualValueOfThresholdNotTrue()
		{
			Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(0.3);
			Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();
			mocks.ReplayAll();
			Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriodMocked));
		}

        [Test]
        public void VerifyIsSatisfiedByCanHandleNoValue()
        {
            staffingThresholds = new StaffingThresholds(new Percent(), new Percent(), new Percent());
            Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(0.31);
            Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();
            mocks.ReplayAll();
            Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriodMocked));
        }
    }
}
