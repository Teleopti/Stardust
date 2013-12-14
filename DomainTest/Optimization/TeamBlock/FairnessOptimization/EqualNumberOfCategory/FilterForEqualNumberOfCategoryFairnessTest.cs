using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	[TestFixture]
	public class FilterForEqualNumberOfCategoryFairnessTest
	{
		private MockRepository _mocks;
		private IFilterForEqualNumberOfCategoryFairness _target;
		private ITeamBlockInfo _teamBlockInfo;
		private ITeamInfo _teamInfo;
		private IPerson _person1;
		private IPerson _person2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new FilterForEqualNumberOfCategoryFairness();
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_person1 = PersonFactory.CreatePerson();
			_person2 = PersonFactory.CreatePerson();
		}

		[Test]
		public void ShouldFilterOutCompleteTeamBlockInfosWithCorrectWorkFlowControlSetOnAllMembers()
		{
			var wfcs = new WorkflowControlSet();
			wfcs.UseShiftCategoryFairness = true;
			_person1.WorkflowControlSet = wfcs;
			_person2.WorkflowControlSet = wfcs;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person1, _person2 });
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> {_teamBlockInfo});
				Assert.That(result[0].Equals(_teamBlockInfo));
			}
		}

		[Test]
		public void ShouldNotIncludeTeamBlockWithIncorrectWorkFlowControlSetOnAMember()
		{
			var wfcs = new WorkflowControlSet();
			wfcs.UseShiftCategoryFairness = true;
			_person1.WorkflowControlSet = wfcs;
			_person2.WorkflowControlSet = new WorkflowControlSet();
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person1, _person2 });
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> { _teamBlockInfo });
				Assert.That(result.Count == 0);
			}
		}

		[Test]
		public void ShouldNotIncludeTeamBlockWithNullWorkFlowControlSetOnAMember()
		{
			var wfcs = new WorkflowControlSet();
			wfcs.UseShiftCategoryFairness = true;
			_person1.WorkflowControlSet = wfcs;
			_person2.WorkflowControlSet = null;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person1, _person2 });
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> { _teamBlockInfo });
				Assert.That(result.Count == 0);
			}
		}

	}
}