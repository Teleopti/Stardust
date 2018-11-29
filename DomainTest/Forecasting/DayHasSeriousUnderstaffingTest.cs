using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
	[TestFixture]
	public class DayHasSeriousUnderstaffingTest
	{
		private DayHasSeriousUnderstaffing target;
		private ISkill skill;
		private StaffingThresholds staffingThresholds;
		private IList<ISkillStaffPeriod> skillStaffPeriods;
		private MockRepository mocks;
		private ISkillStaffPeriod skillStaffPeriodMocked;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			skill = mocks.StrictMock<ISkill>();

			staffingThresholds = new StaffingThresholds(new Percent(-0.1), new Percent(), new Percent());
			Expect.Call(skill.StaffingThresholds).Return(staffingThresholds).Repeat.AtLeastOnce();

			target = new DayHasSeriousUnderstaffing(skill);

			skillStaffPeriodMocked = mocks.StrictMock<ISkillStaffPeriod>();
			skillStaffPeriods = new List<ISkillStaffPeriod> { skillStaffPeriodMocked };
		}

		[Test]
		public void VerifyIsSatisfiedByCanReturnTrue()
		{
			Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(-0.11);
			mocks.ReplayAll();
			//skillStaffPeriods[0].RelativeDifference = -0.11;
			Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriods));
			mocks.VerifyAll();
		}

		[Test]
		public void VerifyIsSatisfiedByCanReturnFalse()
		{
			Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(-0.09);
			mocks.ReplayAll();
			//skillStaffPeriods[0].RelativeDifference = -0.09;
			Assert.IsFalse(target.IsSatisfiedBy(skillStaffPeriods));
			mocks.VerifyAll();
		}

		[Test]
		public void VerifyIsSatisfiedByCanHandleNoValue()
		{
			mocks.BackToRecord(skill);
			Expect.Call(skill.StaffingThresholds).Return(new StaffingThresholds(new Percent(), new Percent(), new Percent()));
			Expect.Call(skillStaffPeriodMocked.RelativeDifference).Return(-0.11);
			
			mocks.ReplayAll();
			//skillStaffPeriods[0].RelativeDifference = -0.11;
			Assert.IsTrue(target.IsSatisfiedBy(skillStaffPeriods));
			mocks.VerifyAll();
		}
	}
}
