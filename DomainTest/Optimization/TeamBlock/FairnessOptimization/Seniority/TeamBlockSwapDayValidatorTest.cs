using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public class TeamBlockSwapDayValidatorTest
	{
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private MockRepository _mock;
		private DateOnly _dateOnly;
		private IPersonPeriod _personPeriod1;
		private IPersonPeriod _personPeriod2;
		private ITeamBlockPersonsSkillChecker _teamBlockPersonsSkillChecker;
		private IRuleSetBag _ruleSetBag;
		private ISkill _skill;
		private TeamBlockSwapDayValidator _target;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_dateOnly = new DateOnly(2013, 1, 1);
			_scheduleDay1 = ScheduleDayFactory.Create(_dateOnly);
			_scheduleDay2 = ScheduleDayFactory.Create(_dateOnly);
			_ruleSetBag = new RuleSetBag();
			_skill = SkillFactory.CreateSkill("Skill");
			_personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(_dateOnly, _ruleSetBag, _skill);
			_personPeriod2 = PersonPeriodFactory.CreatePersonPeriodWithSkills(_dateOnly, _ruleSetBag, _skill);
			_scheduleDay1.Person.AddPersonPeriod(_personPeriod1);
			_scheduleDay2.Person.AddPersonPeriod(_personPeriod2);
			_teamBlockPersonsSkillChecker = _mock.StrictMock<ITeamBlockPersonsSkillChecker>();
			_target = new TeamBlockSwapDayValidator(_teamBlockPersonsSkillChecker);	
		}

		[Test]
		public void ShouldReturnTrueWhenPossibleToSwap()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockPersonsSkillChecker.PersonsHaveSameSkills(_personPeriod1, _personPeriod2)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateSwapDays(_scheduleDay1, _scheduleDay2);
				Assert.IsTrue(result);
			}	
		}

		[Test]
		public void ShouldReturnFalseWhenDifferentSkills()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockPersonsSkillChecker.PersonsHaveSameSkills(_personPeriod1, _personPeriod2)).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateSwapDays(_scheduleDay1, _scheduleDay2);
				Assert.IsFalse(result);
			}		
		}

		[Test]
		public void ShouldReturnFalseWhenDifferentRuleSetBag()
		{
			_personPeriod1.RuleSetBag = new RuleSetBag();
			using (_mock.Record())
			{
				Expect.Call(_teamBlockPersonsSkillChecker.PersonsHaveSameSkills(_personPeriod1, _personPeriod2)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateSwapDays(_scheduleDay1, _scheduleDay2);
				Assert.IsFalse(result);
			}		
		}

		[Test]
		public void ShouldReturnFalseWhenDifferentContractTime()
		{
			var activity = new Activity("activity") {InContractTime = true};
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("ShiftCatgory");
			_scheduleDay1.CreateAndAddActivity(activity, _scheduleDay1.Period, shiftCategory);

			using (_mock.Record())
			{
				Expect.Call(_teamBlockPersonsSkillChecker.PersonsHaveSameSkills(_personPeriod1, _personPeriod2)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateSwapDays(_scheduleDay1, _scheduleDay2);
				Assert.IsFalse(result);
			}		
		}
	}
}
