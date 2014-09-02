using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		private bool _cancel;
		private IConstructTeamBlock _constructTeamBlock;
		private IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private IFilterForNoneLockedTeamBlocks _filterForNoneLockedTeamBlocks;
		private DateOnlyPeriod _selectedPeriod;
		private ITeamBlockInfo _teamBlockInfo;
		private ITeamInfo _teamInfo;
		private IBlockInfo _blockInfo;

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
		}

		[Test]
		public void ShouldExecuteService()
		{
			_target = new TeamBlockMoveTimeBetweenDaysService(_teamBlockMoveTimeOptimizer,_constructTeamBlock,_filterForTeamBlockInSelection,_filterForNoneLockedTeamBlocks);
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro>{_matrix1,_matrix2};
			IList<IPerson> selectedPersons = new List<IPerson> {_person1};
			IList<IScheduleMatrixPro> matrixesOnSelectedperiod = new List<IScheduleMatrixPro> { _matrix1 };
			IList<ITeamBlockInfo> teamBlockList = new List<ITeamBlockInfo>() {_teamBlockInfo};
			using (_mock.Record())
			{
				Expect.Call(_constructTeamBlock.Construct(matrixList, _selectedPeriod, selectedPersons,
					_optimizationPreferences.Extra.BlockTypeValue, _optimizationPreferences.Extra.TeamGroupPage)).Return(teamBlockList );
				Expect.Call(_filterForTeamBlockInSelection.Filter(teamBlockList, selectedPersons, _selectedPeriod)).Return(teamBlockList);
				Expect.Call(_filterForNoneLockedTeamBlocks.Filter(teamBlockList)).Return(teamBlockList.ToList());
				Expect.Call(_teamInfo.MatrixesForUnlockedMembersAndPeriod(_selectedPeriod)).Return(matrixesOnSelectedperiod);
				Expect.Call(_matrix1.Person).Return(_person1).Repeat.AtLeastOnce() ;
				Expect.Call(_teamBlockMoveTimeOptimizer.OptimizeMatrix(_optimizationPreferences, matrixList, _rollbackService,
					_periodValueCalculator, _schedulingResultStateHolder, _matrix1, matrixesOnSelectedperiod)).Return(false);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
					.Return(0.815689).Repeat.AtLeastOnce() ;

			}
			using (_mock.Playback())
			{
				
				_target.Execute(_optimizationPreferences, matrixList, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, selectedPersons,_selectedPeriod);
			}
		}

		[Test]
		public void ShouldNotExecuteIfCanceled()
		{
			_target = new TeamBlockMoveTimeBetweenDaysService(_teamBlockMoveTimeOptimizer, _constructTeamBlock, _filterForTeamBlockInSelection, _filterForNoneLockedTeamBlocks);
			_cancel = false;
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _matrix1, _matrix2 };
			IList<IPerson> selectedPersons = new List<IPerson> { _person1 };
			IList<IScheduleMatrixPro> matrixesOnSelectedperiod = new List<IScheduleMatrixPro> { _matrix1 };
			IList<ITeamBlockInfo> teamBlockList = new List<ITeamBlockInfo>() { _teamBlockInfo };
			using (_mock.Record())
			{
				Expect.Call(_constructTeamBlock.Construct(matrixList, _selectedPeriod, selectedPersons,
					_optimizationPreferences.Extra.BlockTypeValue, _optimizationPreferences.Extra.TeamGroupPage)).Return(teamBlockList);
				Expect.Call(_filterForTeamBlockInSelection.Filter(teamBlockList, selectedPersons, _selectedPeriod)).Return(teamBlockList);
				Expect.Call(_filterForNoneLockedTeamBlocks.Filter(teamBlockList)).Return(teamBlockList.ToList());
				Expect.Call(_teamInfo.MatrixesForUnlockedMembersAndPeriod(_selectedPeriod)).Return(matrixesOnSelectedperiod).Repeat.AtLeastOnce() ;
				Expect.Call(_matrix1.Person).Return(_person1);
				Expect.Call(_teamBlockMoveTimeOptimizer.OptimizeMatrix(_optimizationPreferences, matrixList, _rollbackService,
					_periodValueCalculator, _schedulingResultStateHolder, _matrix1, matrixesOnSelectedperiod)).Return(true);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(0.815689);

			}
			using (_mock.Playback())
			{
				_target.ReportProgress += testCancel;
				_cancel = true;
				_target.Execute(_optimizationPreferences, matrixList, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, selectedPersons, _selectedPeriod );
				_target.ReportProgress -= testCancel;
			}
		}

		private void testCancel(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = _cancel;
		}
	}
}
