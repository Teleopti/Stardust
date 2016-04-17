using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
    [TestFixture]
    public class RuleSetAccordingToAccessabilityFilterTest
    {
        private IRuleSetAccordingToAccessabilityFilter _target;
        private IWorkShiftRuleSet _workShiftRuleSet1;
        private MockRepository _mock;
        private IRuleSetBag _ruleSetBag;
        private ITeamBlockIncludedWorkShiftRuleFilter _teamBlockIncludedWorkShiftRuleFilter;
        private ITeamBlockRuleSetBagExtractor _teamBlockRuleSetBagExtractor;
        private ITeamBlockInfo _teamBlockInfo;
        private IBlockInfo _blockInfo;
	    private IRuleSetSkillActivityChecker _ruleSetSkillActivityChecker;
	    private IGroupPersonSkillAggregator _skillAggregator;
	    private ITeamInfo _teamInfo;
	    private List<ISkill> _skillList;
	    private ISkill _skill;
		private List<IPerson> _groupMembers;
	    private IPerson _person;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _teamBlockIncludedWorkShiftRuleFilter = _mock.StrictMock<ITeamBlockIncludedWorkShiftRuleFilter>();
            _teamBlockRuleSetBagExtractor = _mock.StrictMock<ITeamBlockRuleSetBagExtractor>();
            _teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
            _blockInfo = _mock.StrictMock<IBlockInfo>();
	        _ruleSetSkillActivityChecker = _mock.StrictMock<IRuleSetSkillActivityChecker>();
		    _skillAggregator = _mock.StrictMock<IGroupPersonSkillAggregator>();
		    _target = new RuleSetAccordingToAccessabilityFilter(_teamBlockRuleSetBagExtractor,
			    _teamBlockIncludedWorkShiftRuleFilter, _ruleSetSkillActivityChecker, _skillAggregator);
            _workShiftRuleSet1 = _mock.StrictMock<IWorkShiftRuleSet>();
            _ruleSetBag = _mock.StrictMock<IRuleSetBag>();
		    _teamInfo = _mock.StrictMock<ITeamInfo>();
		    _skill = SkillFactory.CreateSkill("skill");
		    _skillList = new List<ISkill>{_skill};
		    _person = PersonFactory.CreatePerson();
			_groupMembers = new List<IPerson>();
        }

        [Test]
        public void ShouldExecuteTeamBlock()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 05, 2014, 03, 09);

            using (_mock.Record())
            {
                Expect.Call(_teamBlockRuleSetBagExtractor.GetRuleSetBag(_teamBlockInfo))
                      .Return(new List<IRuleSetBag> { _ruleSetBag });
                Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo).Repeat.Twice();
                Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod).Repeat.Twice();
                Expect.Call(_teamBlockIncludedWorkShiftRuleFilter.Filter(dateOnlyPeriod, new List<IRuleSetBag> { _ruleSetBag }))
                      .Return(new List<IWorkShiftRuleSet> { _workShiftRuleSet1 });
	            Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
	            Expect.Call(_teamInfo.GroupMembers).Return(_groupMembers);
				Expect.Call(_skillAggregator.AggregatedSkills(_groupMembers, dateOnlyPeriod)).Return(_skillList);
	            Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivities(_workShiftRuleSet1, _skillList)).Return(true);
            }
            using (_mock.Playback())
            {
                var result = _target.FilterForRoleModel(_teamBlockInfo);
                Assert.AreEqual(new List<IWorkShiftRuleSet> { _workShiftRuleSet1 }, result);
            }

        }

        [Test]
        public void ShouldExecuteTeamBlockOnSingleDay()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 05, 2014, 03, 05);

            using (_mock.Record())
            {
                Expect.Call(_teamBlockRuleSetBagExtractor.GetRuleSetBagForTeamMember(_person, new DateOnly(2014, 03, 05)))
                      .Return(new List<IRuleSetBag> { _ruleSetBag });
                Expect.Call(_teamBlockIncludedWorkShiftRuleFilter.Filter(dateOnlyPeriod, new List<IRuleSetBag> { _ruleSetBag }))
                      .Return(new List<IWorkShiftRuleSet> { _workShiftRuleSet1 });
            }
            using (_mock.Playback())
            {
                var result = _target.FilterForTeamMember(_person, new DateOnly(2014, 03, 05));
				Assert.IsTrue(result.ToList().Contains(_workShiftRuleSet1));
            }

        }

    }


}
