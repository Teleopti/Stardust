using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	[TestFixture]
	public class TeamBlockDayOffDaySwapperTest
	{
		private MockRepository _mocks;
		private ITeamBlockDayOffDaySwapper _target;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _range1;
		private IScheduleRange _range2;
		private ISwapServiceNew _swapServiceNew;
		private IPerson _personSenior;
		private IPerson _personJunior;
		private Group _groupSenior;
		private Group _groupJunior;
		private IScheduleMatrixPro _matrixSenior;
		private TeamInfo _teamSenior;
		private TeamInfo _teamJunior;
		private TeamBlockInfo _teamBlockInfoSenior;
		private TeamBlockInfo _teamBlockInfoJunior;
		private DateOnly _dateOnly;
		private ITeamBlockDayOffDaySwapDecisionMaker _decisionMaker;
		private List<DateOnly> _dayOffsToGiveAway;
		private IOptimizationPreferences _optimizationPreferences;
		private DateOnly _dateBefore;
		private PossibleSwappableDays _daysToSwap;
	    private IPostSwapValidationForTeamBlock _postSwapValidationForTeamBlock;
		private ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private IProjectionService _projectionService1;
		private IProjectionService _projectionService2;
		private IVisualLayerCollection _visualLayerCollection1;
		private IVisualLayerCollection _visualLayerCollection2;
		private IDaysOffPreferences _daysOffPreferences;
		private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_swapServiceNew = _mocks.StrictMock<ISwapServiceNew>();
			_decisionMaker = _mocks.StrictMock<ITeamBlockDayOffDaySwapDecisionMaker>();
	        _postSwapValidationForTeamBlock = _mocks.StrictMock<IPostSwapValidationForTeamBlock>();
		    _teamBlockShiftCategoryLimitationValidator = _mocks.StrictMock<ITeamBlockShiftCategoryLimitationValidator>();
		    _target = new TeamBlockDayOffDaySwapper(_swapServiceNew, _decisionMaker, _postSwapValidationForTeamBlock,
		                                            _teamBlockShiftCategoryLimitationValidator);

			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_optimizationPreferences = _mocks.StrictMock<IOptimizationPreferences>();
			_personSenior = PersonFactory.CreatePerson();
			_personJunior = PersonFactory.CreatePerson();
			_groupSenior = new Group(new List<IPerson> { _personSenior }, "Senior");
			_groupJunior = new Group(new List<IPerson> { _personJunior }, "Junior");
			_matrixSenior = _mocks.StrictMock<IScheduleMatrixPro>();
			IList<IList<IScheduleMatrixPro>> groupMatrixesSenior = new List<IList<IScheduleMatrixPro>>();
			groupMatrixesSenior.Add(new List<IScheduleMatrixPro> { _matrixSenior });
			IList<IList<IScheduleMatrixPro>> groupMatrixesJunior = new List<IList<IScheduleMatrixPro>>();
			groupMatrixesJunior.Add(new List<IScheduleMatrixPro> { _matrixSenior });
			_teamSenior = new TeamInfo(_groupSenior, groupMatrixesSenior);
			_teamJunior = new TeamInfo(_groupJunior, groupMatrixesJunior);
			_dateOnly = new DateOnly(2014, 1, 27);
			_dateBefore = _dateOnly.AddDays(-1);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(_dateBefore, _dateOnly));
			_teamBlockInfoSenior = new TeamBlockInfo(_teamSenior, blockInfo);
			_teamBlockInfoJunior = new TeamBlockInfo(_teamJunior, blockInfo);
			_range1 = _mocks.StrictMock<IScheduleRange>();
			_range2 = _mocks.StrictMock<IScheduleRange>();

			_dayOffsToGiveAway = new List<DateOnly>{_dateBefore};
			_daysToSwap = new PossibleSwappableDays
			{
				DateForSeniorDayOff = _dateOnly,
				DateForRemovingSeniorDayOff = _dateBefore
			};

		    _projectionService1 = _mocks.StrictMock<IProjectionService>();
		    _projectionService2 = _mocks.StrictMock<IProjectionService>();
		    _visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
		    _visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();

			_daysOffPreferences = new DaysOffPreferences();
			_dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(_daysOffPreferences);
		}

		[Test]
		public void ShouldFailIfNoPossibleIndividualDaySwaps()
		{
			using (_mocks.Record())
			{
				Expect.Call(_decisionMaker.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _scheduleDictionary,
												  _optimizationPreferences, _dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider)).Return(null);
			    Expect.Call(()=> _rollbackService.ClearModificationCollection());
			}
			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _rollbackService,
											 _scheduleDictionary, _optimizationPreferences, _dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldSwapAccordingToDecisionMaker()
		{
			var seniorToHaveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>(); 
			var juniorToRemoveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>();
			var seniorGiveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>(); 
			var juniorAcceptDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>();
			var swapList1 = new List<IScheduleDay> { seniorToHaveDayOffScheduleDay, juniorToRemoveDayOffScheduleDay };
			var swapList2 = new List<IScheduleDay> { seniorGiveDayOffScheduleDay, juniorAcceptDayOffScheduleDay };
			var swapList = new List<IScheduleDay>();
			swapList.AddRange(swapList1);
			swapList.AddRange(swapList2);
			using (_mocks.Record())
			{
				commonMocks(seniorToHaveDayOffScheduleDay, juniorToRemoveDayOffScheduleDay, seniorGiveDayOffScheduleDay, juniorAcceptDayOffScheduleDay);
				Expect.Call(_swapServiceNew.Swap(new List<IScheduleDay> { seniorToHaveDayOffScheduleDay, juniorToRemoveDayOffScheduleDay }, _scheduleDictionary)).Return(swapList1);
				Expect.Call(_swapServiceNew.Swap(new List<IScheduleDay> { seniorGiveDayOffScheduleDay, juniorAcceptDayOffScheduleDay }, _scheduleDictionary)).Return(swapList2);
				Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
				Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection2);
				Expect.Call(_visualLayerCollection1.ContractTime()).Return(TimeSpan.FromHours(8));
				Expect.Call(_visualLayerCollection2.ContractTime()).Return(TimeSpan.FromHours(8));

				Expect.Call(() => _rollbackService.ClearModificationCollection()).Repeat.Twice();
				Expect.Call(_rollbackService.ModifyParts(swapList)).Return(new List<IBusinessRuleResponse>());
                Expect.Call(_postSwapValidationForTeamBlock.Validate(_teamBlockInfoSenior, _optimizationPreferences, _dayOffOptimizationPreferenceProvider))
                      .IgnoreArguments()
                      .Return(true);
                Expect.Call(_postSwapValidationForTeamBlock.Validate(_teamBlockInfoJunior, _optimizationPreferences, _dayOffOptimizationPreferenceProvider))
                          .IgnoreArguments()
                          .Return(true);
				Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfoSenior, _teamBlockInfoJunior,
																				_optimizationPreferences)).Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _rollbackService,
				                             _scheduleDictionary, _optimizationPreferences, _dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldRollbackIfBusinessRulesViolated()
		{
			
			var seniorToHaveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>(); 
			var juniorToRemoveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>();
			var seniorGiveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>(); 
			var juniorAcceptDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>();
			var swapList1 = new List<IScheduleDay> { seniorToHaveDayOffScheduleDay, juniorToRemoveDayOffScheduleDay };
			var swapList2 = new List<IScheduleDay> { seniorGiveDayOffScheduleDay, juniorAcceptDayOffScheduleDay };
			var swapList = new List<IScheduleDay>();
			swapList.AddRange(swapList1);
			swapList.AddRange(swapList2);
			IBusinessRuleResponse response = new BusinessRuleResponse(typeof(NewDayOffRule), "", true, false,
																	  new DateTimePeriod(), _personSenior,
																	  new DateOnlyPeriod(), "tjillevippen");
			
			using (_mocks.Record())
			{
				commonMocks(seniorToHaveDayOffScheduleDay, juniorToRemoveDayOffScheduleDay, seniorGiveDayOffScheduleDay, juniorAcceptDayOffScheduleDay);
				Expect.Call(_swapServiceNew.Swap(new List<IScheduleDay> { seniorToHaveDayOffScheduleDay, juniorToRemoveDayOffScheduleDay }, _scheduleDictionary)).Return(swapList1);
				Expect.Call(_swapServiceNew.Swap(new List<IScheduleDay> { seniorGiveDayOffScheduleDay, juniorAcceptDayOffScheduleDay }, _scheduleDictionary)).Return(swapList2);
				Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
				Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection2);
				Expect.Call(_visualLayerCollection1.ContractTime()).Return(TimeSpan.FromHours(8));
				Expect.Call(_visualLayerCollection2.ContractTime()).Return(TimeSpan.FromHours(8));

				Expect.Call(() => _rollbackService.ClearModificationCollection()).Repeat.AtLeastOnce();
				Expect.Call(_rollbackService.ModifyParts(swapList)).Return(new List<IBusinessRuleResponse> { response });
				Expect.Call(()=>_rollbackService.Rollback());
			}
			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _rollbackService,
				                             _scheduleDictionary, _optimizationPreferences, _dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldNotSwapWhenContractTimeDiffers()
		{
			var seniorToHaveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>();
			var juniorToRemoveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>();
			var seniorGiveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>();
			var juniorAcceptDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>();
			using (_mocks.Record())
			{
				commonMocks(seniorToHaveDayOffScheduleDay, juniorToRemoveDayOffScheduleDay, seniorGiveDayOffScheduleDay, juniorAcceptDayOffScheduleDay);
				Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
				Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection2);
				Expect.Call(_visualLayerCollection1.ContractTime()).Return(TimeSpan.FromHours(8));
				Expect.Call(_visualLayerCollection2.ContractTime()).Return(TimeSpan.FromHours(7));

				Expect.Call(() => _rollbackService.ClearModificationCollection());
				
			}
			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _rollbackService, 
											_scheduleDictionary, _optimizationPreferences, _dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
				Assert.IsFalse(result);
			}
		}

       

		private void commonMocks(IScheduleDay seniorToHaveDayOffScheduleDay,
		                         IScheduleDay juniorToRemoveDayOffScheduleDay, IScheduleDay seniorGiveDayOffScheduleDay,
		                         IScheduleDay juniorAcceptDayOffScheduleDay)
		{
			Expect.Call(_scheduleDictionary[_personSenior]).Return(_range1);
			Expect.Call(_scheduleDictionary[_personJunior]).Return(_range2);
			Expect.Call(_decisionMaker.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _scheduleDictionary,
			                                  _optimizationPreferences, _dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider)).Return(_daysToSwap);
			Expect.Call(_range1.ScheduledDay(_dateOnly)).Return(seniorToHaveDayOffScheduleDay);
			Expect.Call(_range2.ScheduledDay(_dateOnly)).Return(juniorToRemoveDayOffScheduleDay);
			Expect.Call(_range1.ScheduledDay(_dateBefore)).Return(seniorGiveDayOffScheduleDay);
			Expect.Call(_range2.ScheduledDay(_dateBefore)).Return(juniorAcceptDayOffScheduleDay);

			Expect.Call(seniorToHaveDayOffScheduleDay.ProjectionService()).Return(_projectionService1);
			Expect.Call(juniorAcceptDayOffScheduleDay.ProjectionService()).Return(_projectionService2);
		}
	}
}
