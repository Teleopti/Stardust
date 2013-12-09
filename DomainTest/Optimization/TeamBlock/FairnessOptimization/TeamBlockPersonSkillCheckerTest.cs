using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Interfaces.Domain;

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
			var skill1 = _mocks.DynamicMock<ISkill>();
			var skill2 = _mocks.DynamicMock<ISkill>();
			var personSkill1 = _mocks.DynamicMock<IPersonSkill>();
			var personSkill2 = _mocks.DynamicMock<IPersonSkill>();

			Expect.Call(period1.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1, personSkill2 });
			Expect.Call(period2.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1 });
			Expect.Call(personSkill1.Skill).Return(skill1).Repeat.Twice();
			Expect.Call(personSkill2.Skill).Return(skill2);
			_mocks.ReplayAll();
			Assert.That(_target.PersonsHaveSameSkills(period1, period2), Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFalseIfTwoHasMoreSkills()
		{
			var period1 = _mocks.StrictMock<IPersonPeriod>();
			var period2 = _mocks.StrictMock<IPersonPeriod>();
			var skill1 = _mocks.DynamicMock<ISkill>();
			var skill2 = _mocks.DynamicMock<ISkill>();
			var personSkill1 = _mocks.DynamicMock<IPersonSkill>();
			var personSkill2 = _mocks.DynamicMock<IPersonSkill>();

			Expect.Call(period2.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1, personSkill2 });
			Expect.Call(period1.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1 });
			Expect.Call(personSkill1.Skill).Return(skill1).Repeat.Twice();
			Expect.Call(personSkill2.Skill).Return(skill2);
			_mocks.ReplayAll();
			Assert.That(_target.PersonsHaveSameSkills(period1, period2), Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnTrueIfSameSkills()
		{
			var period1 = _mocks.StrictMock<IPersonPeriod>();
			var period2 = _mocks.StrictMock<IPersonPeriod>();
			var skill1 = _mocks.DynamicMock<ISkill>();
			var skill2 = _mocks.DynamicMock<ISkill>();
			var personSkill1 = _mocks.DynamicMock<IPersonSkill>();
			var personSkill2 = _mocks.DynamicMock<IPersonSkill>();

			Expect.Call(period2.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1, personSkill2 });
			Expect.Call(period1.PersonSkillCollection).Return(new List<IPersonSkill> { personSkill1, personSkill2 });
			Expect.Call(personSkill1.Skill).Return(skill1).Repeat.Twice();
			Expect.Call(personSkill2.Skill).Return(skill2);
			_mocks.ReplayAll();
			Assert.That(_target.PersonsHaveSameSkills(period1, period2), Is.True);
			_mocks.VerifyAll();
		}
	}
}
