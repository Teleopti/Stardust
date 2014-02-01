using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	[TestFixture]
	public class SeniorityTeamBlockSwapperServiceTest
	{
		private MockRepository _mocks;
		private IConstructTeamBlock _constructTeamBlock;
		private IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private IFilterForFullyScheduledBlocks _filterForFullyScheduledBlocks;
		private ISeniorityExtractor _seniorityExtractor;
		private IFilterOnSwapableTeamBlocks _filterOnSwapableTeamBlocks;
		private ITeamBlockLocatorWithHighestPoints _teamBlockLocatorWithHighestPoints;
		private IWeekDayPointCalculatorForTeamBlock _weekDayPointCalculatorForTeamBlock;
		private ISeniorityTeamBlockSwapper _seniorityTeamBlockSwapper;
		private ITeamBlockSeniorityValidator _teamBlockSeniorityValidator;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private ISeniorityTeamBlockSwapperService _target;
		private ITeamBlockInfo _juniorTeamBlock;
		private ITeamBlockInfo _seniorTeamBlock;
		private IScheduleMatrixPro _matrix1;
		private IScheduleMatrixPro _matrix2;
		private IList<IScheduleMatrixPro> _matrixList;
		private ISchedulingOptions _schedulingOptions;
		private IScheduleDictionary _scheduleDictionary;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IOptimizationPreferences _optimizationPreferences;
		private ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
		private IList<IPerson> _selectedPersons;
		private IList<ITeamBlockInfo> _teamBlocksFirstLoop;
		private IList<ITeamBlockInfo> _teamBlocksScondLoop;
		private ITeamBlockPoints _teamBlockPoint1;
		private ITeamBlockPoints _teamBlockPoint2;
		private IList<ITeamBlockPoints> _teamBlockPointsList;
		private IDictionary<ITeamBlockInfo, int> _seniorityDic;
		private IDictionary<DayOfWeek, int> _weekDayValueDic;
		private IDictionary<ITeamBlockInfo, double> _seniorityValueDic;
		private ITeamInfo _seniorTeamInfo;
		private ITeamInfo _juniorTeamInfo;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_constructTeamBlock = _mocks.StrictMock<IConstructTeamBlock>();
			_filterForTeamBlockInSelection = _mocks.StrictMock<IFilterForTeamBlockInSelection>();
			_filterForFullyScheduledBlocks = _mocks.StrictMock<IFilterForFullyScheduledBlocks>();
			_seniorityExtractor = _mocks.StrictMock<ISeniorityExtractor>();
			_filterOnSwapableTeamBlocks = _mocks.StrictMock<IFilterOnSwapableTeamBlocks>();
			_teamBlockLocatorWithHighestPoints = _mocks.StrictMock<ITeamBlockLocatorWithHighestPoints>();
			_weekDayPointCalculatorForTeamBlock = _mocks.StrictMock<IWeekDayPointCalculatorForTeamBlock>();
			_seniorityTeamBlockSwapper = _mocks.StrictMock<ISeniorityTeamBlockSwapper>();
			_teamBlockSeniorityValidator = _mocks.StrictMock<ITeamBlockSeniorityValidator>();
			_schedulingOptionsCreator = _mocks.StrictMock<ISchedulingOptionsCreator>();
			_target = new SeniorityTeamBlockSwapperService(_constructTeamBlock, _filterForTeamBlockInSelection,
														   _filterForFullyScheduledBlocks, _seniorityExtractor,
														   _filterOnSwapableTeamBlocks, _teamBlockLocatorWithHighestPoints,
														   _weekDayPointCalculatorForTeamBlock, _seniorityTeamBlockSwapper,
														   _teamBlockSeniorityValidator, _schedulingOptionsCreator);
			_juniorTeamBlock = _mocks.StrictMock<ITeamBlockInfo>();
			_seniorTeamBlock = _mocks.StrictMock<ITeamBlockInfo>();

			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrixList = new List<IScheduleMatrixPro> { _matrix1, _matrix2 };
			_schedulingOptions = new SchedulingOptions();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_optimizationPreferences = new OptimizationPreferences();
			_restrictionOverLimitValidator = _mocks.StrictMock<ITeamBlockRestrictionOverLimitValidator>();
			_selectedPersons = new List<IPerson>();
			_teamBlocksFirstLoop = new List<ITeamBlockInfo> { _juniorTeamBlock, _seniorTeamBlock };
			_teamBlocksScondLoop = new List<ITeamBlockInfo> { _juniorTeamBlock };
			_teamBlockPointsList = new List<ITeamBlockPoints>();
			_seniorityDic = new Dictionary<ITeamBlockInfo, int>();
			_seniorityDic.Add(_juniorTeamBlock, 1);
			_seniorityDic.Add(_seniorTeamBlock, 2);
			_teamBlockPoint1 = new TeamBlockPoints(_juniorTeamBlock, 2);
			_teamBlockPoint2 = new TeamBlockPoints(_seniorTeamBlock, 3);
			_teamBlockPointsList = new List<ITeamBlockPoints>{_teamBlockPoint1, _teamBlockPoint2};
			_weekDayValueDic = new Dictionary<DayOfWeek, int>();
			_seniorityValueDic = new Dictionary<ITeamBlockInfo, double>();
			_seniorTeamInfo = _mocks.StrictMock<ITeamInfo>();
			_juniorTeamInfo = _mocks.StrictMock<ITeamInfo>();

		}

		[Test]
		public void ShouldNotGoIntoEndlessLoop()
		{
			using (_mocks.Record())
			{
				commonMocks();

				//outer loop
				Expect.Call(_filterOnSwapableTeamBlocks.Filter(_teamBlocksFirstLoop, _seniorTeamBlock)).Return(_teamBlocksFirstLoop);
				Expect.Call(_weekDayPointCalculatorForTeamBlock.CalculateDaysOffSeniorityValue(_juniorTeamBlock, _weekDayValueDic))
				      .Return(15);
				Expect.Call(_weekDayPointCalculatorForTeamBlock.CalculateDaysOffSeniorityValue(_seniorTeamBlock, _weekDayValueDic))
					  .Return(10);

				//inner loop success
				Expect.Call(_teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(_teamBlocksFirstLoop,
																						   _seniorityValueDic))
				      .IgnoreArguments().Return(_juniorTeamBlock);
				Expect.Call(_seniorityTeamBlockSwapper.SwapAndValidate(_seniorTeamBlock, _juniorTeamBlock, _rollbackService,
				                                                       _scheduleDictionary, _optimizationPreferences,
				                                                       _restrictionOverLimitValidator)).Return(true);
				Expect.Call(_seniorTeamBlock.TeamInfo).Return(_seniorTeamInfo);
				Expect.Call(_seniorTeamInfo.Name).Return("Senior");

				//outer loop
				Expect.Call(_filterOnSwapableTeamBlocks.Filter(_teamBlocksScondLoop, _juniorTeamBlock)).Return(_teamBlocksScondLoop);
				Expect.Call(_weekDayPointCalculatorForTeamBlock.CalculateDaysOffSeniorityValue(_juniorTeamBlock, _weekDayValueDic))
					  .Return(10);

				//inner loop fail
				Expect.Call(_teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(_teamBlocksScondLoop,
																						   _seniorityValueDic))
					  .IgnoreArguments().Return(_juniorTeamBlock);
				Expect.Call(_juniorTeamBlock.TeamInfo).Return(_juniorTeamInfo);
				Expect.Call(_juniorTeamInfo.Name).Return("Junior");
			}

			using (_mocks.Playback())
			{
				_target.Execute(_matrixList, new DateOnlyPeriod(), _selectedPersons, _scheduleDictionary, _rollbackService,
				                _optimizationPreferences, _weekDayValueDic, _restrictionOverLimitValidator);
			}
		}

		[Test]
		public void ShouldRespondToCancel()
		{
			using (_mocks.Record())
			{
				commonMocks();

				//outer loop
				Expect.Call(_filterOnSwapableTeamBlocks.Filter(_teamBlocksFirstLoop, _seniorTeamBlock)).Return(_teamBlocksFirstLoop);
				Expect.Call(_weekDayPointCalculatorForTeamBlock.CalculateDaysOffSeniorityValue(_juniorTeamBlock, _weekDayValueDic))
					  .Return(15);
				Expect.Call(_weekDayPointCalculatorForTeamBlock.CalculateDaysOffSeniorityValue(_seniorTeamBlock, _weekDayValueDic))
					  .Return(10);

				//inner loop success
				Expect.Call(_teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(_teamBlocksFirstLoop,
																						   _seniorityValueDic))
					  .IgnoreArguments().Return(_juniorTeamBlock);
				Expect.Call(_seniorityTeamBlockSwapper.SwapAndValidate(_seniorTeamBlock, _juniorTeamBlock, _rollbackService,
																	   _scheduleDictionary, _optimizationPreferences,
																	   _restrictionOverLimitValidator)).Return(true);
				Expect.Call(_seniorTeamBlock.TeamInfo).Return(_seniorTeamInfo);
				Expect.Call(_seniorTeamInfo.Name).Return("Senior");

			}

			using (_mocks.Playback())
			{
				_target.BlockSwapped += _target_BlockSwapped;
				_target.Execute(_matrixList, new DateOnlyPeriod(), _selectedPersons, _scheduleDictionary, _rollbackService,
								_optimizationPreferences, _weekDayValueDic, _restrictionOverLimitValidator);
				_target.BlockSwapped -= _target_BlockSwapped;
			}
		}

		void _target_BlockSwapped(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = true;
		}

		private void commonMocks()
		{
			Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
			Expect.Call(_constructTeamBlock.Construct(_matrixList, new DateOnlyPeriod(), _selectedPersons, true,
			                                          BlockFinderType.SchedulePeriod,
			                                          _schedulingOptions.GroupOnGroupPageForTeamBlockPer))
			      .Return(_teamBlocksFirstLoop);
			Expect.Call(_filterForTeamBlockInSelection.Filter(_teamBlocksFirstLoop, _selectedPersons, new DateOnlyPeriod()))
			      .Return(_teamBlocksFirstLoop);
			Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_juniorTeamBlock)).Return(true);
			Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_seniorTeamBlock)).Return(true);
			Expect.Call(_filterForFullyScheduledBlocks.IsFullyScheduled(_teamBlocksFirstLoop, _scheduleDictionary));
			Expect.Call(_seniorityExtractor.ExtractSeniority(_teamBlocksFirstLoop)).IgnoreArguments().Return(_teamBlockPointsList);
		}
	}
}