using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
	public class IntervalHasUnderstaffingTest
    {
        private IntervalHasUnderstaffing target;
        private readonly Skill skill = new Skill("skill");

        [SetUp]
        public void Setup()
        {
			skill.StaffingThresholds = new StaffingThresholds(new Percent(), new Percent(-0.2), new Percent());
            target = new IntervalHasUnderstaffing(skill);
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnTrue()
        {
            Assert.IsTrue(target.IsSatisfiedBy(new SkillStaffPeriodFake(-0.21)));
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnFalse()
        {
			Assert.IsFalse(target.IsSatisfiedBy(new SkillStaffPeriodFake(-0.19)));
        }

		[Test]
		public void ShouldConsiderActualValueOfThresholdNotTrue()
		{
			Assert.IsFalse(target.IsSatisfiedBy(new SkillStaffPeriodFake(-0.2)));
		}

        [Test]
        public void VerifyIsSatisfiedByCanHandleNoValue()
        {
			skill.StaffingThresholds = new StaffingThresholds(new Percent(), new Percent(), new Percent());
			Assert.IsTrue(target.IsSatisfiedBy(new SkillStaffPeriodFake(-0.21)));
        }
    }
}
