using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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
		private ISkill _skill;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_ruleSetSkillActivityChecker = _mocks.StrictMock<IRuleSetSkillActivityChecker>();
			_target = new RuleSetPersonalSkillsActivityFilter(_ruleSetSkillActivityChecker);
			_ruleSet1 = WorkShiftRuleSetFactory.Create();
			_ruleSet2 = WorkShiftRuleSetFactory.Create();
			_ruleSetList = new List<IWorkShiftRuleSet> {_ruleSet1, _ruleSet2};
			_skill = SkillFactory.CreateSkill("skill");
			_person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> {_skill});
		}

		[Test]
		public void ShouldFilterOutRuleSetsContainingActivitiesFromPersonalSkillsOnly()
		{
			using (_mocks.Record())
			{
				Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivities(_ruleSet1, new List<ISkill> {_skill})).Return(false);
				Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivities(_ruleSet2, new List<ISkill> { _skill })).Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(_ruleSetList, _person, DateOnly.MinValue).ToList();
				Assert.AreEqual(1, result.Count);
				Assert.AreSame(_ruleSet2, result[0]);
			}
		}

	}
}