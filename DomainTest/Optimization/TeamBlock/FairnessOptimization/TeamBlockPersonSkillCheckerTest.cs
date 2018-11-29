using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization
{
	[TestFixture]
	public class TeamBlockPersonsSkillCheckerTest
	{
		private TeamBlockPersonsSkillChecker _target;
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new TeamBlockPersonsSkillChecker();
		}

		[Test]
		public void ShouldReturnFalseIfOneHasMoreSkills()
		{
			var period1 = _mocks.StrictMock<IPersonPeriod>();
			var period2 = _mocks.StrictMock<IPersonPeriod>();
			var skill1 = new Skill("skill1");
			var skill2 = new Skill("skill2");
			var personSkill1 = new PersonSkill(skill1, new Percent(1));
			var personSkill2 = new PersonSkill(skill2, new Percent(1));

			Expect.Call(period1.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1, personSkill2 });
			Expect.Call(period2.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1 });
			_mocks.ReplayAll();
			Assert.That(_target.PersonsHaveSameSkills(period1, period2), Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFalseIfTwoHasMoreSkills()
		{
			var period1 = _mocks.StrictMock<IPersonPeriod>();
			var period2 = _mocks.StrictMock<IPersonPeriod>();
			var skill1 = new Skill("skill1");
			var skill2 = new Skill("skill2");
			var personSkill1 = new PersonSkill(skill1, new Percent(1));
			var personSkill2 = new PersonSkill(skill2, new Percent(1));

			Expect.Call(period2.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1, personSkill2 });
			Expect.Call(period1.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1 });
			_mocks.ReplayAll();
			Assert.That(_target.PersonsHaveSameSkills(period1, period2), Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnTrueIfSameSkills()
		{
			var period1 = _mocks.StrictMock<IPersonPeriod>();
			var period2 = _mocks.StrictMock<IPersonPeriod>();
			var skill1 = new Skill("skill1");
			var skill2 = new Skill("skill2");
			var personSkill1 = new PersonSkill(skill1, new Percent(1));
			var personSkill2 = new PersonSkill(skill2, new Percent(1));

			Expect.Call(period2.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1, personSkill2 });
			Expect.Call(period1.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1, personSkill2 });
			_mocks.ReplayAll();
			Assert.That(_target.PersonsHaveSameSkills(period1, period2), Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldIgnoreNotActiveSkillsBug43037()
		{
			var period1 = _mocks.StrictMock<IPersonPeriod>();
			var period2 = _mocks.StrictMock<IPersonPeriod>();
			var skill1 = new Skill("skill1");
			var skill2 = new Skill("skill2");
			var personSkill1 = new PersonSkill(skill1,new Percent(1));
			var personSkill2 = new PersonSkill(skill2, new Percent(1)) {Active = false};

			Expect.Call(period2.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1, personSkill2 });
			Expect.Call(period1.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1 });
			_mocks.ReplayAll();
			Assert.That(_target.PersonsHaveSameSkills(period1, period2), Is.True);
			_mocks.VerifyAll();
		}
	}
}
