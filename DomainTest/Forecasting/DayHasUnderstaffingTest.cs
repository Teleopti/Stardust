using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
	public class DayHasUnderstaffingTest
    {
        private DayHasUnderstaffing target;
        private ISkill skill;
        private StaffingThresholds staffingThresholds;
        
        [SetUp]
        public void Setup()
        {
            staffingThresholds = new StaffingThresholds(new Percent(), new Percent(-0.2), new Percent());
            skill = SkillFactory.CreateSkill("skillName");
            skill.StaffingThresholds = staffingThresholds;
            target = new DayHasUnderstaffing(skill);
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnTrue()
        {
            Assert.IsTrue(target.IsSatisfiedBy(new List<IValidatePeriod>{ new SkillStaffPeriodFake(-0.21) }));
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnFalse()
        {
            skill.StaffingThresholds = staffingThresholds;
            Assert.IsFalse(target.IsSatisfiedBy(new List<IValidatePeriod>{ new SkillStaffPeriodFake(-0.19) }));
        }

        [Test]
        public void VerifyIsSatisfiedByCanHandleNoValue()
        {   
            skill.StaffingThresholds =  new StaffingThresholds(new Percent(), new Percent(), new Percent());
            Assert.IsTrue(target.IsSatisfiedBy(new List<IValidatePeriod>{ new SkillStaffPeriodFake(-0.21) }));
        }
    }
}
