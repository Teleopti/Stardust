using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class RuleSetPersonalSkillsActivityFilterTest
	{
		private MockRepository _mocks;
		private IRuleSetPersonalSkillsActivityFilter _target;
		private IRuleSetSkillActivityChecker _ruleSetSkillActivityChecker;
		private IList<IWorkShiftRuleSet> _ruleSetList; 
		private IPerson _person;
		private WorkShiftRuleSet _ruleSet1;
		private WorkShiftRuleSet _ruleSet2;
		private ISkill _skill1;
		private ISkill _skill2;
		private ITeamInfo _teamInfo;
		private IPerson _person2;
		private List<IPerson> _groupMemebers;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_ruleSetSkillActivityChecker = _mocks.StrictMock<IRuleSetSkillActivityChecker>();
			_target = new RuleSetPersonalSkillsActivityFilter(_ruleSetSkillActivityChecker, new PersonalSkillsProvider());
			_ruleSet1 = WorkShiftRuleSetFactory.Create();
			_ruleSet2 = WorkShiftRuleSetFactory.Create();
			_ruleSetList = new List<IWorkShiftRuleSet> {_ruleSet1, _ruleSet2};
			_skill1 = SkillFactory.CreateSkill("skill");
			_skill2 = SkillFactory.CreateSkill("skill");
			_person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> {_skill1});
			_person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _skill1, _skill2 });
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_groupMemebers = new List<IPerson> {_person2, _person};
		}

		[Test]
		public void ShouldFilterOutRuleSetsContainingActivitiesFromPersonalSkillsOnly()
		{
			using (_mocks.Record())
			{
				Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivities(_ruleSet1, new List<ISkill> {_skill1})).Return(false);
				Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivities(_ruleSet2, new List<ISkill> { _skill1 })).Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(_ruleSetList, _person.Period(DateOnly.MinValue), DateOnly.MinValue).ToList();
				Assert.AreEqual(1, result.Count);
				Assert.AreSame(_ruleSet2, result[0]);
			}
		}

		[Test]
		public void ShouldFilterForCommonForTeamMembers()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfo.GroupMembers).Return(_groupMemebers);
				//model list
				Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivities(_ruleSet1, new List<ISkill> { _skill1, _skill2 })).Return(true);
				Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivities(_ruleSet2, new List<ISkill> { _skill1, _skill2 })).Return(true);
				
				//first member
				Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivities(_ruleSet1, new List<ISkill> { _skill1, _skill2 })).Return(true);
				Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivities(_ruleSet2, new List<ISkill> { _skill1, _skill2 })).Return(true);

				//second member
				Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivities(_ruleSet1, new List<ISkill> { _skill1 })).Return(true);
				Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivities(_ruleSet2, new List<ISkill> { _skill1 })).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.FilterForRoleModel(_ruleSetList, _teamInfo, DateOnly.MinValue).ToList();
				Assert.AreEqual(1, result.Count);
				Assert.AreSame(_ruleSet1, result[0]);
			}
		}
	}
}