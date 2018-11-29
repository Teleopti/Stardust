using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class DayOffStep1Test
    {
        private MockRepository _mocks;
        private IConstructTeamBlock _constructTeamBlock;
        private IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
        private IFilterForFullyScheduledBlocks _filterForFullyScheduledBlocks;
        private ISeniorityExtractor _seniorityExtractor;
        private IFilterOnSwapableTeamBlocks _filterOnSwapableTeamBlocks;
        private ITeamBlockLocatorWithHighestPoints _teamBlockLocatorWithHighestPoints;
        private ISeniorityTeamBlockSwapper _seniorityTeamBlockSwapper;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
	    private ITeamBlockSeniorityValidator _teamBlockSeniorityValidator;
        private IDayOffStep1 _target;
        private ITeamBlockInfo _juniorTeamBlock;
        private ITeamBlockInfo _seniorTeamBlock;
        private IList<IScheduleMatrixPro> _matrixList;
        private SchedulingOptions _schedulingOptions;
        private IScheduleDictionary _scheduleDictionary;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IOptimizationPreferences _optimizationPreferences;
        //private ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
        private IList<IPerson> _selectedPersons;
        private List<ITeamBlockInfo> _teamBlocksFirstLoop;
        private IList<ITeamBlockInfo> _teamBlocksScondLoop;
        private ITeamBlockPoints _teamBlockPoint1;
        private ITeamBlockPoints _teamBlockPoint2;
        private IList<ITeamBlockPoints> _teamBlockPointsList;
        private IDictionary<ITeamBlockInfo, int> _seniorityDic;
        private IDictionary<DayOfWeek, int> _weekDayValueDic;
        private IDictionary<ITeamBlockInfo, double> _seniorityValueDic;
        private ITeamInfo _seniorTeamInfo;
        private ITeamInfo _juniorTeamInfo;
        private ISeniorTeamBlockLocator _seniorTeamBlockLocator;
        private ISeniorityCalculatorForTeamBlock _seniorityCalculatorForTeamBlock;
	    private IFilterForNoneLockedTeamBlocks _filterForNoneLockedTeamBlocks;
	    private IDaysOffPreferences _daysOffPreferences;
	    private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;


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
            _seniorityTeamBlockSwapper = _mocks.StrictMock<ISeniorityTeamBlockSwapper>();
            _seniorTeamBlockLocator = _mocks.StrictMock<ISeniorTeamBlockLocator>();
            _seniorityCalculatorForTeamBlock = _mocks.StrictMock<ISeniorityCalculatorForTeamBlock>();
	        _schedulingOptionsCreator = _mocks.StrictMock<ISchedulingOptionsCreator>();
	        _teamBlockSeniorityValidator = _mocks.StrictMock<ITeamBlockSeniorityValidator>();
	        _filterForNoneLockedTeamBlocks = _mocks.StrictMock<IFilterForNoneLockedTeamBlocks>();
	        _target = new DayOffStep1(_constructTeamBlock, _filterForTeamBlockInSelection, _filterForFullyScheduledBlocks,
	                                  _seniorityExtractor, _seniorTeamBlockLocator, _filterOnSwapableTeamBlocks,
	                                  _seniorityCalculatorForTeamBlock, _teamBlockLocatorWithHighestPoints,
	                                  _seniorityTeamBlockSwapper, _schedulingOptionsCreator, _teamBlockSeniorityValidator,
									  _filterForNoneLockedTeamBlocks);
            _juniorTeamBlock = _mocks.StrictMock<ITeamBlockInfo>();
            _seniorTeamBlock = _mocks.StrictMock<ITeamBlockInfo>();
            _matrixList = new List<IScheduleMatrixPro>();
            _schedulingOptions = new SchedulingOptions();
            _scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
            _optimizationPreferences = new OptimizationPreferences();
            //_restrictionOverLimitValidator = _mocks.StrictMock<ITeamBlockRestrictionOverLimitValidator>();
            _selectedPersons = new List<IPerson>();
            _teamBlocksFirstLoop = new List<ITeamBlockInfo> { _juniorTeamBlock, _seniorTeamBlock };
            _teamBlocksScondLoop = new List<ITeamBlockInfo> { _juniorTeamBlock };
            _teamBlockPointsList = new List<ITeamBlockPoints>();
            _seniorityDic = new Dictionary<ITeamBlockInfo, int>();
            _seniorityDic.Add(_juniorTeamBlock, 1);
            _seniorityDic.Add(_seniorTeamBlock, 2);
            _teamBlockPoint1 = new TeamBlockPoints(_juniorTeamBlock, 2);
            _teamBlockPoint2 = new TeamBlockPoints(_seniorTeamBlock, 3);
            _teamBlockPointsList = new List<ITeamBlockPoints> { _teamBlockPoint1, _teamBlockPoint2 };
            _weekDayValueDic = new Dictionary<DayOfWeek, int>();
            _seniorityValueDic = new Dictionary<ITeamBlockInfo, double>();
            _seniorTeamInfo = _mocks.StrictMock<ITeamInfo>();
            _juniorTeamInfo = _mocks.StrictMock<ITeamInfo>();
			_daysOffPreferences = new DaysOffPreferences();
			_dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(_daysOffPreferences);
        }

        [Test]
        public void ShouldNotGoIntoEndlessLoop()
        {
            IDictionary<ITeamBlockInfo, double> teamBlockPointsDic = new Dictionary<ITeamBlockInfo, double>();
            teamBlockPointsDic.Add(_seniorTeamBlock, 10);
            teamBlockPointsDic.Add(_juniorTeamBlock, 15);

            IDictionary<ITeamBlockInfo, double> singleJuniorList = new Dictionary<ITeamBlockInfo, double>();
            singleJuniorList.Add(_juniorTeamBlock,10);

            using (_mocks.Record())
            {
                commonMocks();

                //outer loop
                Expect.Call(_filterOnSwapableTeamBlocks.Filter(_teamBlocksFirstLoop, _seniorTeamBlock)).IgnoreArguments() .Return(_teamBlocksFirstLoop);
                Expect.Call(_seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(_teamBlocksFirstLoop,
                                                                                          _weekDayValueDic)).IgnoreArguments().Return(teamBlockPointsDic);

                //inner loop success
                Expect.Call(_teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(_teamBlocksFirstLoop,
                                                                                           _seniorityValueDic))
                      .IgnoreArguments().Return(_juniorTeamBlock);
                Expect.Call(_seniorityTeamBlockSwapper.SwapAndValidate(_seniorTeamBlock, _juniorTeamBlock, _rollbackService,
                                                                       _scheduleDictionary, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
                Expect.Call(_seniorTeamBlock.TeamInfo).Return(_seniorTeamInfo);
                Expect.Call(_seniorTeamInfo.Name).Return("Senior");

                //outer loop
                Expect.Call(_seniorTeamBlockLocator.FindMostSeniorTeamBlock(new List<ITeamBlockPoints>())).IgnoreArguments() .Return(_juniorTeamBlock);
                Expect.Call(_filterOnSwapableTeamBlocks.Filter(_teamBlocksScondLoop, _juniorTeamBlock)).IgnoreArguments().Return(_teamBlocksScondLoop);
                
                Expect.Call(_seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(_teamBlocksScondLoop,
                                                                                          _weekDayValueDic)).IgnoreArguments().Return(singleJuniorList);

                ////inner loop fail
                Expect.Call(_teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(_teamBlocksScondLoop,
                                                                                           _seniorityValueDic))
                      .IgnoreArguments().Return(_juniorTeamBlock);
                Expect.Call(_juniorTeamBlock.TeamInfo).Return(_juniorTeamInfo);
                Expect.Call(_juniorTeamInfo.Name).Return("Junior");
            }

            using (_mocks.Playback())
            {
                _target.PerformStep1(_matrixList, new DateOnlyPeriod(), _selectedPersons, _rollbackService, _scheduleDictionary,
                                _weekDayValueDic, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
            }
        }

        [Test]
        public void ShouldRespondToCancel()
        {
            IDictionary<ITeamBlockInfo, double> teamBlockPointsDic = new Dictionary<ITeamBlockInfo, double>();
            teamBlockPointsDic.Add(_seniorTeamBlock,10);
            teamBlockPointsDic.Add(_juniorTeamBlock ,15);
            using (_mocks.Record())
            {
                commonMocks();

                ////outer loop
                Expect.Call(_filterOnSwapableTeamBlocks.Filter(_teamBlocksFirstLoop, _seniorTeamBlock)).IgnoreArguments().Return(_teamBlocksFirstLoop);
                
                Expect.Call(_seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(_teamBlocksFirstLoop,
                                                                                          _weekDayValueDic)).IgnoreArguments().Return(teamBlockPointsDic);
                
                ////inner loop success
                Expect.Call(_teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(_teamBlocksFirstLoop,
                                                                                           _seniorityValueDic))
                      .IgnoreArguments().Return(_juniorTeamBlock);
                Expect.Call(_seniorityTeamBlockSwapper.SwapAndValidate(_seniorTeamBlock, _juniorTeamBlock, _rollbackService,
                                                                       _scheduleDictionary, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
                Expect.Call(_seniorTeamBlock.TeamInfo).Return(_seniorTeamInfo);
                Expect.Call(_seniorTeamInfo.Name).Return("Senior");

            }

            using (_mocks.Playback())
            {
                _target.BlockSwapped += _target_BlockSwapped;
                _target.PerformStep1(_matrixList, new DateOnlyPeriod(), _selectedPersons,  _rollbackService,_scheduleDictionary ,
                                _weekDayValueDic,_optimizationPreferences, _dayOffOptimizationPreferenceProvider);
                _target.BlockSwapped -= _target_BlockSwapped;
            }
        }

		[Test]
		public void ShouldUserCancel()
		{
			IDictionary<ITeamBlockInfo, double> teamBlockPointsDic = new Dictionary<ITeamBlockInfo, double>();
			teamBlockPointsDic.Add(_seniorTeamBlock, 10);
			teamBlockPointsDic.Add(_juniorTeamBlock, 15);
			using (_mocks.Record())
			{
				commonMocks();

				////outer loop
				Expect.Call(_filterOnSwapableTeamBlocks.Filter(_teamBlocksFirstLoop, _seniorTeamBlock)).IgnoreArguments().Return(_teamBlocksFirstLoop);
				Expect.Call(_seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(_teamBlocksFirstLoop,_weekDayValueDic)).IgnoreArguments().Return(teamBlockPointsDic);

				////inner loop success
				Expect.Call(_teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(_teamBlocksFirstLoop,_seniorityValueDic)).IgnoreArguments().Return(_juniorTeamBlock);
				Expect.Call(_seniorityTeamBlockSwapper.SwapAndValidate(_seniorTeamBlock, _juniorTeamBlock, _rollbackService,
																		_scheduleDictionary, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
				Expect.Call(_seniorTeamBlock.TeamInfo).Return(_seniorTeamInfo);
				Expect.Call(_seniorTeamInfo.Name).Return("Senior");

			}

			using (_mocks.Playback())
			{
				_target.BlockSwapped += _target_BlockSwapped2;
				_target.PerformStep1(_matrixList, new DateOnlyPeriod(), _selectedPersons, _rollbackService, 
									_scheduleDictionary,_weekDayValueDic, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
				_target.BlockSwapped -= _target_BlockSwapped2;
			}
		}

		[Test]
	    public void ShouldNotSwapBlockOfEqualSeniority()
	    {
			IDictionary<ITeamBlockInfo, double> teamBlockPointsDic = new Dictionary<ITeamBlockInfo, double>();
			teamBlockPointsDic.Add(_seniorTeamBlock, 10);
			teamBlockPointsDic.Add(_juniorTeamBlock, 10);
			using (_mocks.Record())
			{
				commonMocks();

				////outer loop
				Expect.Call(_filterOnSwapableTeamBlocks.Filter(_teamBlocksFirstLoop, _seniorTeamBlock)).IgnoreArguments().Return(_teamBlocksFirstLoop);
				Expect.Call(_seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(_teamBlocksFirstLoop, _weekDayValueDic)).IgnoreArguments().Return(teamBlockPointsDic);

				////inner loop success
				Expect.Call(_teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(_teamBlocksFirstLoop, _seniorityValueDic)).IgnoreArguments().Return(_juniorTeamBlock);
				Expect.Call(_seniorTeamBlock.TeamInfo).Return(_seniorTeamInfo);
				Expect.Call(_seniorTeamInfo.Name).Return("Senior");

			}

			using (_mocks.Playback())
			{
				_target.BlockSwapped += _target_BlockSwapped2;
				_target.PerformStep1(_matrixList, new DateOnlyPeriod(), _selectedPersons, _rollbackService, 
									_scheduleDictionary, _weekDayValueDic, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
				_target.BlockSwapped -= _target_BlockSwapped2;
			}    
	    }

        void _target_BlockSwapped(object sender, ResourceOptimizerProgressEventArgs e)
        {
            e.Cancel = true;
        }

		void _target_BlockSwapped2(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.CancelAction();
		}

        private void commonMocks()
        {
	        Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences))
	              .Return(_schedulingOptions);
            Expect.Call(_constructTeamBlock.Construct(_matrixList, new DateOnlyPeriod(), _selectedPersons,
                                                      new SchedulePeriodBlockFinder(), 
                                                      _schedulingOptions.GroupOnGroupPageForTeamBlockPer)).IgnoreArguments() 
                  .Return(_teamBlocksFirstLoop);
	        Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_juniorTeamBlock)).Return(true);
			Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_seniorTeamBlock)).Return(true);
            Expect.Call(_filterForTeamBlockInSelection.Filter(_teamBlocksFirstLoop, _selectedPersons, new DateOnlyPeriod()))
                  .Return(_teamBlocksFirstLoop);
            Expect.Call(_filterForFullyScheduledBlocks.Filter(_teamBlocksFirstLoop, _scheduleDictionary));
            Expect.Call(_seniorityExtractor.ExtractSeniority(_teamBlocksFirstLoop)).IgnoreArguments().Return(_teamBlockPointsList);
            Expect.Call(_seniorTeamBlockLocator.FindMostSeniorTeamBlock(_teamBlockPointsList.ToList())).Return(_seniorTeamBlock);
	        Expect.Call(_filterForNoneLockedTeamBlocks.Filter(_teamBlocksFirstLoop)).IgnoreArguments().Return(_teamBlocksFirstLoop).Repeat.AtLeastOnce();
        }
    }
}
