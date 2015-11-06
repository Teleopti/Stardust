using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
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
    public class DayOffStep2Test
    {
        private MockRepository _mock;
        private IDayOffStep2 _target;
        private ISeniorityExtractor _seniorityExtractor;
        private ISeniorTeamBlockLocator _seniorTeamBlockLocator;
        private IJuniorTeamBlockExtractor _juniorTeamBlockExtractor;
        private ISuitableDayOffSpotDetector _suitableDayOffSpotDetector;
        private IConstructTeamBlock _constructTeamBlock;
        private IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
        private IFilterForFullyScheduledBlocks _filterForFullyScheduledBlocks;
        private ITeamBlockDayOffDaySwapper _teamBlockDayOffSwapper;
        private IFilterOnSwapableTeamBlocks _filterOnSwapableTeamBlocks;
        private ISuitableDayOffsToGiveAway _suitableDayOffsToGiveAway;
        private IList<IScheduleMatrixPro> _allPersonMatrixList;
        private DateOnlyPeriod _selectedPeriod;
        private IList<IPerson> _selectedPersons;
        private GroupPageLight _groupPageLight;
        private ITeamBlockInfo _seniorTeamBlock;
        private ITeamBlockInfo _juniorTeamBlock;
        private ITeamBlockPoints _seniorTeamBlockPoint;
        private ITeamBlockPoints _juniorTeamBlockPoint;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IScheduleDictionary _schedulingDictionary;
        private IOptimizationPreferences _optimizationPreferences;
        private IWeekDayPoints _weekDayPoints;
        private ITeamInfo _teamInfo;
        private ITeamBlockSeniorityValidator _teamBlockSeniorityValidator;
	    private ISeniorityWorkDayRanks _seniorityWorkDayRanks;
	    private IDaysOffPreferences _daysOffPreferences;
	    private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _seniorityExtractor = _mock.StrictMock<ISeniorityExtractor>();
            _seniorTeamBlockLocator = _mock.StrictMock<ISeniorTeamBlockLocator>();
            _juniorTeamBlockExtractor = _mock.StrictMock<IJuniorTeamBlockExtractor>();
            _suitableDayOffSpotDetector = _mock.StrictMock<ISuitableDayOffSpotDetector>();
            _constructTeamBlock = _mock.StrictMock<IConstructTeamBlock>();
            _filterForTeamBlockInSelection = _mock.StrictMock<IFilterForTeamBlockInSelection>();
            _filterForFullyScheduledBlocks = _mock.StrictMock<IFilterForFullyScheduledBlocks>();
            _teamBlockDayOffSwapper = _mock.StrictMock<ITeamBlockDayOffDaySwapper>();
            _filterOnSwapableTeamBlocks = _mock.StrictMock<IFilterOnSwapableTeamBlocks>();
            _suitableDayOffsToGiveAway = _mock.StrictMock<ISuitableDayOffsToGiveAway>();
            _groupPageLight = new GroupPageLight();
            _seniorTeamBlock = _mock.StrictMock<ITeamBlockInfo>();
            _juniorTeamBlock = _mock.StrictMock<ITeamBlockInfo>();
            _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _schedulingDictionary = _mock.StrictMock<IScheduleDictionary>();
            _optimizationPreferences = _mock.StrictMock<IOptimizationPreferences>();
            _teamInfo = _mock.StrictMock<ITeamInfo>();
            _teamBlockSeniorityValidator = _mock.StrictMock<ITeamBlockSeniorityValidator>();
            _allPersonMatrixList = new List<IScheduleMatrixPro>();
            _selectedPeriod = new DateOnlyPeriod(2014,02,11,2014,02,11);
            _selectedPersons = new List<IPerson>();
            _seniorTeamBlockPoint = new TeamBlockPoints(_seniorTeamBlock,27);
            _juniorTeamBlockPoint = new TeamBlockPoints(_juniorTeamBlock, 15);
            _weekDayPoints = new WeekDayPoints();
			_seniorityWorkDayRanks = new SeniorityWorkDayRanks();
			_daysOffPreferences = new DaysOffPreferences();
			_dayOffOptimizationPreferenceProvider = new DayOffOptimizationPreferenceProvider(_daysOffPreferences);

            _target = new DayOffStep2(_seniorityExtractor,_seniorTeamBlockLocator,_juniorTeamBlockExtractor,_suitableDayOffSpotDetector,
                        _constructTeamBlock, _filterForTeamBlockInSelection, _filterForFullyScheduledBlocks, _filterOnSwapableTeamBlocks, _teamBlockDayOffSwapper, _suitableDayOffsToGiveAway,_teamBlockSeniorityValidator);
        }

       

        [Test]
        public void ShouldRunStep1WithOnlyOneElement()
        {
            IList<ITeamBlockInfo> teamBlockList = new List<ITeamBlockInfo>(){_seniorTeamBlock};
            IList<ITeamBlockPoints> teamBlockPointList = new List<ITeamBlockPoints>()
                {
                    _seniorTeamBlockPoint
                };
            ISchedulingOptions schedulingOptions = new SchedulingOptions();
            using (_mock.Record())
            {
                
                commonMocks(teamBlockList, teamBlockPointList);
                Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_seniorTeamBlock)).Return(true);
            }

            using (_mock.Playback())
            {

				_target.PerformStep2(schedulingOptions, _allPersonMatrixList, _selectedPeriod, _selectedPersons, _rollbackService, _schedulingDictionary, 
									_weekDayPoints.GetWeekDaysPoints(_seniorityWorkDayRanks), _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
            }
        }

        private void commonMocks(IList<ITeamBlockInfo> teamBlockList, IList<ITeamBlockPoints> teamBlockPointList)
        {
            Expect.Call(_constructTeamBlock.Construct(_allPersonMatrixList, _selectedPeriod, _selectedPersons,
                                                      BlockFinderType.SchedulePeriod, _groupPageLight))
                  .IgnoreArguments()
                  .Return(teamBlockList);
            Expect.Call(_filterForTeamBlockInSelection.Filter(teamBlockList, _selectedPersons, _selectedPeriod))
                  .IgnoreArguments()
                  .Return(teamBlockList);
            Expect.Call(_filterForFullyScheduledBlocks.Filter(teamBlockList, null)).IgnoreArguments().Return(teamBlockList);
            
            Expect.Call(_seniorityExtractor.ExtractSeniority(teamBlockList)).Return(teamBlockPointList);

            //first level
            Expect.Call(_seniorTeamBlockLocator.FindMostSeniorTeamBlock(teamBlockPointList))
                  .IgnoreArguments()
                  .Return(_seniorTeamBlock);
        }

        [Test]
        public void ShouldRunStep1WithOnlyTwoElements()
        {
            IList<ITeamBlockInfo> teamBlockList = new List<ITeamBlockInfo>() { _seniorTeamBlock,_juniorTeamBlock };
            IList<ITeamBlockPoints> teamBlockPointList = new List<ITeamBlockPoints>(){_seniorTeamBlockPoint,_juniorTeamBlockPoint };
            ISchedulingOptions schedulingOptions = new SchedulingOptions();
            using (_mock.Record())
            {
                Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_juniorTeamBlock)).Return(true);
                commonMocks(teamBlockList, teamBlockPointList);
                //Second level
                Expect.Call(_filterOnSwapableTeamBlocks.Filter(teamBlockList, _seniorTeamBlock)).IgnoreArguments().Return(teamBlockList);
                Expect.Call(_seniorityExtractor.ExtractSeniority(teamBlockList)).IgnoreArguments().Return(teamBlockPointList);
                Expect.Call(_juniorTeamBlockExtractor.GetJuniorTeamBlockInfo(teamBlockPointList.ToList())).IgnoreArguments().Return(_juniorTeamBlock);
                Expect.Call(_seniorTeamBlock.TeamInfo).Return(_teamInfo).Repeat.AtLeastOnce()  ;
				Expect.Call(_teamInfo.Name).Return("senior team").Repeat.AtLeastOnce() ;

                //third level
				Expect.Call(_suitableDayOffSpotDetector.DetectMostValuableSpot(_selectedPeriod.DayCollection(), _weekDayPoints.GetWeekDaysPoints(_seniorityWorkDayRanks))).IgnoreArguments().Return(new DateOnly(2014, 02, 11));
				Expect.Call(_suitableDayOffsToGiveAway.DetectMostValuableSpot(_selectedPeriod.DayCollection(), _weekDayPoints.GetWeekDaysPoints(_seniorityWorkDayRanks))).Return(_selectedPeriod.DayCollection());
                Expect.Call(_teamBlockDayOffSwapper.TrySwap(DateOnly.Today, _seniorTeamBlock, _juniorTeamBlock,_rollbackService, _schedulingDictionary,
															_optimizationPreferences, _selectedPeriod.DayCollection(), _dayOffOptimizationPreferenceProvider)).IgnoreArguments().Return(true);
               
				Expect.Call(_juniorTeamBlockExtractor.GetJuniorTeamBlockInfo(teamBlockPointList.ToList())).IgnoreArguments().Return(_seniorTeamBlock);
               

                //Step 2
                Expect.Call(_seniorTeamBlockLocator.FindMostSeniorTeamBlock(teamBlockPointList))
                      .IgnoreArguments()
                      .Return(_juniorTeamBlock);
				
                teamBlockList.Remove(_seniorTeamBlock);
				
            }

            using (_mock.Playback())
            {

				_target.PerformStep2(schedulingOptions, _allPersonMatrixList, _selectedPeriod, _selectedPersons, _rollbackService, _schedulingDictionary, 
									_weekDayPoints.GetWeekDaysPoints(_seniorityWorkDayRanks), _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
            }
        }


		[Test]
		public void ShouldUserCancel()
		{
			IList<ITeamBlockInfo> teamBlockList = new List<ITeamBlockInfo> { _seniorTeamBlock, _juniorTeamBlock };
			IList<ITeamBlockPoints> teamBlockPointList = new List<ITeamBlockPoints> {_seniorTeamBlockPoint,_juniorTeamBlockPoint};
			ISchedulingOptions schedulingOptions = new SchedulingOptions();
			using (_mock.Record())
			{
				Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_juniorTeamBlock)).Return(true);
				commonMocks(teamBlockList, teamBlockPointList);
				//Second level
				Expect.Call(_filterOnSwapableTeamBlocks.Filter(teamBlockList, _seniorTeamBlock)).IgnoreArguments().Return(teamBlockList);
				Expect.Call(_seniorityExtractor.ExtractSeniority(teamBlockList)).IgnoreArguments().Return(teamBlockPointList);
				Expect.Call(_juniorTeamBlockExtractor.GetJuniorTeamBlockInfo(teamBlockPointList.ToList())).IgnoreArguments().Return(_juniorTeamBlock);
				Expect.Call(_seniorTeamBlock.TeamInfo).Return(_teamInfo).Repeat.AtLeastOnce();
				
				Expect.Call(_teamInfo.Name).Return("senior team").Repeat.AtLeastOnce();

				//third level
				Expect.Call(_suitableDayOffSpotDetector.DetectMostValuableSpot(_selectedPeriod.DayCollection(), _weekDayPoints.GetWeekDaysPoints(_seniorityWorkDayRanks))).IgnoreArguments().Return(new DateOnly(2014, 02, 11));
				Expect.Call(_suitableDayOffsToGiveAway.DetectMostValuableSpot(_selectedPeriod.DayCollection(), _weekDayPoints.GetWeekDaysPoints(_seniorityWorkDayRanks))).Return(_selectedPeriod.DayCollection());
				Expect.Call(_teamBlockDayOffSwapper.TrySwap(DateOnly.Today, _seniorTeamBlock, _juniorTeamBlock, _rollbackService, _schedulingDictionary,
															_optimizationPreferences, _selectedPeriod.DayCollection(), _dayOffOptimizationPreferenceProvider)).IgnoreArguments().Return(true);
				
				teamBlockList.Remove(_seniorTeamBlock);
				Expect.Call(_teamBlockDayOffSwapper.Cancel).Repeat.Any();
			}

			using (_mock.Playback())
			{
				_target.BlockSwapped += targetReportProgress;
				_target.PerformStep2(schedulingOptions, _allPersonMatrixList, _selectedPeriod, _selectedPersons, _rollbackService, _schedulingDictionary, 
									_weekDayPoints.GetWeekDaysPoints(_seniorityWorkDayRanks), _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
				_target.BlockSwapped -= targetReportProgress;
			}
		}

		[Test]
	    public void ShouldNotSwapBlockOfEqualSeniority()
		{
			_seniorTeamBlockPoint = new TeamBlockPoints(_seniorTeamBlock, 15);
			_juniorTeamBlockPoint = new TeamBlockPoints(_juniorTeamBlock, 15);

			IList<ITeamBlockInfo> teamBlockList = new List<ITeamBlockInfo>() { _seniorTeamBlock, _juniorTeamBlock };
			IList<ITeamBlockPoints> teamBlockPointList = new List<ITeamBlockPoints>() { _seniorTeamBlockPoint, _juniorTeamBlockPoint };
			ISchedulingOptions schedulingOptions = new SchedulingOptions();
			using (_mock.Record())
			{
				Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_juniorTeamBlock)).Return(true);
				commonMocks(teamBlockList, teamBlockPointList);
				Expect.Call(_seniorTeamBlockLocator.FindMostSeniorTeamBlock(teamBlockPointList)).IgnoreArguments().Return(_juniorTeamBlock);	
				teamBlockList.Remove(_seniorTeamBlock);
			}

			using (_mock.Playback())
			{
				_target.BlockSwapped += targetReportProgress;
				_target.PerformStep2(schedulingOptions, _allPersonMatrixList, _selectedPeriod, _selectedPersons, _rollbackService, _schedulingDictionary, 
									_weekDayPoints.GetWeekDaysPoints(_seniorityWorkDayRanks), _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
				_target.BlockSwapped -= targetReportProgress;
			}
	    }

		void targetReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.CancelAction();
		}
    }
}
