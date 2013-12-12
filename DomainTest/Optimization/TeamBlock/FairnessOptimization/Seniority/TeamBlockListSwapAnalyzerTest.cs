using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class TeamBlockListSwapAnalyzerTest
	{
		private MockRepository _mock;
		private IDetermineTeamBlockPriority _determineTeamBlockPriority;
		
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private IList<ITeamBlockInfo> _teamBlockInfos;
		private IShiftCategory _shiftCategory1;
		private IShiftCategory _shiftCategory2;
		private IList<IShiftCategory> _shiftCategories;
		private IScheduleDictionary _scheduleDictionary;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private ITeamBlockSwap _teamBlockSwap;
		private ITeamBlockPriorityDefinitionInfo _teamBlockPriorityDefinitionInfo;
		private IList<ITeamBlockInfo> _highToLowTeamBlockInfo;
		private IList<ITeamBlockInfo> _lowToHighTeamBlockInfo; 
		private TeamBlockListSwapAnalyzer _target;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_determineTeamBlockPriority = _mock.StrictMock<IDetermineTeamBlockPriority>();
			_teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfos = new List<ITeamBlockInfo>{_teamBlockInfo1, _teamBlockInfo2};
			_shiftCategory1 = ShiftCategoryFactory.CreateShiftCategory("AA");
			_shiftCategory2 = ShiftCategoryFactory.CreateShiftCategory("BB");
			_shiftCategories = new List<IShiftCategory>{_shiftCategory1, _shiftCategory2};
			_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
			_schedulePartModifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_teamBlockSwap = _mock.StrictMock<ITeamBlockSwap>();
			_teamBlockPriorityDefinitionInfo = _mock.StrictMock<ITeamBlockPriorityDefinitionInfo>();
			_highToLowTeamBlockInfo = new List<ITeamBlockInfo>{_teamBlockInfo1, _teamBlockInfo2};
			_lowToHighTeamBlockInfo = new List<ITeamBlockInfo>{_teamBlockInfo2, _teamBlockInfo1};
			_target = new TeamBlockListSwapAnalyzer(_determineTeamBlockPriority);	
		}

		[Test]
		public void ShouldSwapBestShiftCategoryToMostSeniorBlock()
		{
			using (_mock.Record())
			{
				Expect.Call(_determineTeamBlockPriority.CalculatePriority(_teamBlockInfos, _shiftCategories)).Return(_teamBlockPriorityDefinitionInfo);
				Expect.Call(_teamBlockPriorityDefinitionInfo.HighToLowSeniorityListBlockInfo).Return(_highToLowTeamBlockInfo);
				Expect.Call(_teamBlockPriorityDefinitionInfo.LowToHighSeniorityListBlockInfo).Return(_lowToHighTeamBlockInfo).Repeat.AtLeastOnce();

				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo1)).Return(1);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(2);

				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo1)).Return(2);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(1);

				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo1)).Return(2);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(1);

				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo1)).Return(2);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(1);
				
				Expect.Call(_teamBlockInfo1.Equals(_teamBlockInfo2));
				Expect.Call(_teamBlockInfo2.Equals(_teamBlockInfo2));

				Expect.Call(_teamBlockSwap.Swap(_teamBlockInfo1, _teamBlockInfo2, _schedulePartModifyAndRollbackService,_scheduleDictionary)).Return(true);
				Expect.Call(() =>_teamBlockPriorityDefinitionInfo.SetShiftCategoryPoint(_teamBlockInfo1, 2));
				Expect.Call(() => _teamBlockPriorityDefinitionInfo.SetShiftCategoryPoint(_teamBlockInfo2, 1));
			}

			using (_mock.Playback())
			{
				_target.AnalyzeTeamBlock(_teamBlockInfos, _shiftCategories, _scheduleDictionary, _schedulePartModifyAndRollbackService, _teamBlockSwap);
			}
		}

		[Test]
		public void ShouldNotSwapWorseShiftCategoryToMostSeniorBlock()
		{
			using (_mock.Record())
			{
				Expect.Call(_determineTeamBlockPriority.CalculatePriority(_teamBlockInfos, _shiftCategories)).Return(_teamBlockPriorityDefinitionInfo);
				Expect.Call(_teamBlockPriorityDefinitionInfo.HighToLowSeniorityListBlockInfo).Return(_highToLowTeamBlockInfo);
				Expect.Call(_teamBlockPriorityDefinitionInfo.LowToHighSeniorityListBlockInfo).Return(_lowToHighTeamBlockInfo).Repeat.AtLeastOnce();

				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo1)).Return(2);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(1);

				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo1)).Return(2);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(1);

				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo1)).Return(2);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(1);

				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo1)).Return(2);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(1);

				Expect.Call(_teamBlockInfo1.Equals(_teamBlockInfo2));
				Expect.Call(_teamBlockInfo2.Equals(_teamBlockInfo2));

			}

			using (_mock.Playback())
			{
				_target.AnalyzeTeamBlock(_teamBlockInfos, _shiftCategories, _scheduleDictionary, _schedulePartModifyAndRollbackService, _teamBlockSwap);
			}
		}
	}
}
