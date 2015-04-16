using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.MoveTimeOptimization
{
	[TestFixture]
	public class TeamBlockMoveTimeBetweenDaysServiceTest
	{
		private ITeamBlockMoveTimeOptimizer _teamBlockMoveTimeOptimizer;
		private ITeamBlockMoveTimeBetweenDaysService _target;
		private MockRepository _mock;
		private IOptimizationPreferences _optimizationPreferences;
		private IScheduleMatrixPro _matrix1;
		private IScheduleMatrixPro _matrix2;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IPeriodValueCalculator _periodValueCalculator;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IPerson _person1;
		private IConstructTeamBlock _constructTeamBlock;
		private IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private IFilterForNoneLockedTeamBlocks _filterForNoneLockedTeamBlocks;
		private DateOnlyPeriod _selectedPeriod;
		private ITeamBlockInfo _teamBlockInfo;
		private ITeamInfo _teamInfo;
		private IBlockInfo _blockInfo;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private IEnumerable<IPerson> _groupMembers;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_teamBlockMoveTimeOptimizer = _mock.StrictMock<ITeamBlockMoveTimeOptimizer>();
			
			_optimizationPreferences = new OptimizationPreferences();
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mock.StrictMock<IScheduleMatrixPro>();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_periodValueCalculator = _mock.StrictMock<IPeriodValueCalculator>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_person1 = PersonFactory.CreatePerson("test");
			_constructTeamBlock = _mock.StrictMock<IConstructTeamBlock>();
			_filterForTeamBlockInSelection = _mock.StrictMock<IFilterForTeamBlockInSelection>();
			_filterForNoneLockedTeamBlocks = _mock.StrictMock<IFilterForNoneLockedTeamBlocks>();
			_selectedPeriod = new DateOnlyPeriod(2014,09,01,2014,09,01);
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_blockInfo = _mock.StrictMock<IBlockInfo>();
			_teamBlockInfo = new TeamBlockInfo(_teamInfo,_blockInfo);
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_groupMembers = new List<IPerson> {_person1};
		}

		[Test]
		public void ShouldExecuteService()
		{
			_target = new TeamBlockMoveTimeBetweenDaysService(_teamBlockMoveTimeOptimizer,_constructTeamBlock,_filterForTeamBlockInSelection,_filterForNoneLockedTeamBlocks);
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro>{_matrix1,_matrix2};
			IList<IPerson> selectedPersons = new List<IPerson> {_person1};
			IList<IScheduleMatrixPro> matrixesOnSelectedperiod = new List<IScheduleMatrixPro> { _matrix1 };
			IList<ITeamBlockInfo> teamBlockList = new List<ITeamBlockInfo> {_teamBlockInfo};
			using (_mock.Record())
			{
				Expect.Call(_constructTeamBlock.Construct(matrixList, _selectedPeriod, selectedPersons,
					_optimizationPreferences.Extra.BlockTypeValue, _optimizationPreferences.Extra.TeamGroupPage)).Return(teamBlockList );
				Expect.Call(_filterForTeamBlockInSelection.Filter(teamBlockList, selectedPersons, _selectedPeriod)).Return(teamBlockList);
				Expect.Call(_filterForNoneLockedTeamBlocks.Filter(teamBlockList)).Return(teamBlockList.ToList());
				Expect.Call(_teamInfo.GroupMembers).Return(_groupMembers);
				Expect.Call(_teamInfo.MatrixesForMemberAndPeriod(_person1, _selectedPeriod)).Return(matrixesOnSelectedperiod);
				Expect.Call(_teamBlockMoveTimeOptimizer.OptimizeTeam(_optimizationPreferences, _teamInfo, _matrix1, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, _resourceCalculateDelayer)).Return(false);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
					.Return(0.815689).Repeat.AtLeastOnce() ;
				Expect.Call(_teamInfo.Name).Return("hej");
			}
			using (_mock.Playback())
			{
				_target.Execute(_optimizationPreferences, matrixList, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, selectedPersons,_selectedPeriod, _resourceCalculateDelayer);
			}
		}

		[Test]
		public void ShouldNotExecuteIfCanceled()
		{
			_target = new TeamBlockMoveTimeBetweenDaysService(_teamBlockMoveTimeOptimizer, _constructTeamBlock, _filterForTeamBlockInSelection, _filterForNoneLockedTeamBlocks);
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _matrix1, _matrix2 };
			IList<IPerson> selectedPersons = new List<IPerson> { _person1 };
			IList<IScheduleMatrixPro> matrixesOnSelectedperiod = new List<IScheduleMatrixPro> { _matrix1 };
			IList<ITeamBlockInfo> teamBlockList = new List<ITeamBlockInfo> { _teamBlockInfo };
			using (_mock.Record())
			{
				Expect.Call(_constructTeamBlock.Construct(matrixList, _selectedPeriod, selectedPersons,
					_optimizationPreferences.Extra.BlockTypeValue, _optimizationPreferences.Extra.TeamGroupPage)).Return(teamBlockList);
				Expect.Call(_filterForTeamBlockInSelection.Filter(teamBlockList, selectedPersons, _selectedPeriod)).Return(teamBlockList);
				Expect.Call(_filterForNoneLockedTeamBlocks.Filter(teamBlockList)).Return(teamBlockList.ToList());
				Expect.Call(_teamInfo.GroupMembers).Return(_groupMembers).Repeat.AtLeastOnce();
				Expect.Call(_teamInfo.MatrixesForMemberAndPeriod(_person1, _selectedPeriod)).Return(matrixesOnSelectedperiod).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockMoveTimeOptimizer.OptimizeTeam(_optimizationPreferences, _teamInfo, _matrix1, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, _resourceCalculateDelayer)).Return(true);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(0.815689);
				Expect.Call(_teamInfo.Name).Return("hej");
			}
			using (_mock.Playback())
			{
				_target.ReportProgress += testCancel;
				_target.Execute(_optimizationPreferences, matrixList, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, selectedPersons, _selectedPeriod,_resourceCalculateDelayer);
				_target.ReportProgress -= testCancel;
			}
		}

		[Test]
		public void ShouldUserCancel()
		{
			_target = new TeamBlockMoveTimeBetweenDaysService(_teamBlockMoveTimeOptimizer, _constructTeamBlock, _filterForTeamBlockInSelection, _filterForNoneLockedTeamBlocks);
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _matrix1, _matrix2 };
			IList<IPerson> selectedPersons = new List<IPerson> { _person1 };
			IList<IScheduleMatrixPro> matrixesOnSelectedperiod = new List<IScheduleMatrixPro> { _matrix1 };
			IList<ITeamBlockInfo> teamBlockList = new List<ITeamBlockInfo> { _teamBlockInfo };
			using (_mock.Record())
			{
				Expect.Call(_constructTeamBlock.Construct(matrixList, _selectedPeriod, selectedPersons,_optimizationPreferences.Extra.BlockTypeValue, _optimizationPreferences.Extra.TeamGroupPage)).Return(teamBlockList);
				Expect.Call(_filterForTeamBlockInSelection.Filter(teamBlockList, selectedPersons, _selectedPeriod)).Return(teamBlockList);
				Expect.Call(_filterForNoneLockedTeamBlocks.Filter(teamBlockList)).Return(teamBlockList.ToList());
				Expect.Call(_teamInfo.GroupMembers).Return(_groupMembers).Repeat.AtLeastOnce();
				Expect.Call(_teamInfo.MatrixesForMemberAndPeriod(_person1, _selectedPeriod)).Return(matrixesOnSelectedperiod).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockMoveTimeOptimizer.OptimizeTeam(_optimizationPreferences, _teamInfo, _matrix1, _rollbackService, _periodValueCalculator,_schedulingResultStateHolder, _resourceCalculateDelayer)).Return(true);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(0.815689);
				Expect.Call(_teamInfo.Name).Return("hej");
			}
			using (_mock.Playback())
			{
				_target.ReportProgress += testUserCancel;
				_target.Execute(_optimizationPreferences, matrixList, _rollbackService, _periodValueCalculator,_schedulingResultStateHolder, selectedPersons, _selectedPeriod, _resourceCalculateDelayer);
				_target.ReportProgress -= testUserCancel;
			}
		}

		private void testCancel(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = true;
		}

		private void testUserCancel(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.CancelAction();
		}
	}
}
