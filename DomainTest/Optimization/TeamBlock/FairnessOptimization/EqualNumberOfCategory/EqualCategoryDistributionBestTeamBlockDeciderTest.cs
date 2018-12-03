using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	[TestFixture]
	public class EqualCategoryDistributionBestTeamBlockDeciderTest
	{
		private MockRepository _mocks;
		private IEqualCategoryDistributionBestTeamBlockDecider _target;
		private IDistributionForPersons _distributionForPersons;
		private IScheduleDictionary _scheduleDictionary;
		private ITeamBlockInfo _teamBlockToSwap;
		private ITeamBlockInfo _possibleTeamBlock;
		private ITeamInfo _teamInfo;
		private IScheduleRange _range;
		private IScheduleDay _day;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_distributionForPersons = _mocks.StrictMock<IDistributionForPersons>();
			_target = new EqualCategoryDistributionBestTeamBlockDecider(_distributionForPersons);
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_teamBlockToSwap = _mocks.StrictMock<ITeamBlockInfo>();
			_possibleTeamBlock = _mocks.StrictMock<ITeamBlockInfo>();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_range = _mocks.StrictMock<IScheduleRange>();
			_day = _mocks.StrictMock<IScheduleDay>();
		}

		[Test]
		public void ShouldSelectIfOtherCategory()
		{
			var possibleBlocksToSwap = new List<ITeamBlockInfo> {_possibleTeamBlock};
			var category1 = ShiftCategoryFactory.CreateShiftCategory("hej");
			var category2 = ShiftCategoryFactory.CreateShiftCategory("hopp");
			var distributionDic1 = new Dictionary<IShiftCategory, int> {{category1, 1}, {category2, 1}};
			var distributionSummary1 = new DistributionSummary(distributionDic1);
			var distributionDic2 = new Dictionary<IShiftCategory, int> {{category2, 1}};
			var distributionSummary2 = new DistributionSummary(distributionDic2);
			var person1 = PersonFactory.CreatePerson();
			var groupMembers1 = new List<IPerson> {person1};
			var person2 = PersonFactory.CreatePerson();
			var groupMembers2 = new List<IPerson> { person2 };
			var period = new DateTimePeriod(2013, 09, 12, 8, 2013, 09, 12, 9);
			
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
			                                                                             new Scenario("hepp"), period, category1);
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockToSwap.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(groupMembers1);
				Expect.Call(_distributionForPersons.CreateSummary(groupMembers1, _scheduleDictionary)).Return(distributionSummary2);

				Expect.Call(_possibleTeamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(groupMembers2);
				Expect.Call(_possibleTeamBlock.BlockInfo).Return(new BlockInfo(new DateOnlyPeriod(2013, 12, 3, 2013, 12, 3)));
				Expect.Call(_scheduleDictionary[person2]).Return(_range);
				Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(2013, 12, 3, 2013, 12, 3))).Return(new [] { _day});
				Expect.Call(_day.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				Expect.Call(_day.PersonAssignment()).Return(personAssignment);
			}

			using (_mocks.Playback())
			{
				var result = _target.FindBestSwap(_teamBlockToSwap, possibleBlocksToSwap, distributionSummary1, _scheduleDictionary);
				Assert.That(result.Equals(_possibleTeamBlock));
			}
		}

	}
}