using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class TeamSteadyStatePersonPeriodTest
	{
		private TeamSteadyStatePersonPeriod _target;
		private IPersonPeriod _personPeriodTarget;
		private IPersonPeriod _personPeriod;
		private MockRepository _mocks;
		private IPersonSkill _personSkill1;
		private IPersonSkill _personSkill2;
		private IPersonContract _personContract1;
		private IPersonContract _personContract2;
		private IContract _contract1;
		private IContract _contract2;
		private IPartTimePercentage _partTimePercentage1;
		private IPartTimePercentage _partTimePercentage2;
		private IRuleSetBag _ruleSetBag1;
		private IRuleSetBag _ruleSetBag2;
		private ISkill _skill1;
		private ISkill _skill2;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_personPeriodTarget = _mocks.StrictMock<IPersonPeriod>();
			_personPeriod = _mocks.StrictMock<IPersonPeriod>();
			_personSkill1 = _mocks.StrictMock<IPersonSkill>();
			_personSkill2 = _mocks.StrictMock<IPersonSkill>();
			_personContract1 = _mocks.StrictMock<IPersonContract>();
			_personContract2 = _mocks.StrictMock<IPersonContract>();
			_contract1 = _mocks.StrictMock<IContract>();
			_contract2 = _mocks.StrictMock<IContract>();
			_partTimePercentage1 = _mocks.StrictMock<IPartTimePercentage>();
			_partTimePercentage2 = _mocks.StrictMock<IPartTimePercentage>();
			_ruleSetBag1 = _mocks.StrictMock<IRuleSetBag>();
			_ruleSetBag2 = _mocks.StrictMock<IRuleSetBag>();
			_target = new TeamSteadyStatePersonPeriod(_personPeriodTarget);
			_skill1 = _mocks.StrictMock<ISkill>();
			_skill2 = _mocks.StrictMock<ISkill>();
		}

		[Test]
		public void ShouldNotEqualOnDifferentNumberOfSkills()
		{
			using(_mocks.Record())
			{
				Expect.Call(_personPeriodTarget.PersonSkillCollection).Return(new List<IPersonSkill>{_personSkill1});
				Expect.Call(_personPeriod.PersonSkillCollection).Return(new List<IPersonSkill>());
			}

			using(_mocks.Playback())
			{
				Assert.IsFalse(_target.PersonPeriodEquals(_personPeriod));
			}
		}

		[Test]
		public void ShouldNotEqualOnDifferentSkills()
		{
			using (_mocks.Record())
			{
				Expect.Call(_personPeriodTarget.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 }).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill2 }).Repeat.AtLeastOnce();
				Expect.Call(_personSkill1.Skill).Return(_skill1);
				Expect.Call(_personSkill2.Skill).Return(_skill2);
				Expect.Call(_skill1.Equals(_skill2)).Return(false);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.PersonPeriodEquals(_personPeriod));
			}		
		}

		[Test]
		public void ShouldNotEqualOnDifferentContract()
		{
			using (_mocks.Record())
			{
				Expect.Call(_personPeriodTarget.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 }).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 }).Repeat.AtLeastOnce();
				Expect.Call(_personPeriodTarget.PersonContract).Return(_personContract1);
				Expect.Call(_personPeriod.PersonContract).Return(_personContract2);
				Expect.Call(_personContract1.Contract).Return(_contract1);
				Expect.Call(_personContract2.Contract).Return(_contract2);
				Expect.Call(_contract1.Equals(_contract2)).Return(false);
				Expect.Call(_personSkill1.Skill).Return(_skill1).Repeat.AtLeastOnce();
				Expect.Call(_skill1.Equals(_skill1)).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.PersonPeriodEquals(_personPeriod));
			}			
		}

		[Test]
		public void ShouldNotEqualOnDifferentPartTimePercentage()
		{
			using (_mocks.Record())
			{
				Expect.Call(_personPeriodTarget.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 }).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 }).Repeat.AtLeastOnce();
				Expect.Call(_personPeriodTarget.PersonContract).Return(_personContract1).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.PersonContract).Return(_personContract2).Repeat.AtLeastOnce();
				Expect.Call(_personContract1.Contract).Return(_contract1);
				Expect.Call(_personContract2.Contract).Return(_contract1);
				Expect.Call(_contract1.Equals(_contract1)).Return(true);
				Expect.Call(_personContract1.PartTimePercentage).Return(_partTimePercentage1);
				Expect.Call(_personContract2.PartTimePercentage).Return(_partTimePercentage2);
				Expect.Call(_partTimePercentage1.Equals(_partTimePercentage2)).Return(false);
				Expect.Call(_personSkill1.Skill).Return(_skill1).Repeat.AtLeastOnce();
				Expect.Call(_skill1.Equals(_skill1)).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.PersonPeriodEquals(_personPeriod));
			}			
		}

		[Test]
		public void ShouldNotEqualOnDifferentShiftBag()
		{
			using (_mocks.Record())
			{
				Expect.Call(_personPeriodTarget.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 }).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 }).Repeat.AtLeastOnce();
				Expect.Call(_personPeriodTarget.PersonContract).Return(_personContract1).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.PersonContract).Return(_personContract2).Repeat.AtLeastOnce();
				Expect.Call(_personContract1.Contract).Return(_contract1);
				Expect.Call(_personContract2.Contract).Return(_contract1);
				Expect.Call(_contract1.Equals(_contract1)).Return(true);
				Expect.Call(_personContract1.PartTimePercentage).Return(_partTimePercentage1);
				Expect.Call(_personContract2.PartTimePercentage).Return(_partTimePercentage2);
				Expect.Call(_partTimePercentage1.Equals(_partTimePercentage2)).Return(true);
				Expect.Call(_personPeriodTarget.RuleSetBag).Return(_ruleSetBag1).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag2);
				Expect.Call(_ruleSetBag1.Equals(_ruleSetBag2)).Return(false);
				Expect.Call(_personSkill1.Skill).Return(_skill1).Repeat.AtLeastOnce();
				Expect.Call(_skill1.Equals(_skill1)).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.PersonPeriodEquals(_personPeriod));
			}	
		}

		[Test]
		public void ShouldNotEqualOnTargetShiftBagIsNull()
		{
			using (_mocks.Record())
			{
				Expect.Call(_personPeriodTarget.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 }).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 }).Repeat.AtLeastOnce();
				Expect.Call(_personPeriodTarget.PersonContract).Return(_personContract1).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.PersonContract).Return(_personContract2).Repeat.AtLeastOnce();
				Expect.Call(_personContract1.Contract).Return(_contract1);
				Expect.Call(_personContract2.Contract).Return(_contract1);
				Expect.Call(_contract1.Equals(_contract1)).Return(true);
				Expect.Call(_personContract1.PartTimePercentage).Return(_partTimePercentage1);
				Expect.Call(_personContract2.PartTimePercentage).Return(_partTimePercentage2);
				Expect.Call(_partTimePercentage1.Equals(_partTimePercentage2)).Return(true);
				Expect.Call(_personPeriodTarget.RuleSetBag).Return(null);
				Expect.Call(_personSkill1.Skill).Return(_skill1).Repeat.AtLeastOnce();
				Expect.Call(_skill1.Equals(_skill1)).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.PersonPeriodEquals(_personPeriod));
			}	
		}

		[Test]
		public void ShouldEqualOnSameTargetValues()
		{
			using (_mocks.Record())
			{
				Expect.Call(_personPeriodTarget.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 }).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 }).Repeat.AtLeastOnce();
				Expect.Call(_personPeriodTarget.PersonContract).Return(_personContract1).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.PersonContract).Return(_personContract2).Repeat.AtLeastOnce();
				Expect.Call(_personContract1.Contract).Return(_contract1);
				Expect.Call(_personContract2.Contract).Return(_contract1);
				Expect.Call(_contract1.Equals(_contract1)).Return(true);
				Expect.Call(_personContract1.PartTimePercentage).Return(_partTimePercentage1);
				Expect.Call(_personContract2.PartTimePercentage).Return(_partTimePercentage2);
				Expect.Call(_partTimePercentage1.Equals(_partTimePercentage2)).Return(true);
				Expect.Call(_personPeriodTarget.RuleSetBag).Return(_ruleSetBag1).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag1);
				Expect.Call(_ruleSetBag1.Equals(_ruleSetBag1)).Return(true);
				Expect.Call(_personSkill1.Skill).Return(_skill1).Repeat.AtLeastOnce();
				Expect.Call(_skill1.Equals(_skill1)).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.PersonPeriodEquals(_personPeriod));
			}
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionOnNullPersonPeriod()
		{
			_target.PersonPeriodEquals(null);
		}
	}
}
