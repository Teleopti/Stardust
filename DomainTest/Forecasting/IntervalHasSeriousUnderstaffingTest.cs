using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
	public class IntervalHasSeriousUnderstaffingTest
    {
        private IntervalHasSeriousUnderstaffing target;
        private ISkill skill;
        private MockRepository mocks;
        private StaffingThresholds staffingThresholds;
        private ISkillStaffPeriod skillStaffPeriodMocked;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            skill = mocks.StrictMock<ISkill>();
            staffingThresholds = new StaffingThresholds(new Percent(-0.1), new Percent(), new Percent());
            target = new IntervalHasSeriousUnderstaffing(skill);
            skillStaffPeriodMocked = mocks.StrictMock<ISkillStaffPeriod>();
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnTrue()
        {
            Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(-0.11);
            Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();
            mocks.ReplayAll();
            Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriodMocked));
        }

		[Test]
		public void ShouldConsiderActualValueOfThresholdNotTrue()
		{
			Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(-0.10);
			Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();
			mocks.ReplayAll();
			Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriodMocked));
		}

        [Test]
        public void VerifyIsSatisfiedByCanReturnFalse()
        {
            Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(-0.09);
            Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();
            mocks.ReplayAll();
            Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriodMocked));
        }

        [Test]
        public void VerifyIsSatisfiedByCanHandleNoValue()
        {
            Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(-0.11);
            staffingThresholds = new StaffingThresholds(new Percent(), new Percent(), new Percent());
            Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();
            mocks.ReplayAll();
            Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriodMocked));
        }
    }
}
